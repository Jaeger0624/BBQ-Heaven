using System;
using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
//TODO: 删除该类，已弃用
/// <summary>
/// 挂载在购买按钮上
/// 负责处理购买按钮的逻辑
/// </summary>
public class BuyButton : MonoBehaviour, IController
{
    [SerializeField] private Button button;
    [SerializeField] private IShopItem shopItem;
    [SerializeField] private TextMeshProUGUI priceText;
    public Action OnBuyButtonClick;

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Bind(IShopItem shopItem){
        this.shopItem = shopItem;
        UpdateVisual();
    }

    private void UpdateVisual(){
        if (shopItem == null){
            Debug.LogError("BuyButton 的 shopItem 为空");
            return;
        }
        if (shopItem.price == 0){
            priceText.text = "获取";
            return;
        }
        priceText.text = $"{shopItem.price}<color=yellow>Q</color>";
    }

    private void OnClick(){
        if (shopItem == null){
            Debug.LogError("BuyButton 的 shopItem 为空");
            return;
        }
        int currentCoin = this.GetSystem<IEconomySystem>().coin.Value;
        if (currentCoin < shopItem.price){
            Debug.Log("金币不足，无法购买");
            return;
        }
        this.GetSystem<IShopSystem>().Buy(shopItem);
        OnBuyButtonClick?.Invoke();
    }
}
