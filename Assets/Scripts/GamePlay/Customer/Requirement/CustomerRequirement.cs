using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

public enum RequirementConflictGroup{
    无,
    食材数量
}

public abstract class CustomerRequirement{
    public abstract string Name { get; }
    public abstract RequirementConflictGroup ConflictGroup { get; }
    public int StarAmount;

    public abstract bool IsMet(Customer customer, List<object> context);
    public abstract void Init(Customer customer, List<object> context, Rng rng);
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
    protected abstract bool ConcreteConflicted(CustomerRequirement otherRequirement);
    public abstract CustomerRequirement Clone();
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
    private ContainType containType;

    public override bool IsMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}



        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        int finalAmount = 0;

        // 选择包含类型
        containType = rng.PickOne(Enum.GetValues(typeof(ContainType)).Cast<ContainType>().ToList());
        // 1~3个
        int typeAmount = rng.NextInt(1, 4);
        foodTypes = rng.PickMany(Enum.GetValues(typeof(FoodType)).Cast<FoodType>().ToList(), typeAmount);

        // 组合得到最终星数
        switch (containType){
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
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_是否包含食材种类();
    }
}


// 例如：多于3个 -> 1星
public class Requirement_食材少于 : CustomerRequirement{
    public override string Name => "食材数量";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材少于();
    }
}

public class Requirement_食材多于 : CustomerRequirement{
    public override string Name => "食材数量";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材多于();
    }
}

public class Requirement_食材范围 : CustomerRequirement{
    public override string Name => "食材范围";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材范围();
    }
}

// 变种：
// 例如：
public class Requirement_食材丰富度 : CustomerRequirement{
    public override string Name => "食材丰富度";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材丰富度();
    }
}


// 变种：
// 例如：触发一个配方 -> 1星
// 例如：触发两个配方 -> 1.5星
// 例如：触发至少3个配方 -> 2星
public class Requirement_配方数量 : CustomerRequirement{
    public override string Name => "配方数量";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_配方数量();
    }
}



public class Requirement_触发配方 : CustomerRequirement{
    public override string Name => "触发配方";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_触发配方();
    }
}


public class Requirement_具体食材站位 : CustomerRequirement{
    public override string Name => "具体食材站位";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    public override bool IsMet(Customer customer, List<object> context){
        return false;
    }
    public override void Init(Customer customer, List<object> context, Rng rng){
        return;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_具体食材站位();
    }
}   