using System.Collections.Generic;
using UnityEngine;

public interface IShowTooltip{
    // TooltipInfo是一个描述信息，IShowTooltip需要返回一个List<TooltipInfo>，用于显示多个描述信息
    List<TooltipInfo> GetTooltipInfo();
}

public interface ITooltipData{
    List<TooltipInfo> GetTooltipInfos();
}