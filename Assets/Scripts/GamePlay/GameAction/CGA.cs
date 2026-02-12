
using QFramework;
using cfg;
using UniRx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace cfg{
public partial class CGA : ICanGetSystem, IHaveAnim{
    [NonSerialized]
    private List<IHaveAnim> animTasks = new List<IHaveAnim>();
    public CGA(CGA cga){
        this.ID = cga.ID;
        this.Conditions = new List<Condition>(cga.Conditions);
        this.Actions = new List<GameAction>(cga.Actions.Select(x => x.Clone()));
    }
    public CGA(GameAction action){
        this.ID = Guid.NewGuid().ToString();
        this.Conditions = new List<Condition>();
        this.Actions = new List<GameAction>(){action};
    }
    public bool Execute(object sender, List<object> param)
    {
        // 只要有一个条件不满足，就跳过执行
        if (!this.GetSystem<IGASystem>().EvaluateConditions(sender, Conditions, param)){
            return false;
        }
        foreach (var action in Actions){
            //TODO: 让其越过IGASystem？
            // 如果每个GA的执行都单独再通过IGASystem，则需要在这里添加动画任务，但CGA的动画应该是组合动画
            action.Execute(sender, param);
            animTasks.Add(action);
        }
        return true;
    }
        public IAnimTask GetAnimTask()
        {
            if (animTasks.Count == 0) return new EmptyAnimTask();
            // TODO: 显示CGA的组合动画
            List<IAnimTask> animTasksList = new List<IAnimTask>();
            foreach (var action in Actions){
                animTasksList.Add(action.GetAnimTask());
            }
            IAnimTask animTask = new SequenceAnimTask(animTasksList);
            return animTask;
        }

        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }

        public void ApplyMultiplier(int multiplier)
        {
            foreach (var action in Actions)
            {
                action.ApplyMultiplier(multiplier);
            }
        }
    }

/// <summary>
/// CGA 包装器
/// 作用：将配置表中的 CGA 数据包装成一个可执行的 GameAction。
/// 目的：确保 CGA 作为一个节点出现在 ActionTree 中，无论条件是否满足，都能在日志中追踪。
/// </summary>
public class GA_CGAWrapper : GameAction
{
    private CGA _cgaData;

    public GA_CGAWrapper(CGA cgaData)
    {
        _cgaData = cgaData;
    }

    public override GameAction Clone()
    {
        return new GA_CGAWrapper(new CGA(_cgaData));
    }

    public override void Execute(object sender, List<object> param){}

    public override IObservable<GAResult> ExecuteAsync(object sender, List<object> args)
    {
        // 1. 获取 GASystem 引用 (用于检查条件)
        var gaSystem = GameArchitecture.Interface.GetSystem<IGASystem>();
        
        // 2. 检查条件
        // 即使条件不满足，GA_CGAWrapper 这个节点本身已经存在于树中，IsFinished=true，日志可查
        bool isConditionMet = gaSystem.EvaluateConditions(sender, _cgaData.Conditions, args);

        // 3. 根据结果执行分支
        if (isConditionMet)
        {
            // 条件满足：将 CGA 包含的所有子动作 Chain 进去
            // 这些子动作会挂载在 Wrapper 下面，形成层级
            if (_cgaData.Actions != null)
            {
                foreach (var subAction in _cgaData.Actions)
                {
                    // 继承上下文 args
                    Chain(subAction, sender, args);
                }
            }
        }
        else
        {
            // 条件不满足：什么都不做，Wrapper 执行结束
            // Log 系统可以读取 ActionNode 的状态，看到它没有子节点，或者通过扩展字段标记为“条件失败”
        }

        return Observable.Return(GAResult.Empty);
    }

        public override IAnimTask GetAnimTask()
        {
            return new EmptyAnimTask();
        }

        public override int GetTypeId()
        {
            return 1224122;
        }
    }
}


// 拥有动画的任务（如GA、CGA，执行的是动作效果）
public interface IHaveAnim{
    IAnimTask GetAnimTask();
}
public class TriggerGAEvent : AbstractEvent{
    public string triggerName;
    public TriggerGAEvent(string triggerName){
        this.triggerName = triggerName;
    }
}