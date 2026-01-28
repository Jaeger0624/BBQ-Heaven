using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;

public class ShopPanel : MonoBehaviour, IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [Header("商店基本组件")]
    [SerializeField] public UIPanel uiPanel;
    [Header("商店信息")]
    [SerializeField] private TextMeshProUGUI shopTypeText;
    [SerializeField] private TextMeshProUGUI shopDescriptionText;
    [SerializeField] private TextMeshProUGUI ProbabilityText;
    [Header("商店食材商品")]
    [SerializeField] private FoodDisplayContainer foodDisplayContainer;
    [SerializeField] private PriceButton foodDisplayRefreshButton;
    [Header("商店吉祥物商品")]
    [SerializeField] private MascotDisplayContainer mascotDisplayContainer;
    [SerializeField] private PriceButton mascotDisplayRefreshButton;
    [Header("商店删除商品")]
    [SerializeField] private PriceButton foodDeleteButton;

    // [SerializeField] private ButtonUI 


    private ShopInitContext shopInitContext;

    public void Init(ShopInitContext shopInitContext)
    {
        if (shopInitContext == null) {Debug.LogError("ShopPanel 的 shopInitContext 为空"); return;}
        this.shopInitContext = shopInitContext;
        // 1. 加载商店信息
        shopTypeText.text = shopInitContext.shopName;
        shopDescriptionText.text = shopInitContext.shopDescription;

        if (ProbabilityText != null){
            ProbabilityInfo probabilityInfo = this.GetSystem<IRandomSystem>().probabilityInfo;
            ProbabilityText.text = $"刷新概率\n" +
            $"<color=green>普通</color>: {probabilityInfo.probabilities[Rank.普通] * 100}% " +
            $"<color=blue>稀有</color>: {probabilityInfo.probabilities[Rank.稀有] * 100}% \n" +
            $"<color=purple>史诗</color>: {probabilityInfo.probabilities[Rank.史诗] * 100}% " +
            $"<color=yellow>传说</color>: {probabilityInfo.probabilities[Rank.传说] * 100}% ";
        }
        // 2. 生成商店商品
        GenerateShopItems();
        // 3. 绑定按钮
        BindButtons();
    }

    private void BindButtons(){

        // 刷新按钮
        if (foodDisplayRefreshButton != null){
            foodDisplayRefreshButton.Bind(2, () => {
                foodDisplayContainer.RefreshUI();
            });
        }
        if (mascotDisplayRefreshButton != null){
            mascotDisplayRefreshButton.Bind(2, () => {
                mascotDisplayContainer.RefreshUI();
            });
        }


        // 其他按钮
        if (foodDeleteButton != null){
            foodDeleteButton.Bind(2, () => {
                this.SendEvent(new FoodSelectPanelEvent(FoodContainerType.单选删除, "删除食材", this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList().Select(food => new FoodUIContext(food)).ToList()));
                this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Show));
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
        if (mascotDisplayContainer != null){
            IDisplayTask<MascotUIContext, DisplayMascotView> displayTask = new MascotDisplayTask_随机获取若干(new BuyMascotStrategy(), 3);
            mascotDisplayContainer.SetDisplayTask(displayTask);

            mascotDisplayContainer.RefreshUI();
        }
    }    

    public void ContinueProcess(){
        this.SendEvent<ProcessMoveNextEvent>();
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
    public static ShopInitContext Get(ShopType shopType){
        switch (shopType){
            case ShopType.普通:
                return new ShopInitContext(){
                    shopType = ShopType.普通,
                    shopName = "普通商店",
                    shopDescription = "普通商店中的东西，都是普通品质的",
                };
            case ShopType.批发:
                return new ShopInitContext(){
                    shopType = ShopType.批发,
                    shopName = "批发商店",
                    shopDescription = "批发商店中的东西，都不能单买喔~",
                };
            default:
                Debug.LogError("不支持的商店类型：" + shopType);
                return null;
        }
    }
    public static ShopInitContext GetRandom(Rng rng){
        List<ShopInitContext> shopInitContexts = new List<ShopInitContext>
        {
            new ShopInitContext()
            {
                shopType = ShopType.普通,
                shopName = "普通商店",
                shopDescription = "普通商店中的东西，都是普通品质的",
            },
            new ShopInitContext()
            {
                shopType = ShopType.批发,
                shopName = "批发商店",
                shopDescription = "批发商店中的东西，都不能单买喔~",
            },
            // new ShopInitContext()
            // {
            //     shopType = ShopType.折扣,
            //     shopName = "折扣商店",
            //     shopDescription = "折扣商店中的东西，都有折扣喔~",
            // },
            // new ShopInitContext()
            // {
            //     shopType = ShopType.收藏,
            //     shopName = "收藏商店",
            //     shopDescription = "收藏商店中的东西，都是收藏品喔~",
            // },
            // new ShopInitContext()
            // {
            //     shopType = ShopType.黑市,
            //     shopName = "黑市商店",
            //     shopDescription = "黑市商店中的东西，都是黑市品喔~",
            // },
            // new ShopInitContext()
            // {
            //     shopType = ShopType.升级,
            //     shopName = "升级商店",
            //     shopDescription = "升级商店中的东西，都是升级品喔~",
            // },
            // new ShopInitContext()
            // {
            //     shopType = ShopType.盲盒,
            //     shopName = "盲盒商店",
            //     shopDescription = "盲盒商店中的东西，都是盲盒品喔~",
            // }
        };
        return rng.PickOne(shopInitContexts);
    }

}