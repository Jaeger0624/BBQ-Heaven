using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;
public interface IDialogueSystem : ISystem{
    // 点击信号流（UI层调用）
    void OnClickNext();
    
    // 核心入口：播放对话序列
    IObservable<Unit> ShowDialogues(List<Dialogue> dialogues);
}

public class DialogueSystem : AbstractSystem, IDialogueSystem{
    // 用来接收UI点击事件的信号流
    private Subject<Unit> _nextClickSubject = new Subject<Unit>();
    protected override void OnInit()
    {
        _nextClickSubject = new Subject<Unit>();
    }
    protected override void OnDeinit()
    {
        _nextClickSubject.Dispose();
    }
    public void OnClickNext()
    {
        Debug.Log("OnClickNext");
        // 关键点：只发射信号，不要 OnCompleted，否则Subject就废了，下次点不动
        _nextClickSubject.OnNext(Unit.Default);
    }

    public IObservable<Unit> ShowDialogues(List<Dialogue> dialogues)
    {
        // 使用 Create 创建一个完整的异步任务流
        return Observable.Create<Unit>(observer =>
        {
            // 1. 序列开始：打开面板
            this.SendEvent(new UIPanelEvent(UIPanelType.对话界面, UIPanelAction.Show));

            // 2. 核心逻辑：将 List 转化为串行流
            return dialogues.ToObservable() // 1. 将 List<Dialogue> 转为流
                .Select(dialogue => ShowSingleDialogueInternal(dialogue))
                .Concat()
                .Subscribe(
                    _ => { }, // 每句对话结束的回调（一般不需要做什么）
                    error => observer.OnError(error),
                    () => 
                    {
                        // 3. 序列结束：所有对话都播完了 -> 关闭面板
                        this.SendEvent(new UIPanelEvent(UIPanelType.对话界面, UIPanelAction.Hide));
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                );
        });
    }

    private IObservable<Unit> ShowSingleDialogueInternal(Dialogue dialogue)
    {
        // 使用Defer确保“惰性执行”（只有轮到这句对话时才执行Lambda表达式）
        return Observable.Defer(() =>
        {
            // 发送事件，让UI层显示对话
            this.SendEvent(new ShowDialogueEvent(dialogue));
            return _nextClickSubject.Take(1); // 等待点击信号
        });
    }
}


#region 事件

public class ShowDialogueEvent : AbstractEvent{
    public Dialogue dialogue;
    public ShowDialogueEvent(Dialogue dialogue){
        this.dialogue = dialogue;
    }
}
#endregion