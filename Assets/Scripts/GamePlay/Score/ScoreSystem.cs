using QFramework;
using UnityEngine;
public interface IScoreSystem : ISystem{
    int Score { get; }
    int TargetScore { get; }

    /// <summary> 改变分数 </summary>
    public void ChangeScore(int changeScore);
    /// <summary> 设置目标分数 </summary>
    // public void SetTargetScore(int targetScore);
    public void ChangeTargetScore();
    /// <summary> 检查目标分数 </summary>
    public bool CheckTargetScore();
}
public class ScoreSystem : AbstractSystem, IScoreSystem
{
    public int Score { get; private set; }
    public int TargetScore { get; private set; } = 0;
    private ISetScoreStrategy setScoreStrategy;
    protected override void OnInit()
    {    
        setScoreStrategy = new SetScoreStrategy_每日目标分数();
        this.RegisterEvent<退出日结算_Event>(OnExitDaySettlementEvent);
    }

    protected override void OnDeinit()
    {
        this.UnRegisterEvent<退出日结算_Event>(OnExitDaySettlementEvent);
    }

    private void OnExitDaySettlementEvent(退出日结算_Event evt)
    {
        // 重置分数
        ChangeScore(-Score);
    }

    public void ChangeScore(int changeScore)
    {
        Score += changeScore;
        // Debug.Log($"【ScoreSystem】当前分数: {Score}");

        // 注册动画
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            this.SendEvent(new ChangeScoreEvent(changeScore));
        }));
    }

    private void SetTargetScore(int targetScore)
    {
        this.TargetScore = targetScore;
        this.SendEvent(new SetTargetScoreEvent(targetScore));
    }

    public bool CheckTargetScore()
    {
        if (Score >= TargetScore)
        {
            // 归零
            return true;
        }
        else{
            // 游戏失败
            this.GetSystem<IProcessSystem>().GameOver("分数未达到目标分数");
            return false;
        }
    }

    public void ChangeTargetScore()
    {
        this.SetTargetScore(setScoreStrategy.GetTargetScore());
    }
}


public class ChangeScoreEvent{
    public int changeScore;
    public ChangeScoreEvent(int changeScore){
        this.changeScore = changeScore;
    }
}

public class SetTargetScoreEvent{
    public int targetScore;
    public SetTargetScoreEvent(int targetScore){
        this.targetScore = targetScore;
    }
}