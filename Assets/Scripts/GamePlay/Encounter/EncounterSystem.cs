using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

/// <summary>
/// 遭遇系统
/// </summary>
public interface IEncounterSystem : ISystem{
    void StartEncounter(string id);
    void EndEncounter(string id);
    List<ActiveEncounter> ActiveEncounters { get; }
}

// 运行时包装类
public class ActiveEncounter{
    public EncounterData encounterData;
    public int currentNum;
    public int totalNum;
    public List<SustainEffect> RuntimeSEs;
}

public class EncounterSystem : AbstractSystem, IEncounterSystem
{
    Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IEncounterSystem>();
    List<ActiveEncounter> _activeEncounters = new List<ActiveEncounter>();
    public List<ActiveEncounter> ActiveEncounters => _activeEncounters;
    protected override void OnInit()
    {
    }
    protected override void OnDeinit()
    {
    }
    public void StartEncounter(string id)
    {   
        EncounterData encounterData = this.GetSystem<IDataSystem>().GetEncounterData(id);
        if (encounterData == null)
        {
            Debug.LogError($"遭遇 {id} 不存在");
            return;
        }
        ActiveEncounter activeEncounter = new ActiveEncounter{
            encounterData = encounterData,
            currentNum = encounterData.Num,
            totalNum = encounterData.Num,
            // RuntimeSE = encounterData.SE.Clone(),
            RuntimeSEs = new List<SustainEffect>(encounterData.SEs.Select(x => x.Clone()))
        };
        // 添加到GA系统
        foreach (var runtimeSE in activeEncounter.RuntimeSEs)
        {
            this.GetSystem<IGASystem>().ApplySE(this, runtimeSE);
        }
        _activeEncounters.Add(activeEncounter);
    }
    public void EndEncounter(string id)
    {
        ActiveEncounter activeEncounter = _activeEncounters.FirstOrDefault(x => x.encounterData.ID == id);
        if (activeEncounter == null)
        {
            Debug.LogError($"遭遇 {id} 不存在");
            return;
        }
        foreach (var runtimeSE in activeEncounter.RuntimeSEs)
        {
            this.GetSystem<IGASystem>().RemoveSE(this, runtimeSE);
        }
        _activeEncounters.Remove(activeEncounter);
    }
}
