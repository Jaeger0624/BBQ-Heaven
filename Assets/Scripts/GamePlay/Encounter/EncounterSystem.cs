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
        this.RegisterEvent<TimeTickEvent>(OnTimeTickEvent);
        this.RegisterEvent<FinishCombineBBQEvent>(OnCombineBBQEvent);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<TimeTickEvent>(OnTimeTickEvent);
        this.UnRegisterEvent<FinishCombineBBQEvent>(OnCombineBBQEvent);
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
            currentNum = 0,
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
        Debug.Log($"【EncounterSystem】添加事件: {activeEncounter.encounterData.Name}");
        this.SendEvent(new AddEncounterEvent(activeEncounter));
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
        Debug.Log($"【EncounterSystem】移除事件: {activeEncounter.encounterData.Name}");
        this.SendEvent(new RemoveEncounterEvent(activeEncounter));
    }
    // 生命周期管理
    // 时间Tick事件
    private void OnTimeTickEvent(TimeTickEvent evt)
    {
        foreach (var activeEncounter in _activeEncounters)
        {
            if (activeEncounter.encounterData.Type == EncounterType.时间){
                activeEncounter.currentNum += evt.timePoint;
            }
        }
        HandleEncounter();
    }
    // 完成烧烤事件
    private void OnCombineBBQEvent(FinishCombineBBQEvent evt)
    {
        foreach (var activeEncounter in _activeEncounters)
        {
            if (activeEncounter.encounterData.Type == EncounterType.串数)
            {
                activeEncounter.currentNum ++;
            }
        }
        HandleEncounter();
    }
    private void HandleEncounter(){
        foreach (var activeEncounter in _activeEncounters)
        {
            if (activeEncounter.currentNum >= activeEncounter.totalNum)
            {
                EndEncounter(activeEncounter.encounterData.ID);
            }
        }
    }
}
