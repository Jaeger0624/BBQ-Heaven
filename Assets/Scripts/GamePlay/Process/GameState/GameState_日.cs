using QFramework;
using UnityEngine;

public class GameState_日 : AbstractGameState, ICanSendEvent
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入日状态");


        // 2. 推动下一个状态
        this.SendEvent(new ProcessMoveNextEvent());
    }
    public override void OnExit()
    {
        Debug.Log("【GameState】退出日状态");
    }
}