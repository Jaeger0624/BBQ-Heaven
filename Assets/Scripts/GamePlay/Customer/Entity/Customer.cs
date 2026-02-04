
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
[Serializable]
public class Customer : ICanGetSystem, ICanRegisterEvent, ICanSendEvent{
    public readonly string guid;
    public string name => meta.name;
    public MetaCustomer meta;
    // 耐心阈值 
    public ReactiveProperty<int> PatienceMax;
    // 当前耐心值
    public ReactiveProperty<int> PatienceNow;
    // 顾客状态
    public CustomerState state = CustomerState.Waiting;
    // 是否已服务
    public bool isServed = false;
    // 是否即将离开
    public bool isAboutToLeave = false;
    // 顾客的声望值
    public int reputation = 2;
    // 顾客的声望值惩罚
    public int reputationPenalty = 1;

    // 【新增：顾客需求】
    // 顾客
    [OdinSerialize] public List<CustomerRequirement> requirements;
    public Customer(MetaCustomer metaCustomer, int patienceMax, int reputation){
        this.guid = Guid.NewGuid().ToString();
        this.meta = metaCustomer;
        this.PatienceMax = new ReactiveProperty<int>(patienceMax);
        this.PatienceNow = new ReactiveProperty<int>(0);
        this.reputation = reputation;

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
            Debug.LogError($"【Customer】{meta.name} 要求为空，重新生成");
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


