using System;
using System.Collections.Generic;
using cfg;
using QFramework;

// 通用请求选择事件
public class RequestSelectionEvent : AbstractEvent
{
    public List<string> Choices;      // 候选项
    public string Title;                // 标题
    public Action<string> OnSelect;   // 核心：选择后的回调函数
    public SelectionType SelectionType; // 选择类型
    public RequestSelectionEvent(List<string> choices, string title, SelectionType selectionType, Action<string> onSelect)
    {
        Choices = choices;
        Title = title;
        OnSelect = onSelect;
        SelectionType = selectionType;
    }
}