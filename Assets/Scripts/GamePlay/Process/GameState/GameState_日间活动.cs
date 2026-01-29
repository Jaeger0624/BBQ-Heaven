using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GameState_日间活动 : AbstractGameState
{
    public override void OnEnter(){
        Debug.Log("【GameState】进入日间活动状态");


        this.GetSystem<ISelectorSystem>()
            .RequestSelection(new SelectionRequest_选择商店(new List<ShopType>(){ShopType.普通, ShopType.批发, ShopType.折扣, ShopType.强化}), 0, SelectionPanelType.日间事件);
    }
    public override void OnExit(){
        Debug.Log("【GameState】退出日间活动状态");
        
        this.SendEvent(new CloseShopPanelEvent());
    }
}
