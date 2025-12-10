using QFramework;
using UniRx;
using UnityEngine;

public class GameState_保存游戏 : AbstractGameState
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入保存游戏状态");

        Observable.EveryUpdate().Take(1).Subscribe(_ => {
            this.SendEvent(new ProcessMoveNextEvent());
        });
    }
    public override void OnExit()
    {

        this.GetSystem<ISaveSystem>().SaveGame();
        Debug.Log("【GameState】退出保存游戏状态");
    }
}