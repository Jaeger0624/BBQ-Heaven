using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "TooltipInfoSO", menuName = "TooltipInfo")]
public class TooltipInfoSO : SerializedScriptableObject{
    public List<TextInfo> tooltipInfos;
    public string GetTooltipText(){
        return string.Join("\n", tooltipInfos.Select(tooltipInfo => tooltipInfo.GetText()).ToList());
    }
}