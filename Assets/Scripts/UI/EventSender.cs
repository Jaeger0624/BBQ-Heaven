using System;
using System.Collections.Generic;
using QFramework;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public abstract class EventSender : MonoBehaviour, ICanSendEvent, IController, ITriggerComponent
{
    public Action TriggerEvent { get => 发送事件; }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract void 发送事件();    
}
