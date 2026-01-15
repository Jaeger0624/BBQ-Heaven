using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;


/// <summary>
/// 游戏效果系统 - 系统层
/// 用于处理游戏效果相关的逻辑
/// </summary>
public interface IGASystem : ISystem{
    // 处理GA
    // param是用于让玩家在runtime时传递参数给GA（例如操作决定的参数）
    void ApplyGA(object sender, GameAction gameAction, List<object> param);
    IObservable<Unit> ApplyGAImmediate(object sender, GameAction gameAction, List<object> param);
    // 设置触发器，等待触发器触发后执行
    void SetTrigger(object sender);
    void SendAction(object sender, Action action);
    // 处理CGA
    void ApplyCGA(object sender, CGA cga, List<object> param);

    // 处理SE
    void ApplySE(object sender, SustainEffect sustainEffect);
    void RemoveSE(object sender, SustainEffect sustainEffect);
    // 评估条件
    bool EvaluateConditions(object sender, List<Condition> conditions, List<object> param);
    bool CheckFoodPreview(FoodInstance food, FoodGAType foodGAType);
}

public class GASystem : AbstractSystem, IGASystem{
    # region 队列管理
    // 任务封装
    private class GATask
    {
        public bool IsCGA; // 标记是普通GA还是CGA
        public object Data; // 存 GameAction 或 CGA 对象
        public object Sender;
        public List<object> Param;
    }

    private Queue<GATask> _queue = new Queue<GATask>();
    private bool _isRunning = false; // 队列锁

    #endregion
    protected override void OnInit(){}

    #region  主要外部入口
    /// <summary>
    /// 执行游戏效果
    /// </summary>
    /// <param name="gameAction">游戏效果</param>
    /// <param name="sender">发送者</param>
    public void ApplyGA(object sender, GameAction gameAction, List<object> param)
    {
        EnqueueTask(new GATask 
        { 
            IsCGA = false, 
            Data = gameAction, 
            Sender = sender, 
            Param = param 
        });
    }
    public void SendAction(object sender, Action action)
    {
        GameAction gameAction = new GA_Action(action);
        EnqueueTask(new GATask 
        { 
            IsCGA = false, 
            Data = gameAction, 
            Sender = sender, 
            Param = null 
        });
    }
    public void SetTrigger(object sender)
    {
        Debug.Log($"<color=yellow>【GA_WaitForEvent】开始等待触发...</color>");
        GameAction gameAction = new GA_WaitForEvent();
        EnqueueTask(new GATask 
        { 
            IsCGA = false, 
            Data = gameAction, 
            Sender = sender, 
            Param = null 
        });
    }
    public void ApplyCGA(object sender, CGA cga, List<object> param)
    {
        EnqueueTask(new GATask 
        { 
            IsCGA = true, 
            Data = cga, 
            Sender = sender, 
            Param = param 
        });
    }
    private void EnqueueTask(GATask task)
    {
        _queue.Enqueue(task);
        ProcessQueue();
    }

    #endregion

    #region  核心流水线
    private void ProcessQueue()
    {
        if (_isRunning || _queue.Count == 0) return;

        _isRunning = true;
        var task = _queue.Dequeue();

        // 根据类型选择不同的执行流
        IObservable<Unit> executionStream;

        if (task.IsCGA)
        {
            var cga = task.Data as CGA;
            executionStream = ExecuteCGAStream(task.Sender, cga, task.Param);
        }
        else
        {
            var ga = task.Data as GameAction;
            executionStream = ExecuteSingleGAStream(task.Sender, ga, task.Param);
        }

        // 订阅执行流
        executionStream.Subscribe(
            _ => { }, 
            error => 
            {
                Debug.LogError($"[GASystem] 执行出错: {error}");
                _isRunning = false;
                ProcessQueue(); 
            },
            () => 
            {
                _isRunning = false;
                ProcessQueue(); // 递归处理下一个
            }
        ).AddTo(SettingManager.Instance.gameObject);
    }
    // --- 核心逻辑块 A：执行单个 GA ---
    // 流程：GA.ExecuteAsync -> 拿到结果 -> AnimationSystem.Play -> 结束
    private IObservable<Unit> ExecuteSingleGAStream(object sender, GameAction ga, List<object> param) => ApplyGAImmediate(sender, ga, param);
    public IObservable<Unit> ApplyGAImmediate(object sender, GameAction ga, List<object> param)
    {
        // 复用之前的逻辑：执行异步逻辑 -> 播放动画 -> 等待结束
        return ga.ExecuteAsync(sender, param)
            .SelectMany(result => 
            {
                if (result.AnimTask != null)
                {
                    return this.GetSystem<IAnimationSystem>().PlayTaskAsync(result.AnimTask);
                }
                return Observable.ReturnUnit();
            });
    }

    // --- 核心逻辑块 B：执行 CGA ---
    // 流程：Check Condition -> 遍历子 Actions -> 串行执行(逻辑A->动画A->逻辑B->动画B)
    private IObservable<Unit> ExecuteCGAStream(object sender, CGA cga, List<object> param)
    {
        return Observable.Defer(() => 
        {
            // 1. 评估条件 (同步)
            if (!EvaluateConditions(sender, cga.Conditions, param))
            {
                // Debug.Log($"【GASystem】条件不满足，直接结束: {sender.GetType().Name}");
                return Observable.ReturnUnit(); // 条件不满足，直接结束
            }

            // 2. 串行化执行所有子 GA
            // 使用 Observable.Concat 将列表中的 GA 变成串行流
            // 每一个子 GA 都会复用 ExecuteSingleGAStream 的逻辑 (即包含动画等待)
            var actionStreams = new List<IObservable<Unit>>();
            
            foreach (var subAction in cga.Actions)
            {
                actionStreams.Add(ExecuteSingleGAStream(sender, subAction, param));
            }

            // Concat 会订阅第一个流，等OnCompleted后，再订阅第二个...
            return Observable.Concat(actionStreams);
        });
    }
    #endregion

    public void ApplySE(object sender, SustainEffect sustainEffect)
    {
        if (AddedSEs.TryGetValue(sustainEffect.guid, out SustainEffect se))
        {
            // 若已存在，则修改层数
            se.OnChangeStack(sender, se.StackNumber + 1);
            return;
        }
        // 加入到已添加的SE列表中
        AddedSEs.Add(sustainEffect.guid, sustainEffect);
        sustainEffect.OnAdd(sender);
    }

    public void RemoveSE(object sender, SustainEffect sustainEffect)
    {
        if (!AddedSEs.TryGetValue(sustainEffect.guid, out SustainEffect se))
        {
            Debug.LogError($"由{sender.GetType().Name} 添加的SE {sustainEffect.GetType().Name} 未添加");
            return;
        }
        AddedSEs.Remove(sustainEffect.guid);
        sustainEffect.OnRemove(sender);
    }


    // 基于Guid的SE列表
    private Dictionary<string, SustainEffect> AddedSEs = new Dictionary<string, SustainEffect>();


    public bool EvaluateConditions(object sender, List<Condition> conditions, List<object> param)
    {
        if (conditions == null) return true;
        if (conditions.Count == 0) return true;
        foreach (var condition in conditions){
            if (!condition.Evaluate(sender, param)){
                // AddToContext(condition);
                return false;
            }
        }
        return true;
    }

    public bool CheckFoodPreview(FoodInstance food, FoodGAType triggerType)
    {
        if (food == null || food.food == null) return false;

        // 1. 找到该触发时机下的所有 CGA
        if (!food.food.foodGAs.TryGetValue(triggerType, out List<CGA> cgas))
        {
            return false; // 没有配置这个时机的效果
        }

        // 2. 只要有一个 CGA 的条件满足，就返回 true (或者你可以返回满足的数量)
        foreach (var cga in cgas)
        {
            if (cga.Conditions.Count == 0 || cga.Conditions == null) continue;
            // 这里只评估 Condition，不执行 Action
            // sender 就是 food 本身
            if (EvaluateConditions(food, cga.Conditions, null))
            {
                return true; 
            }
        }

        return false;
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