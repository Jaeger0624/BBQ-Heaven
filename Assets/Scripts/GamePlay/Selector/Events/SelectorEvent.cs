using System;
using System.Collections.Generic;
using cfg;
using QFramework;

// 请求选牌事件
public class RequestCardSelectionEvent : AbstractEvent
{
    public List<CardData> Choices;      // 候选项
    public string Title;                // 标题
    public Action<CardData> OnSelect;   // 核心：选择后的回调函数

    public RequestCardSelectionEvent(List<CardData> choices, string title, Action<CardData> onSelect)
    {
        Choices = choices;
        Title = title;
        OnSelect = onSelect;
    }
}