using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

public class GameState_日间活动 : AbstractGameState
{
    public override async void OnEnter(){

        Debug.Log(CustomerTime.午间.ToString());
        Debug.Log(CustomerTime.夜间.ToString());
        List<SelectionBuildContext> timeContexts = new List<SelectionBuildContext>
        {
            new SelectionBuildContext(SelectionType.自定义, CustomerTime.午间.ToString(), "午间", "午间活动", ""),
            new SelectionBuildContext(SelectionType.自定义, CustomerTime.夜间.ToString(), "夜间", "夜间活动", "")
        };

        await this.GetSystem<ISelectionSystem>()
            .RequestSelection(
            new SelectionRequest_自定义("选择时间",2, timeContexts, (context) => {
                this.GetSystem<ICustomerSystem>().SetCustomerTime(context.Id);
            }),
            0,
            SelectionPanelType.日间事件);

        await this.GetSystem<ISelectionSystem>()
            .RequestSelection(new SelectionRequest_选择商店(new List<ShopType>(){ShopType.普通, ShopType.批发, ShopType.折扣, ShopType.强化}), 0, SelectionPanelType.日间事件);
        
    }
    public override void OnExit(){
        
        this.SendEvent(new CloseShopPanelEvent());
    }
}
