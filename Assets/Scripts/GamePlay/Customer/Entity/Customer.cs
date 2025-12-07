
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
    // 顾客喜好
    public Preference preferences;
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
    public Customer(string name, int patienceMax){
        this.guid = Guid.NewGuid().ToString();
        this.name = name;
        this.PatienceMax = new ReactiveProperty<int>(patienceMax);
        this.PatienceNow = new ReactiveProperty<int>(0);

        // 设置一下喜好
        this.preferences = Preference.CreateRandomPreference();

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

    public string GetDescription(){
        return $"【{name}】喜欢{preferences.foodTypes.FirstOrDefault().ToString()}食材";
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

// 包装类，用于存储顾客满意度相关信息
public class Preference{
    public List<FoodType> foodTypes;
    // 喜欢的食材ID列表
    public List<string> foodIds; 
    // 喜欢的配方ID列表
    public List<string> recipeIds;
    public Preference(List<FoodType> foodTypes, List<string> recipeIds, List<string> foodIds){
        this.foodTypes = foodTypes;
        this.recipeIds = recipeIds;
        this.foodIds = foodIds;
    }

    public static Preference CreateRandomPreference(){
        List<FoodType> foodTypes = new List<FoodType>();
        List<FoodType> allFoodTypes = Enum.GetValues(typeof(FoodType)).Cast<FoodType>().ToList();

        // 随机配备一个食材类型
        foodTypes.AddRange(allFoodTypes.RandomSelect(1));

        List<string> recipeIds = new List<string>();
        List<string> foodIds = new List<string>();
        return new Preference(foodTypes, recipeIds, foodIds);
    }
}