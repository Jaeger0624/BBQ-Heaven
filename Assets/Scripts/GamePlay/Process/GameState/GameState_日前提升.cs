using QFramework;
using UnityEngine;

public class GameState_日前提升 : AbstractGameState
{
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入日前提升状态");

        this.GetSystem<IShopSystem>().GenerateFoodSupplyShopItems();
    }
    public override void OnExit()
    {
        Debug.Log("【GameState】退出日前提升状态");

        this.SendEvent(new HideShop_日前提升());
    }
}