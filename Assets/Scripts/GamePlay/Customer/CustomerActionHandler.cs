using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

public class CustomerActionHandler : ICanGetSystem, ICanSendEvent, ICanRegisterEvent{
    private List<IUnRegister> unRegisters = new List<IUnRegister>();
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public void Init(){
        unRegisters = new List<IUnRegister>();
        List<CustomerActionType> customerActionTypes = Enum.GetValues(typeof(CustomerActionType)).Cast<CustomerActionType>().ToList();
        customerActionTypes.Remove(CustomerActionType.其他顾客离开时);
        customerActionTypes.Remove(CustomerActionType.出现时);
        customerActionTypes.Remove(CustomerActionType.订单完成后);
        customerActionTypes.Remove(CustomerActionType.离开时);
        customerActionTypes.Remove(CustomerActionType.订单进行时);


        customerActionTypes.ForEach(RegisterSingleAction);
    }
    public void Release(){
        unRegisters.ForEach(unRegister => unRegister.UnRegister());
        unRegisters.Clear();
    }

    public IObservable<Unit> HandleCustomerAction(List<Customer> customers, CustomerActionType customerActionType, List<object> parameters){
        // 1. 获取正在点餐的顾客
        if (customers.Count == 0){Debug.LogWarning("触发顾客动作时，传入的顾客列表为空"); return Observable.ReturnUnit();}

        // Debug.Log($"【CustomerActionHandler】触发顾客动作: {customerActionType}");
        List<IObservable<Unit>> actionsToRun = new List<IObservable<Unit>>();

        // 2. 遍历顾客，执行顾客动作
        foreach (var customer in customers){
            foreach (var tag in customer.customerTags){
                List<CustomerCGA> customerCGAs = tag.CustomerActionCGAs.Where(x => x.Type == customerActionType).ToList();
                foreach (var cga in customerCGAs){
                    actionsToRun.Add(this.GetSystem<IGASystem>().ApplyCGA(customer, cga.Cga, parameters));
                }
            }
        }
        if (actionsToRun.Count == 0){return Observable.ReturnUnit();}
        // 3. 执行顾客动作
        return actionsToRun.Concat().Select(_ => Unit.Default);
    }
    // 与系统外的交互，需要注册事件
    private void RegisterSingleAction(CustomerActionType customerActionType){
        switch(customerActionType){
            case CustomerActionType.串完烤串后:
                IUnRegister unRegister = this.RegisterEvent<FinishCombineBBQEvent>(evt => {
                    List<object> parameters = new List<object>
                    {
                        evt.bbq,
                        evt.context
                    };
                    HandleCustomerAction(this.GetSystem<ICustomerSystem>().OrderingCustomers.ToList(), customerActionType, parameters);
                });
                break;
            default:
                Debug.LogError($"未实现的系统外顾客动作类型: {customerActionType}");
                break;
        }
    }
}