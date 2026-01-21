
using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 顾客实例类 - 实例层
/// </summary>
public class Customer : ICanGetSystem, ICanRegisterEvent, ICanSendEvent{
    public readonly string guid;
    public string name { get; private set; } = "";
    public CustomerLook customerLook;
    // 顾客标签
    public List<ICustomerTag> customerTags;
    // 耐心阈值 
    public ReactiveProperty<int> PatienceMax;
    // 当前耐心值
    public ReactiveProperty<int> PatienceNow;
    // 顾客状态
    public CustomerState state = CustomerState.Waiting;
    // 是否即将离开
    public bool isAboutToLeave = false;
    // 顾客的声望值
    public int reputation = 2;

    // 【新增：顾客需求】
    // 顾客
    public List<CustomerRequirement> requirements;
    public Customer(string name, int patienceMax, int reputation){
        this.guid = Guid.NewGuid().ToString();
        this.name = name;
        this.PatienceMax = new ReactiveProperty<int>(patienceMax);
        this.PatienceNow = new ReactiveProperty<int>(0);
        this.reputation = reputation;
        // 设置一下喜好

        // TODO: 测试版本的配置标签
        customerTags = new List<ICustomerTag>
        {
            CustomerTagFactory.CreateCustomerTag(this.GetSystem<IDataSystem>().GetCustomerTagData("1")),
        };

        this.customerLook = this.GetSystem<ICustomerSystem>().CustomerLookMaker.GetCustomerLook(name, new GetCustomerLookStrategy_纯随机());
    }

    public void SetState(CustomerState state){
        this.state = state;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}

public enum CustomerState{
    /// <summary>
    /// 等待
    /// </summary>
    Waiting,
    /// <summary>
    /// 点餐
    /// </summary>
    Ordering,
    /// <summary>
    /// 已服务
    /// </summary>
    Leaved,
}
