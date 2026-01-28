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