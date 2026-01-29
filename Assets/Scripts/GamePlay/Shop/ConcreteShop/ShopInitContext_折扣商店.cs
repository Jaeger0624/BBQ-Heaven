
using UnityEngine;
using QFramework;
using System.Linq;
public class ShopInitContext_折扣商店 : ShopInitContext{
    public ShopInitContext_折扣商店(){
        shopType = ShopType.批发;
        shopName = "折扣商店";
        shopDescription = "折扣商店中的东西，都是打折的哦~";
    }
    public override void InitShopItems(ShopPanel shop)
    {
        if (shop == null) {Debug.LogError("ShopInitContext_折扣商店 的 shopPanel 为空"); return;}

        // 1. 折扣食材商品
        if (shop.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = FoodDisplayTask_随机获取若干.Build(ShopType.折扣);
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

public class ShopInitContext_强化商店 : ShopInitContext{
    public ShopInitContext_强化商店(){
        shopType = ShopType.强化;
        shopName = "强化商店";
        shopDescription = "强化商店中的东西，都自带强化~";
    }
    public override void InitShopItems(ShopPanel shop)
    {
        if (shop == null) {Debug.LogError("ShopInitContext_强化商店 的 shopPanel 为空"); return;}

        // 1. 强化食材商品
        if (shop.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = FoodDisplayTask_随机获取若干.Build(ShopType.强化);
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