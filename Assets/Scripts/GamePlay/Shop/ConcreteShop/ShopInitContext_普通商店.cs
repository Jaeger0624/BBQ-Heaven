
using UnityEngine;
using QFramework;
using System.Linq;
public class ShopInitContext_普通商店 : ShopInitContext
{
    public ShopInitContext_普通商店(){
        shopType = ShopType.普通;
        shopName = "普通商店";
        shopDescription = "非常标准的商店！";
    }
    public override void InitShopItems(ShopPanel shop)
    {
        if (shop == null) {Debug.LogError("ShopInitContext_普通商店 的 shopPanel 为空"); return;}

        // 1. 设置食材商品
        if (shop.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = new FoodDisplayTask_随机获取若干(new BuyFoodStrategy(false), 3, false);
            shop.foodDisplayContainer.SetDisplayTask(displayTask);
            shop.foodDisplayContainer.RefreshUI();
        }
        // 2. 设置吉祥物商品
        if (shop.mascotDisplayContainer != null){
            IDisplayTask<MascotUIContext, DisplayMascotView> displayTask = new MascotDisplayTask_随机获取若干(new BuyMascotStrategy(), 3);
            shop.mascotDisplayContainer.SetDisplayTask(displayTask);
            shop.mascotDisplayContainer.RefreshUI();
        }
        // 3. 绑定食材刷新按钮
        if (shop.foodDisplayRefreshButton != null){
            shop.foodDisplayRefreshButton.Bind(2, () => {
                shop.foodDisplayContainer.RefreshUI();
            });
        }
        // 4. 绑定吉祥物刷新按钮
        if (shop.mascotDisplayRefreshButton != null){
            shop.mascotDisplayRefreshButton.Bind(2, () => {
                shop.mascotDisplayContainer.RefreshUI();
            });
        }
        // 5. 绑定食材删除按钮
        if (shop.foodDeleteButton != null){
            shop.foodDeleteButton.Bind(2, () => {
                shop.SendEvent(new FoodSelectPanelEvent(FoodContainerType.单选删除, "删除食材", shop.GetSystem<IFoodSystem>().FoodRepositorys().Values.Select(food => new FoodUIContext(food)).ToList()));
                shop.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Show));
            });
        }
    }
}

