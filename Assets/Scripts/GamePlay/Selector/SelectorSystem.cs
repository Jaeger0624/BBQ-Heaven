using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;
using UnityEngine;

// 负责目标选择和选择面板的显示的系统
public interface ISelectorSystem : ISystem
{
    IObservable<BoardCell> SelectCell(List<BoardCell> validCells);
    void RequestSelection(ISelectionRequest selectionRequest, int refreshAmount, SelectionPanelType selectionPanelType);
    }
public class SelectorSystem : AbstractSystem, ISelectorSystem
{

    protected override void OnInit()
    {
        
    }
    public IObservable<BoardCell> SelectCell(List<BoardCell> validCells)
    {
        return Observable.Create<BoardCell>(observer =>
        {
            // 1. 进入选择模式：通知 BoardSystem 高亮这些格子
            this.GetSystem<IBoardSystem>().HighlightCells(validCells.Select(cell => cell.position).ToList());
            Debug.Log($"【SelectorSystem】高亮格子数: {validCells.Count}");

            // 2. 监听点击流 (假设 BoardSystem 有一个 CellClicked 事件流)
            var clickSub = this.GetSystem<IBoardSystem>().OnCellClicked
                .Where(cell => validCells.Contains(cell)) // 必须点在有效格子上
                .Take(1) // 只取一次
                .Subscribe(cell =>
                {
                    observer.OnNext(cell);
                    observer.OnCompleted();
                });

            // 3. 监听取消流 (比如右键点击)
            var cancelSub = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(1))
                .Take(1)
                .Subscribe(_ => 
                {
                    // 抛出取消异常，中断流
                    observer.OnError(new OperationCanceledException("Player Cancelled Selection"));
                });

            return Disposable.Create(() =>
            {
                // 清理工作：关闭高亮、取消订阅
                this.GetSystem<IBoardSystem>().ClearHighlight();
                clickSub.Dispose();
                cancelSub.Dispose();
            });
        });
    }

    //TODO:
    // public IObservable<OrderView> SelectOrder(List<OrderView> validOrders)
    // {
    //     return Observable.Create<OrderView>(observer =>
    //     {
    //         // 1. 高亮顾客
    //         this.SendEvent(new HighlightCustomersEvent(validOrders.Select(order => order.customer).ToList()));

    //         return Disposable.Create(() =>
    //         {
    //             this.SendEvent(new UnhighlightCustomersEvent());
    //         });
    //     });
    // }

    public void RequestSelection(ISelectionRequest selectionRequest, int refreshAmount, SelectionPanelType selectionPanelType)
    {
        Debug.Log($"【SelectorSystem】请求选择: {selectionRequest.Title}");
        this.SendEvent(new CreateSelectionEvent(selectionRequest, refreshAmount, selectionPanelType));
    }
}
