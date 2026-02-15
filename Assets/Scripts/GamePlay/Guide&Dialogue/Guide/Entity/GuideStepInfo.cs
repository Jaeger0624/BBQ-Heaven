using System;
using cfg;
using UnityEngine;

/// <summary>
/// 教程步骤配置 - 单个教程步骤的数据
/// 用于配置教程中的每一个步骤，包括显示文本、触发条件、高亮目标等
/// </summary>
[CreateAssetMenu(fileName = "GuideStepInfo", menuName = "Tutorial/GuideStep")]
public class GuideStepInfo : ScriptableObject
{
    [Header("基础信息")]
    [TextArea(3, 10)]
    [Tooltip("教程气泡中显示的文字")]
    public string guideText = "这是教程文本";
    public PlayerActionType triggerAction = PlayerActionType.无;
    public GuideTargetType target = GuideTargetType.无;
    [Tooltip("是否需要玩家点击才能推进（不监听 PlayerAction）")]
    public bool waitForClick = false;
    public bool isConcerned = false;
    private GuideStepData _guideStepData;
    public GuideStepInfo(GuideStepData guideStepData)
    {
        _guideStepData = guideStepData;
        guideText = guideStepData.GuideText;
        triggerAction = guideStepData.TriggerAction;
        target = guideStepData.Target;
        isConcerned = guideStepData.IsConcerned;
    }
}
