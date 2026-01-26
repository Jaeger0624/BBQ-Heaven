
using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using Sirenix.Serialization;
using UniRx;
using UnityEngine;

/// <summary>
/// 顾客实例类 - 实例层
/// </summary>
public class Customer : ICanGetSystem, ICanRegisterEvent, ICanSendEvent{
    public readonly string guid;
    public string name { get; private set; } = "";
    // public CustomerLook customerLook;
    public Sprite customerLook;
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
    [OdinSerialize] public List<CustomerRequirement> requirements;
    public Customer(string name, int patienceMax, int reputation){
        this.guid = Guid.NewGuid().ToString();
        this.name = name;
        this.PatienceMax = new ReactiveProperty<int>(patienceMax);
        this.PatienceNow = new ReactiveProperty<int>(0);
        this.reputation = reputation;
        // 设置一下喜好

        GenerateCustomerTags();

        // this.customerLook = this.GetSystem<ICustomerSystem>().CustomerLookMaker.GetCustomerLook(name, new GetCustomerLookStrategy_纯随机());
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        this.customerLook = rng.PickOne(SettingManager.Instance.ArtSettings.CustomerSprites.sprites);

        // 生成要求
        RefreshRequirements();
    }

    public void SetState(CustomerState state){
        this.state = state;

        // 当顾客状态改变为Ordering时，触发出现时动作
        if (state == CustomerState.Ordering){
            this.GetSystem<ICustomerSystem>().CustomerActionHandler.HandleCustomerAction(new List<Customer>{this}, CustomerActionType.出现时, new List<object>());
        }
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public void ChangePatience(int value){
        if (PatienceNow.Value + value <= 0){
            PatienceNow.Value = 0;
        }
        else{
            PatienceNow.Value += value;
        }
        this.SendEvent(new ChangeCustomerPatienceEvent(this, value));
    }

    public ReviewResult Review(DealContext context){
        if (requirements == null || requirements.Count == 0){
            Debug.LogError($"【Customer】{name} 要求为空，重新生成");
            RefreshRequirements();
        }

        // 1. 初始化
        ReviewResult reviewResult = new ReviewResult();
        reviewResult.TotalStars = 0;
        reviewResult.Records = new List<RequirementCheckRecord>();

        // 2. 检查要求
        foreach (var requirement in requirements){
            bool isMet = requirement.IsMet(this, new List<object>{context});
            reviewResult.Records.Add(new RequirementCheckRecord{
                Description = requirement.GetDescription(),
                StarValue = requirement.StarAmount,
                IsMet = isMet
            });
            if (isMet){
                reviewResult.TotalStars += requirement.StarAmount;
            }
        }

        // 基础分? (可选，比如只要是个串就给1星)
        // result.TotalStars += 1;

        return reviewResult;
    }

    public void RefreshRequirements(){
        RequirementBuilder builder = new RequirementBuilder();
        requirements = builder.GenerateGroup(this);
    }

    public void GenerateCustomerTags(){
        customerTags = new List<ICustomerTag>();
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        List<CustomerTagData> tagDatas = this.GetSystem<IDataSystem>().GetAllCustomerTagData();

        // 随机选取一定数量的标签
        List<CustomerTagData> selectedTagDatas = rng.PickMany(tagDatas, 1);

        customerTags.Add(CustomerTagFactory.CreateCustomerTag(selectedTagDatas[0]));
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


public class ChangeCustomerPatienceEvent : AbstractEvent{
    public Customer customer;
    public int value;
    public ChangeCustomerPatienceEvent(Customer customer, int value){
        this.customer = customer;
        this.value = value;
    }
}