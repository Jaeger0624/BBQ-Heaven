using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MascotView : MonoBehaviour, IShowTooltip{
    // 只装有一个吉祥物
    public Mascot mascot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro stackNumberText;
    [SerializeField] private TooltipParent tooltipParent;
    public TooltipParent parent => tooltipParent ?? null;
    public void Init(Mascot mascot){
        this.mascot = mascot;
        UpdateVisual();
    }

    public void UpdateVisual(){
        nameText.text = mascot.name;
        if (mascot.data.Stackable){
            stackNumberText.text = mascot.StackNumber.ToString();
        }else{
            stackNumberText.text = "";
        }
    }
    
    public void OnClick(){

    }

    public List<TooltipInfo> GetTooltipInfo()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
        TooltipInfo tooltipInfo = new TooltipInfo($"<size=20>{mascot.name}</size>\n<size=14>{mascot.description}</size>");
        tooltipInfos.Add(tooltipInfo);
        return tooltipInfos;
    }
}