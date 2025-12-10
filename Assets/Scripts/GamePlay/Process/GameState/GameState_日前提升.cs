using QFramework;
using UnityEngine;

public class GameState_日前提升 : AbstractGameState
{
    public override void OnEnter()
    {
        this.GetSystem<IShopSystem>().GenerateFoodSupplyShopItems();    
    }
    public override void OnAfterActivate()
    {
        this.GetSystem<ISaveSystem>().SaveGame();
    }
    public override void OnExit()
    {
        this.SendEvent(new HideShop_日前提升());
    }
}