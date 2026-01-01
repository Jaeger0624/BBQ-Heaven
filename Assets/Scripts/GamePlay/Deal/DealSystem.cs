using QFramework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// 交易系统 - 系统层
/// 用于处理交易相关的逻辑（烧烤 + 顾客 = 交易）
/// 也相当于是BBQ System和Customer System的中间层
/// </summary>
public interface IDealSystem : ISystem{
    // 执行交易
	void ExecuteDeal(BBQ bbq, Customer customer);
	// 仅计算不落账（用于预览）
	DealResult PreviewDeal(DealContext context);
    // 读取当前正在处理的交易（供 GA 等效果访问）
    Deal GetCurrentDeal();

	void AddScoreMultiplier(string name, float multiplier);
	void RemoveScoreMultiplier(string name);
}


public class DealSystem : AbstractSystem, IDealSystem
{
    private Deal currentDeal = null;
	private IEarnMoneyStrategy earnMoneyStrategy = new EarnMoneyStrategy_原值();
    public Deal GetCurrentDeal() => currentDeal;
	private Dictionary<string, float> scoreMultipliers = new Dictionary<string, float>();

    protected override void OnInit()
	{
		scoreMultipliers = new Dictionary<string, float>();
	}
	public void AddScoreMultiplier(string name, float multiplier)
	{
		if (!scoreMultipliers.ContainsKey(name)){
			scoreMultipliers.Add(name, multiplier);
			Debug.Log($"【DealSystem】添加得分乘区: {name}, 乘区: {multiplier}");
		}
	}
	public void RemoveScoreMultiplier(string name)
	{
		if (scoreMultipliers.ContainsKey(name)){
			scoreMultipliers.Remove(name);
			Debug.Log($"【DealSystem】移除得分乘区: {name}");
		}
		else{
			Debug.LogError($"【DealSystem】尝试移除不存在的得分乘区: {name}");
		}
	}
	private void ResetSatisfaction(){	
        // 暴露给GA的满意度计算
        CustomerSatisfaction currentSatisfaction = this.GetSystem<ICustomerSystem>().Satisfaction;
        if (currentSatisfaction != null) Debug.LogError("当前满意度应当为空");
        this.GetSystem<ICustomerSystem>().Satisfaction = new CustomerSatisfaction();
	}
	public void ExecuteDeal(BBQ bbq, Customer customer)
    {
		if (bbq == null || customer == null) return;
		// 设置当前交易并派发开始事件
		currentDeal = new Deal(bbq, customer);
		this.SendEvent(new DealStartedEvent(bbq, customer));

		// 开启满意度条
		this.GetSystem<IAnimationSystem>().Append(new SequenceAnimTask(new List<IAnimTask>{
			new ActionAnimTask(() => this.SendEvent(new ShowStatisBarEvent())),
			new DelayAnimTask(0.5f, true),
		}));

		// 重置满意度
		ResetSatisfaction();

		// 创建上下文
		DealContext context = new DealContext(bbq, customer, this.GetSystem<ICustomerSystem>().Satisfaction);

		// 进行满意度的处理计算
		DealResult result = CalculateInternal(context, false);
				
		Debug.Log(result.DealInfo());

		// 将得分转换
		// 结算落账（现金）
		this.GetSystem<IScoreSystem>().ChangeScore(result.price);


		// 关闭满意度条
		this.GetSystem<IAnimationSystem>().Append(new SequenceAnimTask(new List<IAnimTask>{
			new DelayAnimTask(0.5f, true),
			new ActionAnimTask(() => this.SendEvent(new HideStatisBarEvent())),
		}));
		
		// 消耗资源：食材实例彻底移除
		foreach (var food in bbq.foodInstances)
		{
			this.GetSystem<IFoodSystem>().RemoveFoodInstance(food.guid);
		}

		// 交易完成事件（把结果发出去，让DealController自动处理成动画）
		this.SendEvent(new DealCompletedEvent(result));

		// 清理当前交易
		currentDeal = null;

		// 顾客离开（服务完成）
		this.GetSystem<ICustomerSystem>().LeaveCustomer(new List<Customer>{customer});

		// 耗时
		int timePoint = SettingManager.GetSetting<GameplaySettings>().soldBBQTime_默认;
		this.GetSystem<ITimeSystem>().PushTimePoint(timePoint);

		// 归还烤串
		Stick stick = bbq.stick;
		stick.TryReturnStick();
    }

	public DealResult PreviewDeal(DealContext context)
	{
		if (context == null) return null;
		return CalculateInternal(context, true);
	}

	private DealResult CalculateInternal(DealContext context, bool isPreview)    
	{
        
		// 获取满意度（含顾客身上 Tag 的效果执行）
		if (!isPreview) Debug.Log($"【DealSystem】开始计算满意度：顾客:{context.Customer.name}");

		// 最终满意度总乘区
		(float satisfaction, CustomerSatisfaction Satis) = GetSatisfaction(context, isPreview);

		// 定价：满意度乘区 × 珍稀度 × 美味度（耐心作为可选修正，不在基础公式中）
		int rarity = context.BBQ.totalRarity.Value;
		int taste = context.BBQ.totalTaste.Value;
		float rawPrice = satisfaction * rarity * taste;

		// 获取其他得分乘区
		Dictionary<string, float> otherMultipliers = scoreMultipliers;

		foreach (var multiplier in otherMultipliers){
			rawPrice *= multiplier.Value;
		}
		int roundedRawPrice = Mathf.RoundToInt(rawPrice);

		// 计算价格
		int money = earnMoneyStrategy.EarnMoney(roundedRawPrice);


		DealResult result = new DealResult(context.BBQ, context.Customer, satisfaction, Satis, otherMultipliers, rarity, taste, money, rawPrice);

		// 基础计算完成：派发事件，允许 GA 基于当前交易对结果进行调整
		return result;
	}

	public void SetEarnMoneyStrategy(IEarnMoneyStrategy earnMoneyStrategy){
		this.earnMoneyStrategy = earnMoneyStrategy;
	}


    //TODO: 此处安插公式计算顾客满意度
    private (float satisfaction, CustomerSatisfaction satisfactionSystem) GetSatisfaction(
        DealContext context,
        bool isPreview)
		{

		List<ISatisCreater> satisCreaters = new List<ISatisCreater>(){};

		// 1. 执行满意度（内会执行GA）
		satisCreaters.ForEach(x => x.Calculate(context));

		// 2. 添加动画任务（偏好）
		satisCreaters.ForEach(x => this.GetSystem<IAnimationSystem>().Append(x.GetAnimTask()));
		this.GetSystem<IAnimationSystem>().Play();

		// 3. 显示标签视图
		bool hasAnyTagTriggered = context.Customer.customerTags.Any(tag => tag.Preview(context).Any(x => x));
		if (hasAnyTagTriggered) this.GetSystem<IAnimationSystem>().Append(AnimCombine_顾客Tag.Anim_顾客Tag_显示标签视图());

		context.Customer.customerTags.ForEach(tag => {
			// 4. 内部顺序显示Tag结算与CGA效果
			tag.Execute(context);
		});

		// 5. 隐藏标签视图
		if (hasAnyTagTriggered){	
			this.GetSystem<IAnimationSystem>().AddDelay(0.4f, true);
			this.GetSystem<IAnimationSystem>().Append(AnimCombine_顾客Tag.Anim_顾客Tag_隐藏标签视图());
		}

        // 取回当前值后，设置为空
        float satisfaction = context.Satisfaction.GetFinal();

        // 获取并传出，随后设置为空
        CustomerSatisfaction cur = context.Satisfaction;

		// 设置满意度为空
		this.GetSystem<ICustomerSystem>().Satisfaction = null;
        return (satisfaction, cur);
    }
}

#region 交易事件
public class DealStartedEvent : AbstractEvent{
	public BBQ bbq;
	public Customer customer;
	public DealStartedEvent(BBQ bbq, Customer customer){
		this.bbq = bbq;
		this.customer = customer;
	}
}
public class DealCompletedEvent : AbstractEvent{
	public DealResult result;
	public DealCompletedEvent(DealResult result){
		this.result = result;
	}
}


#endregion
