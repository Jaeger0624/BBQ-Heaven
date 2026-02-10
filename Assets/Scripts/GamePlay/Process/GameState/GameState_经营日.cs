using QFramework;
using UnityEngine;

public class GameState_经营日 : AbstractGameState
{

    public override void OnEnter(){

        this.SendEvent(new ShowMainGamePlayEvent());
        this.SendEvent(new ChangePanelEvent(ProcessPanel.Kitchen));

        GameplaySettings settings = SettingManager.GetSetting<GameplaySettings>();
        TimeInfo currentTime = new TimeInfo(settings.currentTime_默认.x, settings.currentTime_默认.y);
        TimeInfo targetTime = new TimeInfo(settings.targetTime_默认.x, settings.targetTime_默认.y);
        // 设置下一个时间信息
        this.GetSystem<ITimeSystem>().SetNextTimeInfo(currentTime, targetTime);
        
        this.GetSystem<IGameSystem>().StartDay();
    }
    public override void OnExit(){
        this.GetSystem<IGameSystem>().EndDay();
        this.SendEvent(new ChangePanelEvent(ProcessPanel.Customer));
    }
}
