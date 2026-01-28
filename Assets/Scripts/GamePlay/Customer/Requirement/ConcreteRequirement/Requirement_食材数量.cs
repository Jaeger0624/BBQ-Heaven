
// 例如：多于3个 -> 1星
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Requirement_食材少于 : CustomerRequirement{
    public override string Name => "食材数量";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    public int targetAmount;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}
        int foodAmount = dealContext.BBQ.foodInstances.Count;
        if (foodAmount <= targetAmount){
            return true;
        }
        else{
            return false;
        }
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){

        // 不多于2个 -> 2星(4)
        // 不多于3个 -> 1.5星(3)
        // 不多于4个 -> 1星(2)
        targetAmount = rng.NextInt(2, 5);
        StarAmount = 7 - targetAmount;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材少于();
    }
    public override string GetDescription(){
        return $"食材不多于 {targetAmount}个";
    }
}

public class Requirement_食材多于 : CustomerRequirement{
    public override string Name => "食材数量";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    private int targetAmount;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}
        int foodAmount = dealContext.BBQ.foodInstances.Count;
        if (foodAmount >= targetAmount){
            return true;
        }
        else{
            return false;
        }
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        // 不少于5个 -> 1星(2)
        // 不少于6个 -> 1.5星(3)
        // 不少于7个 -> 2星(4)
        targetAmount = rng.NextInt(5, 8);
        StarAmount = targetAmount - 2;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材多于();
    }
    public override string GetDescription(){
        return $"食材不少于 {targetAmount} 个";
    }
}

public class Requirement_食材范围 : CustomerRequirement{
    public override string Name => "食材范围";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.食材数量;
    private int minAmount;
    private int maxAmount;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ Debug.LogError("上下文为空"); return false;}
        int foodAmount = dealContext.BBQ.foodInstances.Count;
        if (foodAmount >= minAmount && foodAmount <= maxAmount){
            return true;
        }
        else{
            return false;
        }
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        minAmount = rng.NextInt(2, 5);
        maxAmount = rng.NextInt(minAmount + 1, 8);

        int rangeAmount = maxAmount - minAmount + 1;

        // 范围越大越容易，星级越低
        StarAmount = 9 - rangeAmount;
        if (StarAmount >= 6){
            StarAmount = 6;
        }
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_食材范围();
    }
    public override string GetDescription(){
        return $"食材数量在 {minAmount} 到 {maxAmount} 之间";
    }
}