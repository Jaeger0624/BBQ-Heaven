using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GameState_日前提升 : AbstractGameState
{
    public override void OnEnter()
    {
        // this.GetSystem<IShopSystem>().GenerateFoodSupplyShopItems();   
        this.GetSystem<ISelectorSystem>().RequestSelection(new SelectionRequest_随机食材(3, (foodId) => {
            Debug.Log($"选择食材：{foodId}");
            // 添加食材到仓库
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodId, 1)});
            this.SendEvent(new ProcessMoveNextEvent());
        }), 3, SelectionPanelType.Event);
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