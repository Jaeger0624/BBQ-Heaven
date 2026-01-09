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
    [SerializeField] private TextMeshProUGUI foodNameText;
    public void Bind(FoodUIContext context){
        _context = context;
        foodImage.sprite = Resources.Load<Sprite>("Sprites/" + context.ConfigData.Sprite);
        foodNameText.text = context.ConfigData.Name;
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