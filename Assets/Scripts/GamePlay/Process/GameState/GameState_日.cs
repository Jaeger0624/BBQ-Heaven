using QFramework;
using UnityEngine;

public class GameState_日 : AbstractGameState, ICanSendEvent
{
    public override void OnEnter()
    {


        // 2. 推动下一个状态
        this.SendEvent(new ProcessMoveNextEvent());
    }
    public override void OnExit()
    {
    }
}