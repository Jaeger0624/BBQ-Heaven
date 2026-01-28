using QFramework;
using UnityEngine;

public class GameState_日间活动 : AbstractGameState
{
    public override void OnEnter(){
        Debug.Log("【GameState】进入日间活动状态");


        ShopInitContext shopInitContext = new ShopInitContext(){
            shopType = ShopType.普通,
            shopName = "普通商店",
            shopDescription = "普通商店",
        };

        this.GetSystem<IShopSystem>().GenerateShop(shopInitContext);
    }
    public override void OnExit(){
        Debug.Log("【GameState】退出日间活动状态");
        
        // this.SendEvent(new HideShopEvent_日间商店());

        this.SendEvent(new CloseShopPanelEvent());
    }
}
