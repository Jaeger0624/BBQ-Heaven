using QFramework;
using UnityEngine;

public class GameState_经营月 : AbstractGameState
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入经营月状态");
        this.GetSystem<IGameSystem>().StartMonth();


        //TODO: 进入经营日，后面可以把这个逻辑放到Controller，可以是按完某个按钮
        this.SendEvent(new ProcessMoveNextEvent());
    }
    public override void OnExit()
    {
        Debug.Log("【GameState】退出经营月状态");
    }
}