using System.Collections.Generic;
using UnityEngine;
using cfg;
using System.Linq;

public interface IGuideFlow
{
    string FlowID { get; }
    List<GuideStepInfo> Steps { get; }
}
/// <summary>
/// 教程流程配置 - 管理一组教程步骤
/// 支持线性和非线性两种流程模式
/// </summary>
[CreateAssetMenu(fileName = "GuideFlow", menuName = "Tutorial/GuideFlow")]
public class GuideFlow : ScriptableObject, IGuideFlow
{
    [Header("流程信息")]
    [Tooltip("唯一标识符")]
    public string flowID;

    [Header("步骤列表")]
    [Tooltip("教程步骤列表")]
    public List<GuideStepInfo> steps = new List<GuideStepInfo>();

    public string FlowID => flowID;
    public List<GuideStepInfo> Steps => steps;
}


public class GuideFlowInstance : IGuideFlow
{
    public string FlowID { get; private set; }
    public List<GuideStepInfo> Steps { get; private set; }
    public GuideFlowInstance(string flowID, List<GuideStepData> guideStepDatas)
    {
        FlowID = flowID;
        Steps = guideStepDatas.Select(x => new GuideStepInfo(x)).ToList();
    }
}