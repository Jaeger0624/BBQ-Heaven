using System.Collections.Generic;
using UnityEngine;
using QFramework;

public interface IGuideSystem : ISystem 
{
    void RegisterTarget(GuideTarget target);
    void UnregisterTarget(GuideTarget target);
    GuideTarget GetTarget(string targetID);
}
public class GuideSystem : AbstractSystem, IGuideSystem
{
    private Dictionary<string, GuideTarget> _targets = new Dictionary<string, GuideTarget>();

    protected override void OnInit() { }

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
}