using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程流程配置 - 管理一组教程步骤
/// 支持线性和非线性两种流程模式
/// </summary>
[CreateAssetMenu(fileName = "GuideFlow", menuName = "Tutorial/GuideFlow")]
public class GuideFlow : ScriptableObject
{
    [Header("流程信息")]
    [Tooltip("唯一标识符")]
    public string flowID;

    [Tooltip("是否为线性流程（true=按顺序执行，false=可跳步）")]
    public bool isLinear = true;

    [Header("步骤列表")]
    [Tooltip("教程步骤列表")]
    public List<GuideStepInfo> steps = new List<GuideStepInfo>();

    [Header("流程配置")]
    [Tooltip("是否允许跳过整个教程")]
    public bool canSkip = false;
    
    [Tooltip("是否暂停游戏时间（调用 TimeSystem）")]
    public bool pauseGame = false;

    [Header("完成奖励（可选）")]
    [Tooltip("完成教程后触发的奖励")]
    public string rewardID = "";
}
