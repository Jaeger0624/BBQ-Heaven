using System;
using UnityEngine;
using QFramework;
using UnityEngine.UI;
using TMPro;
using cfg;

public class DisplayFoodView : MonoBehaviour, IDisplayItemView<FoodUIContext>{
    private FoodUIContext _context;
    private Action<FoodUIContext> _onClick;
    [SerializeField] private Image foodImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image foodRank;
    [SerializeField] private Transform foodSizeParent;
    [SerializeField] private TextMeshProUGUI foodNameText;
    [SerializeField] private TextMeshProUGUI foodCostText;
    [SerializeField] private TextMeshProUGUI foodRarityText;
    [SerializeField] private TextMeshProUGUI foodTasteText;
    public void Bind(FoodUIContext context){
        _context = context;
        foodNameText.text = context.ConfigData.Name;

        if (foodImage!=null){
            foodImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        }
        if (foodNameText!=null){
            foodNameText.text = context.ConfigData.Name;
        }
        if (backgroundImage!=null){
            // backgroundImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        }
        if (foodRarityText!=null){
            foodRarityText.text = context.ConfigData.Rarity.ToString();
        }
        if (foodTasteText!=null){
            foodTasteText.text = context.ConfigData.Taste.ToString();
        }
        if (foodRank!=null){
            switch (context.ConfigData.Rank){
                case Rank.普通:
                    foodRank.color = Color.green;
                    break;
                case Rank.稀有:
                    foodRank.color = Color.blue;
                    break;
                case Rank.史诗:
                // 史诗颜色为紫色
                    foodRank.color = new Color(0.5f, 0f, 0.5f); // Purple
                    break;
                case Rank.传说:
                    foodRank.color = Color.yellow;
                    break;
            }
        }
    }
    public void SetInteraction(Action<FoodUIContext> onClick)
    {
        _onClick = onClick;
    }
    public void OnClick()
    {
        _onClick?.Invoke(_context);
    }
    public void SetSelectedState(bool isSelected)
    {
        if (isSelected){
            foodImage.color = Color.red;
        }else{
            foodImage.color = Color.white;
        }
    }
}