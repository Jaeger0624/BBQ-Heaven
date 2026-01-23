using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
        
public class MascotDisplayView : MonoBehaviour, IShowTooltip
{
    [SerializeField] private TooltipParent tooltipParent;
    [SerializeField] private Image MascotDisplayImage;
    [SerializeField] private TextMeshProUGUI MascotNameText;
    [SerializeField] private TextMeshProUGUI MascotStackNumberText;
    [SerializeField] private TextMeshProUGUI MascotPriceText;
    public TooltipParent parent => tooltipParent ?? null;
    private Mascot mascot;
    
    public void Bind(Mascot mascot){
        this.mascot = mascot;
        UpdateVisual();
    }
    private void UpdateVisual(){
        if (mascot == null){
            Debug.LogError("Food is null");
            return;
        }

        // Sprite sprite = Resources.Load<Sprite>("Sprites/" + mascot.data.Icon);
        // if (sprite == null){
        //     Debug.LogError("Sprite is null");
        //     return;
        // }
        // foodImage.sprite = sprite;
        if (MascotNameText != null){
            MascotNameText.text = mascot.name;
        }
        if (MascotStackNumberText != null){
            MascotStackNumberText.text = mascot.StackNumber == 1 ? "" : mascot.StackNumber.ToString();
        }
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        string description = $"<size=50>{mascot.name}</size>\n<size=36>{mascot.description}</size>";
        TooltipInfo tooltipInfo = new TooltipInfo(description);
        return new List<TooltipInfo>(){tooltipInfo};
    }
}