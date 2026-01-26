using QFramework;
using UnityEngine;

public class GameState_日结算 : AbstractGameState
{
    public override void OnEnter(){
        Debug.Log("【GameState】进入日结算状态");
        IGetDailyEconomy getDailyEconomyStrategy = new GetDailyEconomyStrategy_默认();
        this.GetSystem<IEconomySystem>().UseDailyEconomy(this.GetSystem<IEconomySystem>().GetDailyInfo(getDailyEconomyStrategy));
    
    }
    public override void OnExit(){
        Debug.Log("【GameState】退出日结算状态");
        this.SendEvent(new 退出日结算_Event());
        this.SendEvent(new CloseDailyEconomyPanelEvent());
        this.SendEvent(new HideMainGamePlayEvent());

        // 若检测到目前天数是第二天
        if (this.GetSystem<IGameSystem>().CurrentDay == 2)
        {
            this.SendEvent(new UIPanelEvent(UIPanelType.感谢游玩界面, UIPanelAction.Show));
        }
    }
}

public class 退出日结算_Event : AbstractEvent{}