using System;
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

    [Header("触发条件")]
    [Tooltip("监听的玩家行为类型，触发后推进教程")]
    public PlayerActionType triggerAction = PlayerActionType.无;

    [Header("高亮目标（可选）")]
    [Tooltip("对应 GuideTarget.TargetID，留空则不高亮")]
    public string targetID = "";

    [Header("显示配置")]
    [Tooltip("气泡文本显示方向")]
    public GuideTextDirection textDirection = GuideTextDirection.右;
    
    [Tooltip("是否需要玩家点击才能推进（不监听 PlayerAction）")]
    public bool waitForClick = false;
    
    [Tooltip("显示前的延迟时间（秒）")]
    public float delayBeforeShow = 0f;

    [Header("非线性专用")]
    [Tooltip("步骤组ID，同组步骤可任意顺序完成")]
    public string groupID = "";
    
    [Tooltip("是否为可选步骤（跳过不影响流程）")]
    public bool isOptional = false;
}
