using QFramework;
using UniRx;
using UnityEngine;
using UnityEngine.Timeline;

public interface IEconomySystem : ISystem{
    int baseScore{get;}
    int maxInterestScore{get;}
    int profitScore{get;}
    ReactiveProperty<int> coin { get; }
    void AddCoin(int amount);
    void CostCoin(int amount);
    DailyEconomyInfo GetDailyEconomyInfo(IGetDailyEconomyStrategy getDailyEconomyStrategy);
    /// <summary>
    /// 生效日结算
    /// </summary>
    /// <param name="dailyEconomyInfo"></param>
    void UseDailyEconomy(DailyEconomyInfo dailyEconomyInfo);
}
public class EconomySystem : AbstractSystem, IEconomySystem
{
    public int baseScore{get; set;} = 3;
    public int maxInterestScore{get; set;} = 5;
    public int profitScore{get; set;} = 2;
    // Q币，用于购买道具和升级等
    public ReactiveProperty<int> coin { get; private set; }
    protected override void OnInit()
    {
        coin = new ReactiveProperty<int>(0);
    }
    protected override void OnDeinit()
    {
        coin.Dispose();
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

    public DailyEconomyInfo GetDailyEconomyInfo(IGetDailyEconomyStrategy getDailyEconomyStrategy){
        if (getDailyEconomyStrategy == null){
            getDailyEconomyStrategy = new GetDailyEconomyStrategy_默认();
        }
        return getDailyEconomyStrategy.GetDailyEconomyInfo();
    }
    public void UseDailyEconomy(DailyEconomyInfo dailyEconomyInfo){
        // 1. 获取金币
        int coin = dailyEconomyInfo.totalScore;

        AddCoin(coin);

        // 2. 创建日结算面板
        this.SendEvent(new CreateDailyEconomyPanelEvent(dailyEconomyInfo));
    }
}


public class DailyEconomyInfo{
    public int baseScore{get;} // 基础
    public int interestScore{get;} // 利息（基础最大值为5）
    public int profitScore{get;} // 利润
    public int totalScore{get;} // 总得分

    public DailyEconomyInfo(int baseScore, int interestScore, int profitScore){
        this.baseScore = baseScore;
        this.interestScore = interestScore;
        this.profitScore = profitScore;
        this.totalScore = baseScore + interestScore + profitScore;
    }

}


public interface IGetDailyEconomyStrategy{
    DailyEconomyInfo GetDailyEconomyInfo();
}
public abstract class AbstractGetDailyEconomyStrategy : IGetDailyEconomyStrategy, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract DailyEconomyInfo GetDailyEconomyInfo();
}

public class GetDailyEconomyStrategy_默认 : AbstractGetDailyEconomyStrategy{
    public override DailyEconomyInfo GetDailyEconomyInfo(){
        int baseScore = this.GetSystem<IEconomySystem>().baseScore;
        int interestScore = GetInterestScore();
        int profitScore = GetProfitScore();
        return new DailyEconomyInfo(baseScore, interestScore, profitScore);
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
    public override DailyEconomyInfo GetDailyEconomyInfo(){
        return new DailyEconomyInfo(0, 0, 0);
    }
}

public class GetDailyEconomyStrategy_拉满 : AbstractGetDailyEconomyStrategy{
    public override DailyEconomyInfo GetDailyEconomyInfo(){
        int baseScore = this.GetSystem<IEconomySystem>().baseScore;
        int maxInterestScore = this.GetSystem<IEconomySystem>().maxInterestScore;
        int profitScore = this.GetSystem<IEconomySystem>().profitScore;
        return new DailyEconomyInfo(baseScore, maxInterestScore, profitScore);
    }
}