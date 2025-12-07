using QFramework;
using UnityEngine;

public interface ISetScoreStrategy{
    int GetTargetScore();
}

public abstract class AbstractSetScoreStrategy : ISetScoreStrategy, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract int GetTargetScore();
}

public class SetScoreStrategy_默认 : AbstractSetScoreStrategy{
    public override int GetTargetScore()
    {
        return 100 * this.GetSystem<IGameSystem>().CurrentDay;
    }
}

public class SetScoreStrategy_翻倍 : AbstractSetScoreStrategy{
    public override int GetTargetScore()
    {
        if (this.GetSystem<IScoreSystem>().TargetScore <= 100){
            return 100;
        }else{
            return this.GetSystem<IScoreSystem>().TargetScore * 2;
        }
    }
}

public class SetScoreStrategy_每日目标分数 : AbstractSetScoreStrategy{
    public override int GetTargetScore()
    {
        if (SettingManager.GetSetting<GameplaySettings>().每日目标分数列表.Count == 0){
            Debug.LogError("【SetScoreStrategy_每日目标分数】每日目标分数列表为空");
            return 0;
        }
        int currentDay = this.GetSystem<IGameSystem>().CurrentDay;
            int index = currentDay - 1;
            int finalIndex = Mathf.Min(index, SettingManager.GetSetting<GameplaySettings>().每日目标分数列表.Count - 1);
            return SettingManager.GetSetting<GameplaySettings>().每日目标分数列表[finalIndex];
    }
}

