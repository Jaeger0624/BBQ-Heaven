using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
        
public class MascotDisplayView : MonoBehaviour, IShowTooltip
{
    [SerializeField] private TooltipParent tooltipParent;
    [SerializeField] private Image foodImage;
    [SerializeField] private TextMeshProUGUI amountText;
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
        amountText.text = mascot.StackNumber == 1 ? "" : mascot.StackNumber.ToString();
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        return new List<TooltipInfo>();
    }
}