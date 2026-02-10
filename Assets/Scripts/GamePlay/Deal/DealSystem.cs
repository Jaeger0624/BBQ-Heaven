using QFramework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UniRx;
using cfg;


/// <summary>
/// 交易系统 - 系统层
/// 用于处理交易相关的逻辑（烧烤 + 顾客 = 交易）
/// 也相当于是BBQ System和Customer System的中间层
/// </summary>
public interface IDealSystem : ISystem{
    // 执行交易
	void ExecuteDeal(BBQ bbq, Customer customer);
	DealResult ExecuteTempDeal(BBQResultTemp resultTemp, Customer customer);
	void AddScoreMultiplier(string name, float multiplier);
	void RemoveScoreMultiplier(string name);
}


public class DealSystem : AbstractSystem, IDealSystem
{

	private IEarnMoneyStrategy earnMoneyStrategy = new EarnMoneyStrategy_原值();
	private Dictionary<string, float> scoreMultipliers = new Dictionary<string, float>();

    protected override void OnInit()
	{
		scoreMultipliers = new Dictionary<string, float>();
		this.RegisterEvent<TimeUpEvent>(OnTimeUpEvent);
		this.RegisterEvent<EndDayEvent>(OnEndDayEvent);
	}
	protected override void OnDeinit()
	{
		this.UnRegisterEvent<TimeUpEvent>(OnTimeUpEvent);
		this.UnRegisterEvent<EndDayEvent>(OnEndDayEvent);
	}
	private void OnTimeUpEvent(TimeUpEvent evt)
	{
		AddScoreMultiplier("疲劳", 0.5f);
	}
	private void OnEndDayEvent(EndDayEvent evt)
	{
		if (!evt.StageMeet(EventStage.System)) return;
		// 清除所有得分乘区
		ClearScoreMultipliers();
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
	private void ClearScoreMultipliers()
	{
		scoreMultipliers.Clear();
	}

	private void ResetSatisfaction(){	
        // 暴露给GA的满意度计算
        CustomerSatisfaction currentSatisfaction = this.GetSystem<ICustomerSystem>().Satisfaction;
        if (currentSatisfaction != null) Debug.LogError("当前满意度应当为空");
        this.GetSystem<ICustomerSystem>().Satisfaction = new CustomerSatisfaction();
	}

	public async void ExecuteDeal(BBQ bbq, Customer customer)
    {
		if (bbq == null || customer == null) return;
		this.SendEvent(new DealStartedEvent(bbq, customer));

		// 重置满意度
		ResetSatisfaction();
		customer.isServed = true;

		// 创建上下文
		DealContext context = new DealContext(bbq, customer, this.GetSystem<ICustomerSystem>().Satisfaction);

		// 触发顾客订单进行时动作（如“讨价还价”等）
		IObservable<Unit> encounterObservable = this.GetSystem<ICustomerSystem>().CustomerActionHandler.HandleCustomerAction(new List<Customer>{customer}, CustomerActionType.订单进行时, new List<object>{bbq, context});

		// 等待顾客订单进行时动作完成
		await encounterObservable;

		int rarity = bbq.totalRarity.Value;
		int taste = bbq.totalTaste.Value;
		Color rarityColor = SettingManager.Instance.DevSettings.AddRarityTextColor;
		Color tasteColor = SettingManager.Instance.DevSettings.AddTasteTextColor;
		float scoreTextAnimDuration = SettingManager.Instance.AnimSettings.ScoreTextAnimDuration;
		// 乘上基本得分
		List<IAnimTask> animTasks = new List<IAnimTask>(){
			new ActionAnimTask(() => this.SendEvent(new ShowScorePanelEvent())),
			new DelayAnimTask(0.1f, true),
			new ActionAnimTask(() => this.SendEvent(new ShowScoreTextEvent($"{rarity.ToString().ToColor(rarityColor)} x {taste.ToString().ToColor(tasteColor)}"))),
			new DelayAnimTask(scoreTextAnimDuration + 0.2f, true),
			// new ActionAnimTask(() => this.SendEvent(new UpdateScoreViewEvent(rarity))),
			// new DelayAnimTask(0.6f, true),
			new ActionAnimTask(() => this.SendEvent(new UpdateScoreViewEvent(taste * rarity))),
			new DelayAnimTask(scoreTextAnimDuration + 0.2f, true),
			// new ActionAnimTask(() => this.SendEvent(new CloseScorePanelEvent())),
		};

		IAnimTask animTask = new SequenceAnimTask(animTasks);
		this.GetSystem<IAnimationSystem>().Append(animTask);

		// 进行满意度的处理计算
		DealResult result = GetResult(context);
				
		Debug.Log(result.DealInfo());

		AfterDeal(result, context);
    }

	private DealResult GetResult(DealContext context)    
	{
		// 获得顾客评价
		ReviewResult reviewResult = context.Customer.Review(context);
		Debug.Log($"【DealSystem -- 顾客评价】\n{reviewResult.ReviewInfo()}");

		// 获取满意度（含顾客身上 Tag 的效果执行）
		Debug.Log($"【DealSystem】开始计算满意度：顾客:{context.Customer.name}");
		List<ScoreRecord> scoreRecords = new List<ScoreRecord>();
		// 最终满意度总乘区
		(float satisfaction, CustomerSatisfaction Satis) = GetSatisfaction(context);

		// 定价：满意度乘区 × 珍稀度 × 美味度（耐心作为可选修正，不在基础公式中）
		int rarity = context.BBQ.totalRarity.Value;
		int taste = context.BBQ.totalTaste.Value;
		float rawPrice = satisfaction * rarity * taste;

		scoreRecords.Add(new ScoreRecord("满意度", satisfaction, rawPrice));

		// 获取其他得分乘区
		Dictionary<string, float> otherMultipliers = new();

		// 1. 添加得分乘区
		foreach (var multiplier in scoreMultipliers){
			otherMultipliers.Add(multiplier.Key, multiplier.Value);
		}
		// 2. 添加其他得分乘区
		foreach (var multiplier in context.OtherMultipliers){
			otherMultipliers.Add(multiplier.Key, multiplier.Value);
		}

		// 执行乘区计算
		foreach (var multiplier in otherMultipliers){
			rawPrice *= multiplier.Value;
			scoreRecords.Add(new ScoreRecord(multiplier.Key, multiplier.Value, rawPrice));
		}

		// 星级计算
		foreach (var record in reviewResult.Records){
			if (record.IsMet){
				if (record.StarValue <= 2){
					Debug.LogError($"【DealSystem】星级计算至少要是3星，否则无效，错误记录：{record.Description}");
					continue;
				}

				// 根据奖励类型计算
				switch (record.Award.AwardType){
					case RequirementAwardType.倍率:
						rawPrice *= record.Award.AwardValue;
						break;
					default:
						Debug.LogError($"【DealSystem】未知奖励类型: {record.Award.AwardType}");
						break;
				}
				scoreRecords.Add(new ScoreRecord(record.Description, record.Award.AwardValue, rawPrice));
			}
		}

		int roundedRawPrice = Mathf.RoundToInt(rawPrice);
		// 计算价格
		int money = earnMoneyStrategy.EarnMoney(roundedRawPrice);

		DealResult result = new DealResult(context.BBQ, context.Customer, satisfaction, Satis, otherMultipliers, rarity, taste, money, rawPrice, scoreRecords);


		// 基础计算完成：派发事件，允许 GA 基于当前交易对结果进行调整
		return result;
	}



    //TODO: 此处安插公式计算顾客满意度
    private (float satisfaction, CustomerSatisfaction satisfactionSystem) GetSatisfaction(DealContext context)
	{

		// 1. 检测是否有标签满足条件
		bool hasAnyTagTriggered = context.Customer.meta.customerTags.Any(tag => tag.Preview(context).Any(x => x));
		if (hasAnyTagTriggered) this.GetSystem<IAnimationSystem>().Append(AnimCombine_顾客Tag.Anim_顾客Tag_显示标签视图());

		// 2. 执行标签
		context.Customer.meta.customerTags.ForEach(tag => {
			// 2.1 内部顺序显示Tag结算与CGA效果
			tag.Execute(context);
		});

		// 2.2 隐藏标签视图
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


	private void AfterDeal(DealResult result, DealContext context){
		float scoreTextAnimDuration = SettingManager.Instance.AnimSettings.ScoreTextAnimDuration;
		List<ScoreRecord> scoreRecords = result.scoreRecords;
		List<IAnimTask> animTasks = new List<IAnimTask>();
		foreach (var record in scoreRecords){
			
			// 更新得分面板
			string multiplierText = $"<size=36><color=yellow>{record.name}</color></size>\n<size=56>{record.multiplier}x</size>";
			animTasks.Add(new ActionAnimTask(() => this.SendEvent(new UpdateScoreViewEvent(Mathf.RoundToInt(record.currentScore), multiplierText))));
			animTasks.Add(new DelayAnimTask(scoreTextAnimDuration + 0.2f, true));
		}
		animTasks.Add(new DelayAnimTask(0.5f, true));
		animTasks.Add(new ActionAnimTask(() => this.SendEvent(new CloseScorePanelEvent())));

		this.GetSystem<IAnimationSystem>().Append(new SequenceAnimTask(animTasks));

		// 将得分转换
		// 结算落账（现金）
		this.GetSystem<IScoreSystem>().ChangeScore(result.price);

		// 增加顾客的声望值
		this.GetSystem<IPCSystem>().AddReputation(context.Customer.reputation);
		
		// 消耗资源：食材实例彻底移除
		foreach (var food in context.BBQ.foodInstances)
		{
			this.GetSystem<IFoodSystem>().RemoveFoodInstance(food.guid);
		}

		// 交易完成事件（把结果发出去，让DealController自动处理成动画）
		this.SendEvent(new DealCompletedEvent(result));

		// 触发顾客订单完成后动作
		this.GetSystem<ICustomerSystem>().CustomerActionHandler.HandleCustomerAction(new List<Customer>{context.Customer}, CustomerActionType.订单完成后, new List<object>{result, context});

		// 记录顾客服务次数
		this.GetSystem<ICustomerSystem>().Recorder.OnServeCustomer(context, result);

		// 顾客离开（服务完成）
		this.GetSystem<ICustomerSystem>().LeaveCustomer(new List<Customer>{context.Customer});

		// 归还烤串
		Stick stick = context.BBQ.stick;
		stick.TryReturnStick();
	}

	
	// 不引入Satisfaction的临时交易（同时也不触发食材效果，只是简单的加值计算）
	public DealResult ExecuteTempDeal(BBQResultTemp resultTemp, Customer customer){
		if (resultTemp == null || customer == null) return null;
		BBQ bbq = new BBQ(resultTemp.stick, resultTemp.foodInstances);

		DealContext context = new DealContext(bbq, customer, new CustomerSatisfaction());
		int rarity = bbq.foodInstances.Sum(x => x.rarity);
		int taste = bbq.foodInstances.Sum(x => x.taste);
		float rawPrice = rarity * taste;
		Dictionary<string, float> otherMultipliers = new();
		// 1. 添加得分乘区
		foreach (var multiplier in scoreMultipliers){
			otherMultipliers.Add(multiplier.Key, multiplier.Value);
		}
		// 2. 添加其他得分乘区
		foreach (var multiplier in context.OtherMultipliers){
			otherMultipliers.Add(multiplier.Key, multiplier.Value);
		}

		// 执行乘区计算
		foreach (var multiplier in otherMultipliers){
			rawPrice *= multiplier.Value;
		}

		ReviewResult reviewResult = customer.Review(context);

		// 星级计算
		foreach (var record in reviewResult.Records){
			if (record.IsMet){
				if (record.StarValue <= 2){
					Debug.LogError($"【DealSystem】星级计算至少要是3星，否则无效，错误记录：{record.Description}");
					continue;
				}
				rawPrice *= record.StarValue / 2f;
			}
		}

		int roundedRawPrice = Mathf.RoundToInt(rawPrice);
		// 计算价格
		int money = earnMoneyStrategy.EarnMoney(roundedRawPrice);
		DealResult result = new DealResult(context.BBQ, context.Customer, 1f, null, otherMultipliers, rarity, taste, money, rawPrice, null);
		return result;
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
