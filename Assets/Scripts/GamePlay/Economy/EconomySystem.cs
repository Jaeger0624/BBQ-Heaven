using QFramework;
using UniRx;
using UnityEngine;
using UnityEngine.Timeline;

public interface IEconomySystem : ISystem, ISavable{
    int baseScore{get;}
    int maxInterestScore{get;}
    int profitScore{get;}
    ReactiveProperty<int> coin { get; }
    void AddCoin(int amount);
    void CostCoin(int amount);
    DailyInfo GetDailyInfo(IGetDailyEconomy getDailyEconomyStrategy);
    /// <summary>
    /// 生效日结算
    /// </summary>
    /// <param name="dailyEconomyInfo"></param>
    void UseDailyEconomy(DailyInfo dailyEconomyData);
}
public class EconomySystem : AbstractSystem, IEconomySystem
{
    public int baseScore{get; set;} = 3;
    public int maxInterestScore{get; set;} = 5;
    public int profitScore{get; set;} = 2;
    // Q币，用于购买道具和升级等
    public ReactiveProperty<int> coin { get; private set; }
    private DailyInfo dailyInfo;
    protected override void OnInit()
    {
        coin = new ReactiveProperty<int>(0);
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<DealCompletedEvent>(OnDealCompleted);
        this.RegisterEvent<AddEncounterEvent>(OnAddEncounter);
    }
    protected override void OnDeinit()
    {
        coin.Dispose();
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<DealCompletedEvent>(OnDealCompleted);
        this.UnRegisterEvent<AddEncounterEvent>(OnAddEncounter);
    }
    public void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        dailyInfo = new DailyInfo();
    }
    public void OnDealCompleted(DealCompletedEvent evt)
    {
        // 1. 增加交易次数
        dailyInfo.dealCount++;

        // 2. 更新最佳交易
        if (dailyInfo.BestDeal == null || evt.result.price > dailyInfo.BestDeal.price)
        {
            dailyInfo.BestDeal = evt.result;
        }

        // 3. 更新食材销量
        foreach (var foodInstance in evt.result.bbq.foodInstances)
        {
            string foodID = foodInstance.food.foodDataId;
            if (dailyInfo.FoodSales.ContainsKey(foodID)) dailyInfo.FoodSales[foodID]++;
            else dailyInfo.FoodSales[foodID] = 1;
        }
    }
    public void OnAddEncounter(AddEncounterEvent evt)
    {
        // 1. 增加遭遇次数
        dailyInfo.EncounterNames.Add(evt.activeEncounter.encounterData.Name);
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.coin = coin.Value;
    }
    public void Load(GameArchive archive)
    {
        coin.Value = archive.playerInfoData.coin;
    }
    public void AddCoin(int amount)
    {
        coin.Value += amount;
    }
    public void CostCoin(int amount)
    {
        if (coin.Value < amount)
        {
            Debug.Log($"经济系统：金币不足，无法扣除 {amount} 金币");
            coin.Value = 0;
            return;
        }
        coin.Value -= amount;
    }

    public DailyInfo GetDailyInfo(IGetDailyEconomy getDailyEconomyStrategy)
    {
        DailyEconomy dailyEconomyInfo = getDailyEconomyStrategy.GetDailyEconomyInfo();

        if (dailyInfo == null)
        {
            dailyInfo = new DailyInfo();
            Debug.LogError("【EconomySystem】每日信息为空，创建新的每日信息");
        }
        dailyInfo.DailyEconomy = dailyEconomyInfo;
        return dailyInfo;
    }
    public void UseDailyEconomy(DailyInfo dailyInfo)
    {
        AddCoin(dailyInfo.DailyEconomy.totalScore);
        this.SendEvent(new CreateDailyEconomyPanelEvent(dailyInfo));
    }
}


public interface IGetDailyEconomy{
    DailyEconomy GetDailyEconomyInfo();
}
public abstract class AbstractGetDailyEconomyStrategy : IGetDailyEconomy, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract DailyEconomy GetDailyEconomyInfo();
}

public class GetDailyEconomyStrategy_默认 : AbstractGetDailyEconomyStrategy{
    public override DailyEconomy GetDailyEconomyInfo(){
        int baseScore = this.GetSystem<IEconomySystem>().baseScore;
        int interestScore = GetInterestScore();
        int profitScore = GetProfitScore();
        return new DailyEconomy(baseScore, interestScore, profitScore);
    }

    private int GetInterestScore(){
        // 1. 获取当前Q币数量
        int currentQCoin = this.GetSystem<IEconomySystem>().coin.Value;

        // 2. 计算利息
        int interestScore = currentQCoin/5;

        // 3. 限制最大值
        int final = Mathf.Min(interestScore, this.GetSystem<IEconomySystem>().maxInterestScore);
        return final;
    }

    private int GetProfitScore(){
        // 1. 获取当前得分
        int currentScore = this.GetSystem<IScoreSystem>().Score;
        int targetScore = this.GetSystem<IScoreSystem>().TargetScore;

        // 2. 计算奖金 => 算当前分数与目标分数的倍数关系
        float multiplier = (float)currentScore / (float)targetScore;
        
        // 3. 计算奖金 => 取2的对数
        int profitScore = (int)Mathf.Log(multiplier, 2);
        Debug.Log($"【EconomySystem】\n当前得分：{currentScore}，目标得分：{targetScore}\n倍数：{multiplier}，利润为: {profitScore}<color=yellow>Q</color>");
        return profitScore;
    }
}

public class GetDailyEconomyStrategy_零 : AbstractGetDailyEconomyStrategy{
    public override DailyEconomy GetDailyEconomyInfo(){
        return new DailyEconomy(0, 0, 0);
    }
}

public class GetDailyEconomyStrategy_拉满 : AbstractGetDailyEconomyStrategy{
    public override DailyEconomy GetDailyEconomyInfo(){
        int baseScore = this.GetSystem<IEconomySystem>().baseScore;
        int maxInterestScore = this.GetSystem<IEconomySystem>().maxInterestScore;
        int profitScore = this.GetSystem<IEconomySystem>().profitScore;
        return new DailyEconomy(baseScore, maxInterestScore, profitScore);
    }
}