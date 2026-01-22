using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

public class Requirement_食材连续 : CustomerRequirement{
    public override string Name => "食材连续";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    private int targetAmount;
    private FoodType foodType;
    public bool specificType = false;
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context.FirstOrDefault() as DealContext;
        if (dealContext == null){
            Debug.LogError("DealContext is null");
            return false;
        }
        // 如果数量都不满足，直接返回false
        if (dealContext.BBQ.foodInstances.Count(x => x.food.foodData.Type == foodType) < targetAmount){
            return false;
        }
        int maxAmount = 0;
        string currentFoodID = null;
        if (specificType){
            foreach (FoodInstance foodInstance in dealContext.BBQ.foodInstances){
                if (currentFoodID == null){
                    currentFoodID = foodInstance.food.foodData.ID;
                    maxAmount = 1;
                }
                else{
                    if (currentFoodID == foodInstance.food.foodData.ID){
                        maxAmount++;
                    }
                    else{
                        maxAmount = 1;
                        currentFoodID = foodInstance.food.foodData.ID;
                    }
                }
                if (maxAmount >= targetAmount){
                    return true;
                }
            }
        }
        else{
        // 位置必须连续
            foreach (FoodInstance foodInstance in dealContext.BBQ.foodInstances){
                if (foodInstance.food.foodData.Type == foodType){
                    maxAmount += 1;
                    if (maxAmount >= targetAmount){
                        return true;
                    }
                    continue;
                }
                else{
                    maxAmount = 0;
                }
            }
        }
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
        StarAmount = targetAmount;
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