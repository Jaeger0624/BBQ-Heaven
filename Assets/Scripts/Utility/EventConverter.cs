using System;
using System.Runtime.CompilerServices;
using cfg;
using QFramework;
using UnityEngine;

public static class EventBinder{
    public static IUnRegister Convert(cfg.EventType evt, ICanRegisterEvent observer, Action ApplyActions)
    {
        switch (evt)
        {
            case cfg.EventType.每日开始系统前:
                return observer.RegisterEvent<StartNewDayEvent>(evt => {
                    if (evt.StageMeet(EventStage.Before))
                    ApplyActions();
                });
            case cfg.EventType.每日开始系统后:
                return observer.RegisterEvent<StartNewDayEvent>(evt => {
                    if (evt.StageMeet(EventStage.After))
                    ApplyActions();
                });
            case cfg.EventType.完成烧烤后:
                return observer.RegisterEvent<FinishCombineBBQEvent>(evt => {
                    ApplyActions();
                });
            case cfg.EventType.抽牌后:
                return observer.RegisterEvent<DrawSingleCardEvent>(evt => {
                    ApplyActions();
                });
            default:
                Debug.LogError($"不支持的事件类型: {evt}");
                return null;
        }
    }
}