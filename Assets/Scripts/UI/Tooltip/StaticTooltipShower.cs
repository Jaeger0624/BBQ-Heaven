using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class StaticTooltipShower : SerializedMonoBehaviour, IShowTooltip
{
    [SerializeField] private List<TooltipInfoSO> tooltipInfoSOs;

    public List<TooltipInfo> GetTooltipInfo(){
        return tooltipInfoSOs.Select(tooltipInfoSO => new TooltipInfo(tooltipInfoSO.GetTooltipText())).ToList();
    }
}
