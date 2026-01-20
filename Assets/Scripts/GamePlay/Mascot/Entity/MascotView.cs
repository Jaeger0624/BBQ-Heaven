using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotView : MonoBehaviour, IShowTooltip{
    // 只装有一个吉祥物
    public Mascot mascot;
    [SerializeField] private Image mascotImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI stackNumberText;
    public void Init(Mascot mascot){
        this.mascot = mascot;
        UpdateVisual();
    }

    public void UpdateVisual(){
        if (mascot == null){
            Debug.LogError("Mascot is null");
            return;
        }
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
        TooltipInfo tooltipInfo = new TooltipInfo($"<size=48>{mascot.name}</size>\n<size=32>{mascot.description}</size>");
        tooltipInfos.Add(tooltipInfo);
        return tooltipInfos;
    }
}