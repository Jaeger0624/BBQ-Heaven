
using QFramework;
using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace cfg{
public struct GAResult{
    public IAnimTask AnimTask;
    public static GAResult Empty => new GAResult{ AnimTask = new EmptyAnimTask() };
    public static GAResult FromAnim(IAnimTask animTask) => new GAResult{ AnimTask = animTask };
}

public abstract partial class GameAction : ICanGetSystem, IHaveAnim, ICanSendEvent
{
    public GameAction(){}
    // 默认实现：如果子类没重写 ExecuteAsync，就跑同步逻辑 + 获取 GetAnimTask
    public virtual IObservable<GAResult> ExecuteAsync(object sender, List<object> param)
    {
        return Observable.Create<GAResult>(observer =>
        {
            // 开发期：不捕获异常，让堆栈直接暴露到 Console 便于定位
            // 1. 跑你原来的同步逻辑 (Execute)
            this.Execute(sender, param);
            
            if (this.GetSystem<IProxySystem>().isTesting){
                observer.OnNext(GAResult.Empty);
                observer.OnCompleted();
                return Disposable.Empty;
            }
            // 2. 拿你原来的动画 (GetAnimTask)
            var anim = this.GetAnimTask();
            
            // 3. 发送结果并结束
            observer.OnNext(GAResult.FromAnim(anim));
            observer.OnCompleted();
            
            return Disposable.Empty;
        });
    }
    public abstract void Execute(object sender, List<object> param);
    public virtual void ApplyMultiplier(int multiplier){}
    public abstract IAnimTask GetAnimTask();
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    public abstract GameAction Clone();
    public virtual void SetRelation(object target){}

    /// <summary>
    /// 【核心辅助方法】触发连锁反应
    /// 在具体的 Action 逻辑中调用此方法来插入子效果
    /// </summary>
    /// <param name="subAction">要触发的子动作</param>
    /// <param name="sender">触发者（通常透传当前的 sender）</param>
    /// <param name="args">上下文参数（通常复制当前的 args 或创建新的）</param>
    protected void Chain(GameAction subAction, object sender, List<object> args)
    {
        var gaSystem = GameArchitecture.Interface.GetSystem<IGASystem>();
        if (gaSystem != null)
        {
            gaSystem.TriggerReaction(subAction, sender, args);
        }
    }

    protected void Chain(List<GameAction> gameActionList, object sender, List<object> args)
    {
        if (gameActionList == null) return;
        foreach (var gameAction in gameActionList)
        {
            Chain(gameAction, sender, args);
        }
    }
    /// <summary>
    /// 【重载】触发一个带条件的动作 (CGA)
    /// 系统会自动将其包装为 Wrapper 放入执行树
    /// </summary>
    protected void Chain(CGA cga, object sender, List<object> args)
    {
        if (cga == null) return;

        // 包装成 Node 放入树中
        var wrapper = new GA_CGAWrapper(cga);
        
        // 这里的 Chain 也就是调用 GASystem.TriggerReaction
        Chain(wrapper, sender, args);
    }

    /// <summary>
    /// 【重载】批量触发 CGA 列表
    /// </summary>
    protected void Chain(List<CGA> cgaList, object sender, List<object> args)
    {
        if (cgaList == null) return;
        foreach (var cga in cgaList)
        {
            Chain(cga, sender, args);
        }
    }
    public virtual string GetDescription(object sender, List<object> args)
    {
        // 默认返回类名，子类应该重写这个方法提供更有意义的文本
        return $"执行: {this.GetType().Name}";
    }
}

    public partial class GA_Action : GameAction
    {
        Action<GALogContext> action;
        private GALogContext logContext;
        public GA_Action(Action<GALogContext> action){
            this.action = action;
        }
        public override GameAction Clone()
        {
            return new GA_Action(action);
        }
        public override void Execute(object sender, List<object> param)
        {
            // 创建日志上下文
            logContext = new GALogContext();

            // 执行动作，并提供日志上下文以修改
            action?.Invoke(logContext);
        }
        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }
        public override int GetTypeId()
        {
            return 124532355; // 随便写
        }
        public override string GetDescription(object sender, List<object> args)
        {
            return logContext.Log;
        }
    }


}


