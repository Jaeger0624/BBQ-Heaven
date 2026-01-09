using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 遭遇系统
/// </summary>
public interface IEncounterSystem : ISystem{
    // 持续型遭遇
    void StartEncounter(string id);
    void EndEncounter(string id);
    // 瞬间型遭遇
    IObservable<string> TriggerInstantEncounter(string id, List<object> param);
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
    private List<object> currentParams = new List<object>();

    // 使用 AsyncSubject 来等待当前遭遇完成
    private AsyncSubject<string> _currentEncounterSubject;
    protected override void OnInit()
    {
        // 1. 监听时间Tick事件
        this.RegisterEvent<TimeTickEvent>(OnTimeTickEvent);
        // 2. 监听完成烧烤事件
        this.RegisterEvent<FinishCombineBBQEvent>(OnCombineBBQEvent);
        // 3. 监听选择选项事件
        this.RegisterEvent<SelectOptionEvent>(OnSelectOptionEvent);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<TimeTickEvent>(OnTimeTickEvent);
        this.UnRegisterEvent<FinishCombineBBQEvent>(OnCombineBBQEvent);
        this.UnRegisterEvent<SelectOptionEvent>(OnSelectOptionEvent);
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

    // 触发瞬时遭遇必须要提供上下文
    public IObservable<string> TriggerInstantEncounter(string id, List<object> param)
    {
        // 0. 上下文初始化
        currentParams = new List<object>();
        currentParams.AddRange(param);

        // 1. 获取瞬间遭遇数据
        InstantEncounterData instantEncounterData = this.GetSystem<IDataSystem>().GetInstantEncounterData(id);
        if (instantEncounterData == null) {Debug.LogError($"瞬间遭遇 {id} 不存在"); return Observable.Return($"瞬间遭遇{id}不存在");}

        // 2. 获取选项
        List<OptionData> options = instantEncounterData.Options.Select(x => this.GetSystem<IDataSystem>().GetOptionData(x)).ToList();
        if (options.Count == 0){Debug.LogError($"瞬间遭遇 {id} 没有选项"); return Observable.Return($"瞬间遭遇{id}没有选项");}

        // 3. 创建瞬间遭遇实例
        InstantEncounter instantEncounter = new InstantEncounter(instantEncounterData, options);

        // 4. 发送事件
        this.SendEvent(new TriggerInstantEncounterEvent(instantEncounter));
        Debug.Log($"【EncounterSystem】触发瞬间遭遇: {instantEncounter.Name}");

        // 5. 实例化 AsyncSubject
        _currentEncounterSubject = new AsyncSubject<string>();

        return _currentEncounterSubject;
    }
    private void OnSelectOptionEvent(SelectOptionEvent evt) => HandleOption(evt.optionData);
    private void HandleOption(OptionData optionData)
    {
        Debug.Log($"【EncounterSystem】选择选项: {optionData.Name}");
        CGA cga = new CGA(optionData.Action);
        this.GetSystem<IGASystem>().ApplyCGA(this, cga, currentParams);

        if (_currentEncounterSubject != null)
        {
            _currentEncounterSubject.OnNext(optionData.Name);
            _currentEncounterSubject.OnCompleted();
            _currentEncounterSubject = null;
        }
        else{
            Debug.LogError("【EncounterSystem】当前遭遇不存在");
            return;
        }
    }
}