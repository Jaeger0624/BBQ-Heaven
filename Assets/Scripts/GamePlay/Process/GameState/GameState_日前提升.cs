using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GameState_日前提升 : AbstractGameState
{
    public override void OnEnter()
    {
        // this.GetSystem<IShopSystem>().GenerateFoodSupplyShopItems();   
        this.GetSystem<ISelectionSystem>().RequestSelection(new SelectionRequest_随机食材(3, (foodContext) => {
            Debug.Log($"选择食材：{foodContext.Id}");
            // 添加食材到仓库
            this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(foodContext.Id, 1, new List<FoodCardEnhancement>())});
            this.SendEvent(new ProcessMoveNextEvent());
        }), 3, SelectionPanelType.食材);
    }
    public override void OnAfterActivate()
    {
        this.GetSystem<ISaveSystem>().SaveGame();
    }
    public override void OnExit()
    {
    }
}