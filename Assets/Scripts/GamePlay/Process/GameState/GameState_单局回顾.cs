using UnityEngine;

public class GameState_单局回顾 : AbstractGameState
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入单局回顾状态");
    }
    public override void OnExit()
    {
        Debug.Log("【GameState】退出单局回顾状态");
    }
}