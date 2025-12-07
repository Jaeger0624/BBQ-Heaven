using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
public interface IItemGenerator{
    void GenerateItem(List<IShopItem> shopItems);
    void Reset();
}
public class FoodItemGenerator : MonoBehaviour, IItemGenerator
{
    [LabelText("食材商品预制体")]
    public GameObject foodShopItemPrefab;
    [LabelText("内容父物体")]
    public Transform content;
    public Action OnClickEvent;
    private List<ShopItemView_食材> foodShopItemViews = new List<ShopItemView_食材>();
    public void GenerateItem(List<IShopItem> items){
        foodShopItemViews.Clear();
        List<FoodShopItem> foodShopItems = items.Cast<FoodShopItem>().ToList();
        foreach (var foodShopItem in foodShopItems){
            // 1. 创建食材商品视图
            GameObject foodShopItemObject = Instantiate(foodShopItemPrefab, content);
            ShopItemView_食材 foodShopItemView = foodShopItemObject.GetComponent<ShopItemView_食材>();
            
            // 2. 绑定食材商品数据
            foodShopItemView.Bind(foodShopItem);
            foodShopItemViews.Add(foodShopItemView);

            // 3. 绑定购买按钮点击事件
            foodShopItemView.buyButton.OnBuyButtonClick += OnClickEvent;
        }
    }
    public void Reset()
    {
        foodShopItemViews.ForEach(foodShopItemView => {
            if (foodShopItemView.buyButton == null) return;
            if (foodShopItemView.buyButton.OnBuyButtonClick == null) return;
            foodShopItemView.buyButton.OnBuyButtonClick -= OnClickEvent;
            Destroy(foodShopItemView.gameObject);
        });
        foodShopItemViews.Clear();
    }
}
