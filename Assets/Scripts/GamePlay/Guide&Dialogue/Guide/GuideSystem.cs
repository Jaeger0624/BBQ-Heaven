using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UniRx;
using System;

/// <summary>
/// 教程系统接口 - 管理教程流程和步骤
/// </summary>
public interface IGuideSystem : ISystem 
{
    // ========== 现有方法 ==========
    void RegisterTarget(GuideTarget target);
    void UnregisterTarget(GuideTarget target);
    GuideTarget GetTarget(string targetID);

    void StartGuide(GuideFlow flow);
    void StopGuide();
    void PauseGuide();
    void ResumeGuide();

    void NextStep();
    void SkipStep();

    bool IsGuideActive { get; }
    string CurrentFlowID { get; }
    int CurrentStepIndex { get; }
    GuideStepInfo CurrentStep { get; }
}

/// <summary>
/// 教程系统实现 - 管理教程流程、步骤推进和玩家行为监听
/// </summary>
public class GuideSystem : AbstractSystem, IGuideSystem
{
    // ========== 依赖注入 ==========
    private Dictionary<string, GuideTarget> _targets = new Dictionary<string, GuideTarget>();
    
    // ========== 教程状态 ==========
    private GuideFlow _currentFlow;
    private int _currentStepIndex = -1;
    private bool _isPaused = false;
    private CompositeDisposable _guideDisposables = new CompositeDisposable();
    
    // ========== 非线性流程专用 ==========
    private HashSet<int> _completedSteps = new HashSet<int>();

    // ========== 属性实现 ==========
    public bool IsGuideActive => _currentFlow != null;
    public string CurrentFlowID => _currentFlow?.flowID;
    public int CurrentStepIndex => _currentStepIndex;
    public GuideStepInfo CurrentStep => 
        (_currentFlow != null && _currentStepIndex >= 0 && _currentStepIndex < _currentFlow.steps.Count) 
            ? _currentFlow.steps[_currentStepIndex] 
            : null;

    protected override void OnInit()
    {
        // 监听玩家动作事件
        this.RegisterEvent<PlayerActionEvent>(OnPlayerAction);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<PlayerActionEvent>(OnPlayerAction);
    }

    // ========== 现有方法实现 ==========
    public void RegisterTarget(GuideTarget target)
    {
        if (string.IsNullOrEmpty(target.TargetID)) return;
        if (!_targets.ContainsKey(target.TargetID))
        {
            _targets.Add(target.TargetID, target);
        }
    }

    public void UnregisterTarget(GuideTarget target)
    {
        if (string.IsNullOrEmpty(target.TargetID)) return;
        if (_targets.ContainsKey(target.TargetID))
        {
            _targets.Remove(target.TargetID);
        }
    }

    public GuideTarget GetTarget(string targetID)
    {
        if (_targets.TryGetValue(targetID, out var target))
        {
            return target;
        }
        Debug.LogWarning($"[GuideSystem] 找不到目标: {targetID}, 请检查Target是否激活或ID拼写");
        return null;
    }

    // ========== 新增方法 - 流程控制实现 ==========
    /// <summary>
    /// 启动指定教程
    /// </summary>
    public void StartGuide(GuideFlow flow)
    {
        if (IsGuideActive)
        {
            Debug.LogWarning($"[GuideSystem] 已有教程在运行: {CurrentFlowID}，请先停止");
            return;
        }
        if (flow == null)
        {
            Debug.LogError($"[GuideSystem] 教程流程为空");
            return;
        }
        // 2. 初始化状态
        _currentFlow = flow;
        _currentStepIndex = 0;
        _isPaused = false;
        _completedSteps.Clear();

        // 3. 发送教程开始事件
        this.SendEvent(new GuideStartEvent(flow));

        // 4. 显示第一步
        ShowCurrentStep();

        Debug.Log($"[GuideSystem] 教程启动: {flow.flowID}");
    }

    /// <summary>
    /// 停止当前教程
    /// </summary>
    public void StopGuide()
    {
        if (!IsGuideActive) return;

        var flowID = CurrentFlowID;
        var stoppedIndex = _currentStepIndex;

        // 隐藏当前步骤
        HideCurrentStep();

        // 发送停止事件
        this.SendEvent(new GuideStopEvent(flowID, stoppedIndex));

        // 清理状态
        _currentFlow = null;
        _currentStepIndex = -1;
        _isPaused = false;
        _completedSteps.Clear();

        Debug.Log($"[GuideSystem] 教程停止: {flowID}");
    }

    /// <summary>
    /// 暂停教程（保留当前状态）
    /// </summary>
    public void PauseGuide()
    {
        if (!IsGuideActive || _isPaused) return;
        
        _isPaused = true;
        HideCurrentStep();
        
        Debug.Log($"[GuideSystem] 教程暂停: {CurrentFlowID}");
    }

    /// <summary>
    /// 恢复教程
    /// </summary>
    public void ResumeGuide()
    {
        if (!IsGuideActive || !_isPaused) return;
        
        _isPaused = false;
        ShowCurrentStep();
        
        Debug.Log($"[GuideSystem] 教程恢复: {CurrentFlowID}");
    }

    // ========== 新增方法 - 步骤控制实现 ==========
    /// <summary>
    /// 推进到下一步
    /// </summary>
    public void NextStep()
    {
        if (!IsGuideActive || _isPaused) return;

        var prevStepIndex = _currentStepIndex;
        var prevStep = CurrentStep;

        // 隐藏当前步骤
        HideCurrentStep();

        // 标记当前步骤为已完成（非线性流程）
        _completedSteps.Add(_currentStepIndex);
        
        // 推进索引
        _currentStepIndex++;

        // 检查是否完成
        if (_currentStepIndex >= _currentFlow.steps.Count)
        {
            CompleteGuide();
            return;
        }

        // 发送步骤推进事件
        this.SendEvent(new GuideStepAdvanceEvent(
            CurrentFlowID, 
            prevStepIndex, 
            _currentStepIndex, 
            prevStep, 
            CurrentStep
        ));

        // 显示下一步
        ShowCurrentStep();

        Debug.Log($"[GuideSystem] 步骤推进: {prevStepIndex} -> {_currentStepIndex}");
    }

    /// <summary>
    /// 跳过当前步骤（仅非线性流程）
    /// </summary>
    public void SkipStep()
    {
        if (!IsGuideActive || _isPaused) return;
        
        // 标记为已完成
        _completedSteps.Add(_currentStepIndex);
        
        // 检查是否所有步骤都完成
        if (CheckNonLinearComplete())
        {
            CompleteGuide();
        }
        else
        {
            // 找到下一个未完成的步骤
            MoveToNextUncompletedStep();
        }
    }

    // ========== 私有方法 ==========
    /// <summary>
    /// 显示当前步骤
    /// </summary>
    private void ShowCurrentStep()
    {
        var step = CurrentStep;
        if (step == null) return;

        // 发送步骤显示事件
        this.SendEvent(new GuideStepShowEvent(CurrentFlowID, _currentStepIndex, step));
    }

    /// <summary>
    /// 隐藏当前步骤
    /// </summary>
    private void HideCurrentStep()
    {
        var step = CurrentStep;
        if (step == null) return;

        // 发送步骤隐藏事件
        this.SendEvent(new GuideStepHideEvent(CurrentFlowID, _currentStepIndex, step));
    }

    /// <summary>
    /// 完成教程
    /// </summary>
    private void CompleteGuide()
    {
        var flowID = CurrentFlowID;
        var flow = _currentFlow;

        // 发送完成事件
        this.SendEvent(new GuideCompleteEvent(flowID, flow));

        // 清理状态
        _currentFlow = null;
        _currentStepIndex = -1;
        _isPaused = false;
        _completedSteps.Clear();

        Debug.Log($"[GuideSystem] 教程完成: {flowID}");
    }

    /// <summary>
    /// 玩家动作事件处理
    /// </summary>
    private void OnPlayerAction(PlayerActionEvent evt)
    {
        Debug.Log($"[GuideSystem] 玩家动作事件处理: {evt.actionType}");
        if (!IsGuideActive || _isPaused) return;

        var currentStep = CurrentStep;
        if (currentStep == null) return;

        // 线性流程：严格匹配当前步骤
        if (currentStep.triggerAction == evt.actionType)
        {
            NextStep();
        }
    }

    /// <summary>
    /// 检查非线性流程进度
    /// </summary>
    private void CheckNonLinearProgress(PlayerActionType actionType)
    {
        // 遍历所有步骤，找到匹配的未完成步骤
        for (int i = 0; i < _currentFlow.steps.Count; i++)
        {
            if (_completedSteps.Contains(i)) continue;

            var step = _currentFlow.steps[i];
            if (step.triggerAction == actionType)
            {
                // 标记为已完成
                _completedSteps.Add(i);

                // 如果是当前步骤，推进到下一步
                if (i == _currentStepIndex)
                {
                    NextStep();
                }
                // 如果不是当前步骤，检查是否所有步骤都完成
                else if (CheckNonLinearComplete())
                {
                    CompleteGuide();
                }

                return;
            }
        }
    }
    /// <summary>
    /// 检查非线性流程是否完成
    /// </summary>
    private bool CheckNonLinearComplete()
    {
        foreach (var step in _currentFlow.steps)
        {
            int index = _currentFlow.steps.IndexOf(step);
            if (!_completedSteps.Contains(index))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 移动到下一个未完成的步骤
    /// </summary>
    private void MoveToNextUncompletedStep()
    {
        for (int i = 0; i < _currentFlow.steps.Count; i++)
        {
            if (!_completedSteps.Contains(i))
            {
                HideCurrentStep();
                _currentStepIndex = i;
                ShowCurrentStep();
                return;
            }
        }
    }
}
