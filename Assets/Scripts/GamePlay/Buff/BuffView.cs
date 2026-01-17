using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuffView : MonoBehaviour, IShowTooltip{
    [SerializeField] private TextMeshProUGUI stackNumberText;
    private Buff buff;
    public void Bind(Buff buff){
        this.buff = buff;
        UpdateVisual();
    }
    public void UpdateVisual(){
        if (buff.isStackable){
            stackNumberText.text = buff.GetStackNumber().ToString();
        }
        else{
            stackNumberText.text = "";
        }
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        return new List<TooltipInfo>{new TooltipInfo(buff.name)};
    }
}