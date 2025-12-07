using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeDisplayView : MonoBehaviour, IShowTooltip
{
    [SerializeField] private TooltipParent tooltipParent;
    [SerializeField] private Image foodImage;
    [SerializeField] private TextMeshProUGUI amountText;
    public TooltipParent parent => tooltipParent ?? null;
    private Recipe recipe;
    
    public void Bind(Recipe recipe){
        this.recipe = recipe;
        UpdateVisual();
    }
    private void UpdateVisual(){
        if (recipe == null){
            Debug.LogError("Recipe is null");
            return;
        }

        // Sprite sprite = Resources.Load<Sprite>("Sprites/" + mascot.data.Icon);
        // if (sprite == null){
        //     Debug.LogError("Sprite is null");
        //     return;
        // }
        // foodImage.sprite = sprite;

        // 先临时用着，把配方名字显示出来
        amountText.text = recipe.name;
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        return new List<TooltipInfo>();
    }
}