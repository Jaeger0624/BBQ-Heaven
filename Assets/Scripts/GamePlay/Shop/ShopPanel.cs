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
    [SerializeField] public TextMeshProUGUI shopTypeText;
    [SerializeField] public TextMeshProUGUI shopDescriptionText;
    [SerializeField] public TextMeshProUGUI ProbabilityText;
    [Header("商店食材商品")]
    [SerializeField] public FoodDisplayContainer foodDisplayContainer;
    [SerializeField] public PriceButton foodDisplayRefreshButton;
    [Header("商店吉祥物商品")]
    [SerializeField] public MascotDisplayContainer mascotDisplayContainer;
    [SerializeField] public PriceButton mascotDisplayRefreshButton;
    [Header("商店删除商品")]
    [SerializeField] public PriceButton foodDeleteButton;

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

        shopInitContext.InitShopItems(this);
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


public abstract class ShopInitContext{
    public ShopType shopType;
    public string shopName;
    public string shopDescription;
    /// <summary>
    /// 初始化商店商品
    /// </summary>
    /// <param name="shopPanel">商店面板</param>
    public abstract void InitShopItems(ShopPanel shopPanel);

    public static ShopInitContext Get(ShopType shopType){
        switch (shopType){
            case ShopType.普通:
                return new ShopInitContext_普通商店();
            case ShopType.批发:
                return new ShopInitContext_批发商店();
            default:
                Debug.LogError("不支持的商店类型：" + shopType);
                return null;
        }
    }
    public static ShopInitContext GetRandom(Rng rng){
        List<ShopInitContext> shopInitContexts = new List<ShopInitContext>
        {
            new ShopInitContext_普通商店(),
            new ShopInitContext_批发商店()
        };
        return rng.PickOne(shopInitContexts);
    }
}

public class ShopInitContext_普通商店 : ShopInitContext
{
    public ShopInitContext_普通商店(){
        shopType = ShopType.普通;
        shopName = "普通商店";
        shopDescription = "非常标准的商店！";
    }
    public override void InitShopItems(ShopPanel shopPanel)
    {
        if (shopPanel == null) {Debug.LogError("ShopInitContext_普通商店 的 shopPanel 为空"); return;}


        if (shopPanel.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = new FoodDisplayTask_随机获取若干(new BuyFoodStrategy(false), 3);
            shopPanel.foodDisplayContainer.SetDisplayTask(displayTask);

            shopPanel.foodDisplayContainer.RefreshUI();
        }
        if (shopPanel.mascotDisplayContainer != null){
            IDisplayTask<MascotUIContext, DisplayMascotView> displayTask = new MascotDisplayTask_随机获取若干(new BuyMascotStrategy(), 3);
            shopPanel.mascotDisplayContainer.SetDisplayTask(displayTask);

            shopPanel.mascotDisplayContainer.RefreshUI();
        }
    }
}

public class ShopInitContext_批发商店 : ShopInitContext{
    public ShopInitContext_批发商店(){
        shopType = ShopType.批发;
        shopName = "批发商店";
        shopDescription = "批发商店中的东西，都不能单买喔~";
    }
    public override void InitShopItems(ShopPanel shopPanel)
    {
        if (shopPanel == null) {Debug.LogError("ShopInitContext_批发商店 的 shopPanel 为空"); return;}

        if (shopPanel.foodDisplayContainer != null){
            IDisplayTask<FoodUIContext, DisplayFoodView> displayTask = new FoodDisplayTask_随机获取若干(new BuyFoodStrategy(true), 3);
            shopPanel.foodDisplayContainer.SetDisplayTask(displayTask);

            shopPanel.foodDisplayContainer.RefreshUI();
        }
    }
}