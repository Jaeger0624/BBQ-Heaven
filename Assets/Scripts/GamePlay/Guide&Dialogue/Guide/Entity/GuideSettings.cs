using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideSettings", menuName = "Tutorial/GuideSettings")] 
public class GuideSettings : ScriptableObject
{
    [SerializeField] private GuideFlow defaultGuideFlow;
    public List<GuideFlow> guideFlows = new List<GuideFlow>();
    public GuideFlow GetGuideFlow(string flowName)
    {
        if (string.IsNullOrEmpty(flowName)) return null;
        if (guideFlows == null || guideFlows.Count == 0) return null;
        if (!guideFlows.Any(x => x.name == flowName)) return null;
        return guideFlows.FirstOrDefault(x => x.name == flowName);
    }
    public GuideFlow GetDefaultGuideFlow()
    {
        return defaultGuideFlow;
    }
}