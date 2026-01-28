
using UnityEngine;
using QFramework;
using System.Linq;
public class ShopInitContext_批发商店 : ShopInitContext{
    public ShopInitContext_批发商店(){
        shopType = ShopType.批发;
        shopName = "批发商店";
        shopDescription = "批发商店中的东西，都不能单买喔~";
    }
    public override void InitShopItems(ShopPanel shop)
    {
        if (shop == null) {Debug.LogError("ShopInitContext_批发商店 的 shopPanel 为空"); return;}

        if (shop.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = new FoodDisplayTask_随机获取若干(new BuyFoodStrategy(true), 3, true);
            shop.foodDisplayContainer.SetDisplayTask(displayTask);
            shop.foodDisplayContainer.RefreshUI();
        }
    }
}