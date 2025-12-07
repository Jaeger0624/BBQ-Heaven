using System.Collections.Generic;
using UnityEngine;

public class ShopItemGenerator : MonoBehaviour, IItemGenerator
{
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform content;
    private List<ShopItemView_通用> shopItemViews = new List<ShopItemView_通用>();
    public void GenerateItem(List<IShopItem> items){
        shopItemViews.Clear();
        foreach (var shopItem in items){
            GameObject shopItemObject = Instantiate(shopItemPrefab, content);
            ShopItemView_通用 shopItemView = shopItemObject.GetComponent<ShopItemView_通用>();
            shopItemView.Bind(shopItem);
            shopItemViews.Add(shopItemView);
        }
    }

    public void Reset(){
        shopItemViews.ForEach(shopItemView => {
            if (shopItemView == null) return;
            Destroy(shopItemView.gameObject);
        });
        shopItemViews.Clear();
    }

}
