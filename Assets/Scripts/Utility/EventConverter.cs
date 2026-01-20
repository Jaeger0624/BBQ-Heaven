using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using cfg;
using QFramework;
using UnityEngine;

public static class EventBinder{
    public static IUnRegister Convert(cfg.EventType evt, ICanRegisterEvent observer, Action<List<object>> ApplyActions)
    {
        List<object> parameters = new List<object>();
        switch (evt)
        {
            case cfg.EventType.每日开始系统前:
                return observer.RegisterEvent<StartNewDayEvent>(evt => {
                    if (evt.StageMeet(EventStage.Before))
                    parameters.AddRange(evt.parameters);
                    ApplyActions(parameters);
                });
            case cfg.EventType.每日开始系统后:
                return observer.RegisterEvent<StartNewDayEvent>(evt => {
                    if (evt.StageMeet(EventStage.After))
                    parameters.AddRange(evt.parameters);
                    ApplyActions(parameters);
                });
            case cfg.EventType.完成烧烤后:
                return observer.RegisterEvent<FinishCombineBBQEvent>(evt => {
                    parameters.AddRange(evt.parameters);
                    ApplyActions(parameters);
                });
            case cfg.EventType.抽牌后:
                return observer.RegisterEvent<DrawSingleCardEvent>(evt => {
                    parameters.AddRange(evt.parameters);
                    ApplyActions(parameters);
                });
            case cfg.EventType.串串构建时:
                return observer.RegisterEvent<AfterCalculateBBQEvent>(evt => {
                    parameters.AddRange(evt.parameters);
                    ApplyActions(parameters);
                });
            default:
                Debug.LogError($"不支持的事件类型: {evt}");
                return null;
        }
    }
}

public interface IMascotEvent{
    List<object> parameters { get; }
}