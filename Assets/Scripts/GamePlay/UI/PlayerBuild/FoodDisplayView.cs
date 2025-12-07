using System.Collections.Generic;
using cfg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodDisplayView : MonoBehaviour, IShowTooltip
{
    [SerializeField] private TooltipParent tooltipParent;
    [SerializeField] private Image foodImage;
    [SerializeField] private TextMeshProUGUI amountText;
    public TooltipParent parent => tooltipParent ?? null;
    private FoodData foodData;
    private int amount = 0;
    
    public void Bind(FoodData foodData, int amount){
        this.foodData = foodData;
        this.amount = amount;
        UpdateVisual();
    }
    private void UpdateVisual(){
        if (foodData == null){
            Debug.LogError("Food is null");
            return;
        }

        Sprite sprite = Resources.Load<Sprite>("Sprites/" + foodData.Sprite);
        if (sprite == null){
            Debug.LogError("Sprite is null");
            return;
        }
        foodImage.sprite = sprite;
        amountText.text = amount.ToString();
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        string description = $"<size=36>{foodData.Name}</size>\n<size=24>{foodData.Description}</size>";
        TooltipInfo tooltipInfo = new TooltipInfo(description);
        return new List<TooltipInfo>(){tooltipInfo};
    }
}