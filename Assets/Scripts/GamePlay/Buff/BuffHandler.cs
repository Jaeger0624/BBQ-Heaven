using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public abstract class BuffHandler : ICanRegisterEvent, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract cfg.EventType eventType { get; }
    private List<IUnRegister> eventUnRegisters = new List<IUnRegister>();
    public virtual void Register(){
        eventUnRegisters.Add(EventBinder.Convert(eventType, this, OnTrigger));

        InternalRegister();
    }
    public abstract void InternalRegister();
    public virtual void Unregister(){
        foreach (var eventUnRegister in eventUnRegisters){
            eventUnRegister.UnRegister();
        }
        eventUnRegisters.Clear();
    }
    public abstract void OnTrigger(List<object> parameters);
}


// 【丝滑】使用卡牌后，抽一张卡
public class BuffHandler_丝滑: BuffHandler{
    public override cfg.EventType eventType => cfg.EventType.使用卡牌;
    public override void OnTrigger(List<object> parameters){
        int removeBuff = this.GetSystem<IBuffSystem>().RemoveBuff("softness",1);
        if (removeBuff == 0){
            return;
        }
        Debug.Log("【Buff：丝滑】触发");

        this.GetSystem<ICardSystem>().DrawCard(1);
    }
    public override void InternalRegister(){}
}
