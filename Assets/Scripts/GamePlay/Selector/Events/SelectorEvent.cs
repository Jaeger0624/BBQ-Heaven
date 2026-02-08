using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;

// 通用请求选择事件
public class SelectRequest
{
    public List<SelectionBuildContext> Contexts;      // 候选项
    public string Title;                // 标题
    public Action<SelectionBuildContext> OnSelect;   // 核心：选择后的回调函数
    public SelectRequest(List<SelectionBuildContext> choices, string title, Action<SelectionBuildContext> onSelect)
    {
        Contexts = choices;
        Title = title;
        OnSelect = onSelect;
    }
}


public class CreateSelectionEvent : AbstractEvent{
    public ISelectionRequest SelectionRequest;
    public int RefreshAmount;
    public SelectionPanelType SelectionPanelType;
    private AsyncSubject<Unit> Subject;
    public CreateSelectionEvent(ISelectionRequest selectionRequest, int refreshAmount, SelectionPanelType selectionPanelType){
        SelectionRequest = selectionRequest;
        RefreshAmount = refreshAmount;
        SelectionPanelType = selectionPanelType;
        Subject = new AsyncSubject<Unit>();
    }
    public AsyncSubject<Unit> GetSubject(){
        return Subject;
    }
}