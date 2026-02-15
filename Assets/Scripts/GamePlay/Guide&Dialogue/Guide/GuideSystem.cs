using System.Collections.Generic;
using UnityEngine;
using QFramework;
using UniRx;
using System;
using cfg;
using System.Linq;

/// <summary>
/// 教程系统接口 - 管理教程流程和步骤
/// </summary>
public interface IGuideSystem : ISystem 
{
    void StartGuide(string flowID);
    void StartGuide(IGuideFlow flow);
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
    // ========== 教程状态 ==========
    private IGuideFlow _currentFlow;
    private int _currentStepIndex = -1;
    private bool _isPaused = false;
    private CompositeDisposable _guideDisposables = new CompositeDisposable();
    
    // ========== 非线性流程专用 ==========
    private HashSet<int> _completedSteps = new HashSet<int>();

    // ========== 属性实现 ==========
    public bool IsGuideActive => _currentFlow != null;
    public string CurrentFlowID => _currentFlow?.FlowID;
    public int CurrentStepIndex => _currentStepIndex;
    public GuideStepInfo CurrentStep => 
        (_currentFlow != null && _currentStepIndex >= 0 && _currentStepIndex < _currentFlow.Steps.Count) 
            ? _currentFlow.Steps[_currentStepIndex] 
            : null;

    private List<IGuideFlow> _guideFlows;

    protected override void OnInit()
    {
        _guideFlows = new List<IGuideFlow>();
        InitGuideFlows(this.GetSystem<IDataSystem>().GetAllGuideStepData());
        // 监听玩家动作事件
        this.RegisterEvent<PlayerActionEvent>(OnPlayerAction);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<PlayerActionEvent>(OnPlayerAction);
    }
    private void InitGuideFlows(List<GuideStepData> guideStepDatas)
    {
        // 按照FlowID分组
        var groupedSteps = guideStepDatas.GroupBy(x => x.FlowID);
        foreach (var group in groupedSteps)
        {
            IGuideFlow guideFlow = new GuideFlowInstance(group.Key, group.ToList());
            _guideFlows.Add(guideFlow);
        }
    }


    // ========== 新增方法 - 流程控制实现 ==========
    public void StartGuide(string flowID)
    {
        var flow = _guideFlows.FirstOrDefault(x => x.FlowID == flowID);
        if (flow == null)
        {
            Debug.LogError($"[GuideSystem] 教程流程不存在: {flowID}");
            return;
        }
        StartGuide(flow);
    }
    public void StartGuide(IGuideFlow flow)
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

        Debug.Log($"[GuideSystem] 教程启动: {flow.FlowID}, 共有{flow.Steps.Count}步");
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
        if (_currentStepIndex >= _currentFlow.Steps.Count)
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
        if (step == null)
        {
            Debug.LogError($"[GuideSystem] 当前步骤为空: {CurrentFlowID}");
            return;
        }

        // 发送步骤显示事件
        this.SendEvent(new GuideStepShowEvent(CurrentFlowID, _currentStepIndex, step));
    }

    /// <summary>
    /// 隐藏当前步骤
    /// </summary>
    private void HideCurrentStep()
    {
        var step = CurrentStep;
        if (step == null)
        {
            Debug.LogError($"[GuideSystem] 当前步骤为空: {CurrentFlowID}");
            return;
        }

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
        // Debug.Log($"[GuideSystem] 玩家动作事件处理: {evt.actionType}");
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
        for (int i = 0; i < _currentFlow.Steps.Count; i++)
        {
            if (_completedSteps.Contains(i)) continue;

            var step = _currentFlow.Steps[i];
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
        foreach (var step in _currentFlow.Steps)
        {
            int index = _currentFlow.Steps.IndexOf(step);
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
        for (int i = 0; i < _currentFlow.Steps.Count; i++)
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
