using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public enum RequirementConflictGroup{
    无,
    食材数量,
}

public abstract class CustomerRequirement : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string Name { get; }
    public abstract RequirementConflictGroup ConflictGroup { get; }
    public int StarAmount;
    private bool isInited = false;
    public bool IsMet(Customer customer, List<object> context){
        if (!isInited){
            Debug.LogError("要求未初始化");
            return false;
        }
        return InternalCheckMet(customer, context);
    }
    public void Init(Customer customer, List<object> context, Rng rng){
        isInited = true;
        InternalInit(customer, context, rng);
    }
    public bool IsConflicted(CustomerRequirement otherRequirement){
        // 1. 是否是自己，若是，则返回true
        if (this.GetType() == otherRequirement.GetType()){
            return true;
        }

        // 2. 若存在冲突组且匹配，则直接返回true
        if (ConflictGroup != RequirementConflictGroup.无 && ConflictGroup == otherRequirement.ConflictGroup){
            return true;
        }
        else{
            // 若不存在冲突组或不匹配，则需要判断具体冲突
            return ConcreteConflicted(otherRequirement);
        }
    }
    protected abstract void InternalInit(Customer customer, List<object> context, Rng rng);
    protected abstract bool ConcreteConflicted(CustomerRequirement otherRequirement);
    protected abstract bool InternalCheckMet(Customer customer, List<object> context);
    public abstract CustomerRequirement Clone();
    public abstract string GetDescription();
}


// 变种：包含、只有、没有
// 例如：包含蔬菜、水果食材
// 例如：只有蔬菜、水果 -> 1.5星 (3)
// 例如：没有蔬菜、水果 -> 2星 (4)
public enum ContainType{
    包含,
    只有,
    没有,
}
public class Requirement_是否包含食材种类 : CustomerRequirement{
    public override string Name => "是否包含食材种类";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    
    private List<FoodType> foodTypes = new List<FoodType>();
    public int TypesAmount => foodTypes.Count;
    public ContainType ContainType;

    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}

        switch (ContainType){
            case ContainType.包含:
                foreach (var foodType in foodTypes){
                    if (!dealContext.BBQ.foodInstances.Any(x => x.food.foodData.Type == foodType)){
                        return false;
                    }
                }
                return true;
            case ContainType.只有:
                foreach (var foodInstance in dealContext.BBQ.foodInstances){
                    // 对于每一个食材实例，判断是否包含所有食材类型
                    if (!foodTypes.Contains(foodInstance.food.foodData.Type)){
                        return false;
                    }
                }
                return true;
            case ContainType.没有:
                foreach (var foodType in foodTypes){
                    if (dealContext.BBQ.foodInstances.Any(x => x.food.foodData.Type == foodType)){
                        return false;
                    }
                }
                return true;
            default:
                Debug.LogError("未匹配到包含类型");
                return false;
        }
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        int finalAmount = 0;

        // 选择包含类型
        ContainType = rng.PickOne(Enum.GetValues(typeof(ContainType)).Cast<ContainType>().ToList());
        // 1~3个
        int typeAmount = rng.NextInt(1, 4);
        foodTypes = rng.PickMany(Enum.GetValues(typeof(FoodType)).Cast<FoodType>().Where(x => x != FoodType.无).ToList(), typeAmount);

        // 组合得到最终星数
        switch (ContainType){
            case ContainType.包含:
                // 包含越多难度越高，所以星数越高
                finalAmount = typeAmount;
                break;
            case ContainType.只有:
                // 只有越难度越高，所以星数越高
                finalAmount = 3;
                break;
            case ContainType.没有:
                // 没有越多难度越高，所以星数越高
                finalAmount = typeAmount;
                break;
        }

        StarAmount = finalAmount;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_是否包含食材种类();
    }
    public override string GetDescription(){
        return $"{ContainType} {string.Join(", ", foodTypes.Select(x => x.ToString()))}";
    }
}

// 变种：
// 例如：
public class Requirement_食材丰富度 : CustomerRequirement{
    public override string Name => "食材丰富度";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    private int targetAmount;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}
        int amount = dealContext.BBQ.foodInstances.Select(x => x.food.foodData.Type).Distinct().Count();
        if (amount < targetAmount){
            return false;
        }
        return true;
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        
        // 2种 -> 1星(2)
        // 3种 -> 1.5星(3)
        // 4种 -> 2星(4)
        targetAmount = rng.NextInt(2, 5);
        StarAmount = 6 - targetAmount;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        if (otherRequirement is Requirement_是否包含食材种类 other){
            // switch (other.ContainType){
            //     case ContainType.包含:
            //         return false;
            //     case ContainType.只有:
            //         return true;
            //     case ContainType.没有:
            //         return true;
            // }

            // 先简单写成只要包含食材种类就冲突
            return true; 
        }
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材丰富度();
    }
    public override string GetDescription(){
        return $"至少有{targetAmount}种类型的食材";
    }
}


public enum SpecificFoodPositionType{
    无,
    首或尾,
    正中间,
    最前,
    最后,
}