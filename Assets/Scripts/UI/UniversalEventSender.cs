using System.Collections.Generic;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


public class UniversalEventSender : SerializedMonoBehaviour
{
    
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [OdinSerialize]
    public List<AbstractEvent> events = new List<AbstractEvent>();
    [OdinSerialize]
    public List<AbstractEvent> eventsBack = new List<AbstractEvent>();
    public void SendEvents()
    {
        foreach (var evt in events)
        {
            HandleUniversalEvent(evt);
        }
    }

    public void SendEventsBack(){
        foreach (var evt in eventsBack)
        {
            HandleUniversalEvent(evt);
        }
    }

    private void HandleUniversalEvent(AbstractEvent evt){

        switch (evt.GetType().Name)
        {
            case nameof(ShowClockPreviewEvent):
                ShowClockPreviewEvent showClockPreviewEvent = evt as ShowClockPreviewEvent;
                this.GetArchitecture().SendEvent<ShowClockPreviewEvent>(showClockPreviewEvent);
                break;
            case nameof(HideClockPreviewEvent):
                HideClockPreviewEvent hideClockPreviewEvent = evt as HideClockPreviewEvent;
                this.GetArchitecture().SendEvent<HideClockPreviewEvent>(hideClockPreviewEvent);
                break;
            default:
                Debug.LogError($"未处理的通用事件: {evt.GetType().Name}");
                break;
        }
    }
}