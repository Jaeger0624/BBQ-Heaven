// 展示物品的视图
using System.Collections.Generic;
using UnityEngine;

public interface IShowItemData{
    Sprite GetItemSprite();
    List<TooltipInfo> GetTooltipInfo();
}

public class ItemView : MonoBehaviour, IShowTooltip
{
    [SerializeField] private TooltipParent tooltipParent;
    public TooltipParent parent => tooltipParent ?? null;
    public IShowItemData itemData;
    public void Bind(IShowItemData itemData){
        this.itemData = itemData;
    }
    public List<TooltipInfo> GetTooltipInfo()
    {
        return itemData.GetTooltipInfo();
    }
}