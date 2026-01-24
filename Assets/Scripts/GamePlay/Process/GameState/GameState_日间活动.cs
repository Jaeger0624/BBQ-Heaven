using QFramework;
using UnityEngine;

public class GameState_日间活动 : AbstractGameState
{
    public override void OnEnter(){
        Debug.Log("【GameState】进入日间活动状态");

        // 1. 生成每日商品
        // this.GetSystem<IShopSystem>().GenerateDailyShopItems();
        // 2. 随机选择事件
        // this.GetSystem<IMyEventSystem>().RandomSelectEvent(3);

        this.SendEvent(new CreateShopPanelEvent(new ShopInitContext(){
            shopType = ShopType.普通,
            shopName = "普通商店",
            shopDescription = "普通商店",
        }));
    }
    public override void OnExit(){
        Debug.Log("【GameState】退出日间活动状态");
        
        // this.SendEvent(new HideShopEvent_日间商店());

        this.SendEvent(new CloseShopPanelEvent());
    }
}
