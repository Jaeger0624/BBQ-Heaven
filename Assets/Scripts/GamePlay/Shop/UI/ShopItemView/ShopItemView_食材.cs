using System;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView_食材 : MonoBehaviour, IController
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _amount;
    [SerializeField] public BuyButton buyButton;
    private FoodShopItem shopItem;
    public int MaxChooseTime = 1;
    public int CurrentChooseTime = 0;
    public void Bind(FoodShopItem shopItem){
        if (shopItem == null) {Debug.LogError("ShopItemView_食材 的 shopItem 为空"); return;}
        this.shopItem = shopItem;
        UpdateVisual();
    }
    

    private void UpdateVisual(){
        if (shopItem == null) {Debug.LogError("ShopItemView_食材 的 shopItem 为空"); return;}
        // 1. 获取FoodData
        FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(shopItem.id);
        if (foodData == null) {Debug.LogError("ShopItemView_食材 的 foodData 为空"); return;}
        // 2. 更新视觉
        _icon.sprite = Resources.Load<Sprite>("Sprites/" + foodData.Sprite);
        _name.text = foodData.Name;
        _amount.text = $"x{shopItem.amount}";
        buyButton.Bind(shopItem);

        buyButton.OnBuyButtonClick += AfterClickedEvent;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;


    public void AfterClickedEvent(){
        CurrentChooseTime++;
        if (CurrentChooseTime < MaxChooseTime) return;
        buyButton.OnBuyButtonClick -= AfterClickedEvent;
        buyButton.OnBuyButtonClick = null;
        buyButton = null;
        shopItem = null;
        _icon = null;
        _name = null;
        _amount = null;
        Destroy(gameObject);
    }

    private void Reset()
    {
        CurrentChooseTime = 0;
    }
}
