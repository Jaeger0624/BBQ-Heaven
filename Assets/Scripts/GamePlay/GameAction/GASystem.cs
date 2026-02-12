using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 游戏效果系统 - 系统层
/// 核心职责：维护动作执行树（Action Tree），确保连锁反应的正确顺序（DFS），并提供执行日志
/// </summary>
public interface IGASystem : ISystem
{
    bool IsRunning { get; }
    ReactiveCollection<ActionNode> ActionHistory { get; }
    
// --- 新核心接口 ---
    void AddRootAction(GameAction action, object sender, List<object> args);
    void TriggerReaction(GameAction reactionAction, object sender, List<object> args);
    void TriggerReaction(CGA cga, object sender, List<object> args);
    void SendAction(object sender, Action action);

    // --- 独立功能模块 ---
    void ApplySE(object sender, SustainEffect sustainEffect);
    void RemoveSE(object sender, SustainEffect sustainEffect);
    bool EvaluateConditions(object sender, List<Condition> conditions, List<object> param);
    bool CheckFoodPreview(FoodInstance food, FoodGAType foodGAType);
}

public class GASystem : AbstractSystem, IGASystem
{
    #region 核心状态 (Action Tree)

    // 当前正在处理的“根”动作（树的根节点，用于生成完整日志）
    private ActionNode _currentRootNode;

    // 当前执行焦点（新触发的连锁反应会挂载到这个节点的子列表中，实现优先执行）
    private ActionNode _activeNode;

    // 历史记录（UI日志系统直接读取此列表）
    private ReactiveCollection<ActionNode> _actionHistory = new ReactiveCollection<ActionNode>();
    public ReactiveCollection<ActionNode> ActionHistory => _actionHistory;

    // 锁：防止递归执行过程中意外开启新的根流程
    public bool IsRunning => _isRunning;
    private bool _isRunning = false;

    // SE 状态存储
    private Dictionary<string, SustainEffect> AddedSEs = new Dictionary<string, SustainEffect>();

    #endregion

    protected override void OnInit() { }

    #region 新版核心入口 (AddRoot / TriggerReaction)

    public void AddRootAction(GameAction action, object sender, List<object> args)
    {
        if (action == null) return;

        // 1. 创建根节点
        var node = new ActionNode(action, sender, args, null);

        if (!_isRunning)
        {
            _currentRootNode = node;
            // 2. 启动递归树执行流
            RunTree(node).Subscribe(
                _ => { }, 
                error => Debug.LogError($"[GASystem] Root Execution Error: {error}"),
                () => OnRootActionFinished()
            );
        }
        else
        {
            // 简单处理并发：如果系统正忙，暂时打印警告
            // TODO: 未来可以引入一个 PendingQueue 来排队处理并发的根事件
            Debug.LogWarning($"GASystem Busy: 正在执行 {_activeNode?.ActionData?.GetType().Name}，新请求被忽略/排队");
        }
    }

    public void TriggerReaction(GameAction action, object sender, List<object> args)
    {
        if (action == null) return;

        if (_activeNode == null)
        {
            // 兜底：如果当前没有焦点，当作根节点处理
            AddRootAction(action, sender, args);
            return;
        }

        // 关键逻辑：挂载到当前焦点下，成为子节点
        // 在 RunTree 的逻辑中，子节点会被优先执行（深度优先）
        new ActionNode(action, sender, args, _activeNode);
    }

    public void TriggerReaction(CGA cga, object sender, List<object> args)
    {
        if (cga == null) return;
        if (_activeNode == null)
        {
            AddRootAction(new GA_CGAWrapper(cga), sender, args);
            return;
        }
        new ActionNode(new GA_CGAWrapper(cga), sender, args, _activeNode);
    }

    #endregion

    #region 核心递归执行逻辑 (DFS)

    // 执行单个节点（及其衍生的所有子节点）
    private IObservable<Unit> RunTree(ActionNode node)
    {
        return Observable.Create<Unit>(observer =>
        {
            _isRunning = true;
            _activeNode = node; // 1. 切换焦点到当前节点

            // 2. 执行节点自身的逻辑 (包含 CGA 的特殊处理)
            return ExecuteNodeLogic(node)
                .Concat(Observable.Defer(() =>
                {
                    // 3. 自身逻辑跑完后，立刻检查有没有产生子节点（连锁反应）
                    if (node.Children.Count > 0)
                    {
                        // 如果有，优先执行子节点序列（这就是插队成功的原理）
                        return RunChildrenSequence(node.Children);
                    }
                    return Observable.Return(Unit.Default);
                }))
                .Subscribe(
                    result => { }, 
                    error => {
                        Debug.LogError($"[GASystem] Node Error ({node.ActionData?.GetType().Name}): {error}");
                        observer.OnError(error);
                    },
                    () =>
                    {
                        // 4. 节点及其所有子孙执行完毕
                        node.IsFinished = true;
                        _activeNode = node.Parent; // 焦点回溯给父节点
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                );
        });
    }

    // 串行执行子节点列表
    private IObservable<Unit> RunChildrenSequence(List<ActionNode> children)
    {
        var sequence = Observable.Return(Unit.Default);
        foreach (var child in children)
        {
            // 串行执行所有子节点
            sequence = sequence.Concat(Observable.Defer(() => RunTree(child)));
        }
        return sequence;
    }

    // 真正的逻辑执行 + 动画播放
    private IObservable<Unit> ExecuteNodeLogic(ActionNode node)
    {
        // 这里的 node.ActionData 就是 GameAction
        var ga = node.ActionData;
        var sender = node.Sender;
        var param = node.Params;

        // 执行核心：ExecuteAsync -> Play Animation
        return ga.ExecuteAsync(sender, param)
            .SelectMany(result => 
            {
                // 如果是测试模式，跳过动画
                if (this.GetSystem<IProxySystem>().isTesting)
                {
                    return Observable.ReturnUnit();
                }
                
                // 播放动画
                if (result.AnimTask != null)
                {
                    return this.GetSystem<IAnimationSystem>().PlayTaskAsync(result.AnimTask);
                }
                return Observable.ReturnUnit();
            });
    }

    private void OnRootActionFinished()
    {
        _isRunning = false;
        
        // 保存历史记录供日志UI显示
        if (_currentRootNode != null)
        {
            ActionHistory.Add(_currentRootNode);
            // 限制历史数量防止内存泄漏
            if (ActionHistory.Count > 50) ActionHistory.RemoveAt(0);
        }
        
        _currentRootNode = null;
        _activeNode = null;
    }

    public IObservable<Unit> ApplyCGA(object sender, CGA cga, List<object> param, bool insertAtHead = false)
    {
        // 核心：CGA 在新架构中被视为一个特殊的“容器型动作”
        // 我们需要手动把 CGA 拆解成逻辑，融入树中
        
        // 1. 评估条件
        if (!EvaluateConditions(sender, cga.Conditions, param))
        {
            return Observable.ReturnUnit(); // 条件不满足
        }

        // 2. 如果满足，将 CGA 的 Actions 列表依次加入树中
        // 注意：这里我们模拟成“当前节点触发了这一组 Actions”
        // 如果当前没有运行树，则这一组 Actions 成为新的 Root（串行）
        
        if (cga.Actions != null)
        {
            foreach (var subAction in cga.Actions)
            {
                TriggerReaction(subAction, sender, param);
            }
        }

        return Observable.ReturnUnit();
    }

    public void SendAction(object sender, Action action)
    {
        var wrapper = new GA_Action(action);
        TriggerReaction(wrapper, sender, null);
    }

    #endregion

    #region 辅助功能 (SE & Condition)

    public void ApplySE(object sender, SustainEffect sustainEffect)
    {
        if (AddedSEs.TryGetValue(sustainEffect.guid, out SustainEffect se))
        {
            se.OnChangeStack(sender, se.StackNumber + 1);
            return;
        }
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

    public bool EvaluateConditions(object sender, List<Condition> conditions, List<object> param)
    {
        if (conditions == null || conditions.Count == 0) return true;
        foreach (var condition in conditions)
        {
            if (!condition.Evaluate(sender, param))
            {
                return false;
            }
        }
        return true;
    }

    public bool CheckFoodPreview(FoodInstance food, FoodGAType triggerType)
    {
        if (food == null || food.food == null) return false;

        if (!food.food.foodGAs.TryGetValue(triggerType, out List<CGA> cgas))
        {
            return false;
        }

        foreach (var cga in cgas)
        {
            if (cga.Conditions == null || cga.Conditions.Count == 0) continue;
            if (EvaluateConditions(food, cga.Conditions, null))
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}