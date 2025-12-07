using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView_通用 : MonoBehaviour{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private BuyButton buyButton;
    public IShopItem shopItem { get; set; }
    public int MaxChooseTime = 1;
    public int CurrentChooseTime = 0;

    public void Bind(IShopItem shopItem) {
        if (shopItem == null) {Debug.LogError("ShopItemView_食材 的 shopItem 为空"); return;}
        this.shopItem = shopItem;
        UpdateVisual();
    }

    private void UpdateVisual(){
        if (shopItem == null) {Debug.LogError("ShopItemView_食材 的 shopItem 为空"); return;}
        // 1. 更新视觉
        // _icon.sprite = Resources.Load<Sprite>("Sprites/" + shopItem.icon);
        _name.text = shopItem.name;
        buyButton.Bind(shopItem);
        buyButton.OnBuyButtonClick += AfterClickedEvent;
    }

    private void AfterClickedEvent(){
        CurrentChooseTime++;
        if (CurrentChooseTime < MaxChooseTime) return;
        buyButton.OnBuyButtonClick -= AfterClickedEvent;
        buyButton.OnBuyButtonClick = null;
        buyButton = null;
        shopItem = null;
        _icon = null;
        _name = null;
        Destroy(gameObject);
    }
}