using System;
using System.Collections.Generic;
using System.Linq;
using cfg;


public class Requirement_食材连续 : CustomerRequirement{
    public override string Name => "食材连续";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    private int targetAmount;
    private FoodType foodType;
    public bool specificType = false;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context.FirstOrDefault() as DealContext;
        if (dealContext == null){
            LogKit.E("【CustomerRequirement】DealContext is null");
            return false;
        }
        // 如果数量都不满足，直接返回false
        if (dealContext.BBQ.foodInstances.Count < targetAmount){
            LogKit.W($"【CustomerRequirement】食材数量不满足：{dealContext.BBQ.foodInstances.Count} < {targetAmount}");
            return false;
        }
        int maxAmount = 0;
        if (specificType){
            foreach (FoodInstance foodInstance in dealContext.BBQ.foodInstances){
                if (foodInstance.food.foodData.Type == foodType){
                    maxAmount += 1;
                    if (maxAmount >= targetAmount){
                        LogKit.I($"【CustomerRequirement】食材连续：{foodInstance.food.foodData.Type} {maxAmount} >= {targetAmount}");
                        return true;
                    }
                    continue;
                }
                else{
                    maxAmount = 0;
                }
            }
        }
        else{
            // 若有4个食材，判断3个连续，需要2轮
            int round = dealContext.BBQ.foodInstances.Count - targetAmount + 1;
            for (int i = 0; i < round; i++){
                string id = dealContext.BBQ.foodInstances[i].food.foodData.ID;
                for (int j = 0; j < targetAmount; j++){
                    if (dealContext.BBQ.foodInstances[i + j].food.foodData.ID != id){
                        break;
                    }
                    if (j == targetAmount - 1){
                        LogKit.I($"【CustomerRequirement】非具体类型，且食材连续：{id}");
                        return true;
                    }
                }
            }
        }
        LogKit.I($"【CustomerRequirement】食材不连续");
        return false;
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        specificType = rng.NextBool();
        if (specificType){
            foodType = rng.PickOne(Enum.GetValues(typeof(FoodType)).Cast<FoodType>().Where(x => x != FoodType.无 && x != FoodType.肉类).ToList());
        }
        else{
            foodType = FoodType.无;
        }
        // 2~4个
        targetAmount = rng.NextInt(2, 5);
        StarAmount = targetAmount + 1;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        if (otherRequirement is Requirement_食材少于 otherRequirement_食材少于){
            if (targetAmount <= otherRequirement_食材少于.targetAmount){
                return true;
            }
        }
        return false;
    }

    public override CustomerRequirement Clone()
    {
        return new Requirement_食材连续();
    }

    public override string GetDescription()
    {
        if (specificType){
            return $"有{targetAmount}个连续<color=yellow>{foodType.ToString()}</color>";
        }
        else{
            return $"有{targetAmount}个连续相同食材";
        }
    }
}