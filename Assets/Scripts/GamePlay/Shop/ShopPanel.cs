using System.Collections.Generic;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;

public class ShopPanel : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [Header("商店基本组件")]
    [SerializeField] public UIPanel uiPanel;
    [Header("商店信息")]
    [SerializeField] private TextMeshProUGUI shopTypeText;
    [SerializeField] private TextMeshProUGUI shopDescriptionText;
    [Header("商店食材商品")]
    [SerializeField] private FoodDisplayContainer foodDisplayContainer;
    [SerializeField] private PriceButton foodDisplayRefreshButton;
    [Header("商店删除商品")]
    [SerializeField] private PriceButton deletePriceButton;
    // [Header("商店吉祥物商品")]

    // [SerializeField] private ButtonUI 


    private ShopInitContext shopInitContext;

    public void Init(ShopInitContext shopInitContext)
    {
        if (shopInitContext == null) {Debug.LogError("ShopPanel 的 shopInitContext 为空"); return;}
        this.shopInitContext = shopInitContext;

        // 1. 加载商店信息
        shopTypeText.text = shopInitContext.shopName;
        shopDescriptionText.text = shopInitContext.shopDescription;

        // 2. 生成商店商品
        GenerateShopItems();


        // 3. 绑定按钮
        BindButtons();
    }

    private void BindButtons(){
        if (foodDisplayRefreshButton != null){
            foodDisplayRefreshButton.Bind(2, () => {
                foodDisplayContainer.RefreshUI();
            });
        }
    }

    private void GenerateShopItems()
    {
        if (shopInitContext == null) {Debug.LogError("ShopPanel 的 shopInitContext 为空"); return;}


        if (foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = new FoodDisplayTask_随机获取若干(new BuyFoodStrategy(), 3);
            foodDisplayContainer.SetDisplayTask(displayTask);

            foodDisplayContainer.RefreshUI();
        }
    }    
}

// 不同商店刷新价格不一样也很关键
public enum ShopType{
    普通,
    批发,
    折扣,
    收藏,
    黑市,
    升级,
    盲盒
}
public class ShopInitContext{
    public ShopType shopType;
    public string shopName;
    public string shopDescription;
}

public class ShopInfo{
    public ShopItemType shopItemType;
    public int shopItemAmount;
    // 折扣概率
    public bool isDiscount;
}