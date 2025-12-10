using QFramework;
using UnityEngine;

public class GameState_日 : AbstractGameState, ICanSendEvent
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入日状态");

        // 1. 保存游戏进度
        this.GetSystem<ISaveSystem>().SaveGame();

        // 2. 推动下一个状态
        this.SendEvent(new ProcessMoveNextEvent());
    }
    public override void OnExit()
    {
        Debug.Log("【GameState】退出日状态");
    }
}