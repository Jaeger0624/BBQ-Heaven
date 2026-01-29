
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

        // 1. 批发食材商品
        if (shop.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = FoodDisplayTask_随机获取若干.Build(ShopType.批发);

            shop.foodDisplayContainer.SetDisplayTask(displayTask);
            shop.foodDisplayContainer.RefreshUI();
        }

        // 2. 绑定食材刷新按钮
        if (shop.foodDisplayRefreshButton != null){
            shop.foodDisplayRefreshButton.Bind(2, () => {
                shop.foodDisplayContainer.RefreshUI();
            });
        }
    }
}