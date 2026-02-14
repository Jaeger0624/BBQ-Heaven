using QFramework;

/// <summary>
/// 教程开始事件 - 当教程流程启动时触发
/// </summary>
public class GuideStartEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 教程流程配置
    /// </summary>
    public GuideFlow flow;

    public GuideStartEvent(string flowID, GuideFlow flow)
    {
        this.flowID = flowID;
        this.flow = flow;
    }
}

/// <summary>
/// 教程步骤显示事件 - 当需要显示某个教程步骤时触发
/// </summary>
public class GuideStepShowEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 步骤索引
    /// </summary>
    public int stepIndex;
    
    /// <summary>
    /// 步骤配置信息
    /// </summary>
    public GuideStepInfo stepInfo;

    public GuideStepShowEvent(string flowID, int stepIndex, GuideStepInfo stepInfo)
    {
        this.flowID = flowID;
        this.stepIndex = stepIndex;
        this.stepInfo = stepInfo;
    }
}

/// <summary>
/// 教程步骤隐藏事件 - 当需要隐藏某个教程步骤时触发
/// </summary>
public class GuideStepHideEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 步骤索引
    /// </summary>
    public int stepIndex;
    
    /// <summary>
    /// 步骤配置信息
    /// </summary>
    public GuideStepInfo stepInfo;

    public GuideStepHideEvent(string flowID, int stepIndex, GuideStepInfo stepInfo)
    {
        this.flowID = flowID;
        this.stepIndex = stepIndex;
        this.stepInfo = stepInfo;
    }
}

/// <summary>
/// 教程步骤推进事件 - 当教程步骤推进时触发
/// </summary>
public class GuideStepAdvanceEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 上一步骤索引
    /// </summary>
    public int previousStepIndex;
    
    /// <summary>
    /// 当前步骤索引
    /// </summary>
    public int currentStepIndex;
    
    /// <summary>
    /// 上一步骤配置
    /// </summary>
    public GuideStepInfo previousStep;
    
    /// <summary>
    /// 当前步骤配置
    /// </summary>
    public GuideStepInfo currentStep;

    public GuideStepAdvanceEvent(string flowID, int prevIndex, int currIndex, 
                                  GuideStepInfo prevStep, GuideStepInfo currStep)
    {
        this.flowID = flowID;
        this.previousStepIndex = prevIndex;
        this.currentStepIndex = currIndex;
        this.previousStep = prevStep;
        this.currentStep = currStep;
    }
}

/// <summary>
/// 教程完成事件 - 当教程流程完成时触发
/// </summary>
public class GuideCompleteEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 教程流程配置
    /// </summary>
    public GuideFlow flow;

    public GuideCompleteEvent(string flowID, GuideFlow flow)
    {
        this.flowID = flowID;
        this.flow = flow;
    }
}

/// <summary>
/// 教程停止事件 - 当教程流程被中断时触发
/// </summary>
public class GuideStopEvent : AbstractEvent
{
    /// <summary>
    /// 教程流程ID
    /// </summary>
    public string flowID;
    
    /// <summary>
    /// 停止时的步骤索引
    /// </summary>
    public int stoppedAtStepIndex;

    public GuideStopEvent(string flowID, int stoppedAtStepIndex)
    {
        this.flowID = flowID;
        this.stoppedAtStepIndex = stoppedAtStepIndex;
    }
}
