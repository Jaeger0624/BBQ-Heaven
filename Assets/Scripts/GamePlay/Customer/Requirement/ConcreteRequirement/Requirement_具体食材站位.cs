
using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;

public class SpecificFoodPositionContext{
    public bool isType = false;
    public FoodCard selectedFoodCard = null;
    public FoodType selectedFoodType = FoodType.无;
    public SpecificFoodPositionType PositionType = SpecificFoodPositionType.无;
}
public class Requirement_具体食材站位 : CustomerRequirement{
    public override string Name => "具体食材站位";
    public override RequirementConflictGroup ConflictGroup => RequirementConflictGroup.无;
    public SpecificFoodPositionContext Context = new SpecificFoodPositionContext();
    protected override bool InternalCheckMet(Customer customer, List<object> context){
        DealContext dealContext = context?.FirstOrDefault() as DealContext;
        if (dealContext == null){ LogKit.E("【CustomerRequirement】上下文为空"); return false;}
        switch (Context.PositionType){
            case SpecificFoodPositionType.首或尾:
                FoodInstance firstFoodInstance = dealContext.BBQ.foodInstances[0];
                FoodInstance lastFoodInstance = dealContext.BBQ.foodInstances[dealContext.BBQ.foodInstances.Count - 1];
                if (Check(firstFoodInstance) || Check(lastFoodInstance)){
                    return true;
                }
                return false;
            case SpecificFoodPositionType.正中间:
                if (dealContext.BBQ.foodInstances.Count % 2 == 0){
                    int middleIndex = dealContext.BBQ.foodInstances.Count / 2 - 1;
                    int middleIndex2 = dealContext.BBQ.foodInstances.Count / 2;
                    FoodInstance middleFoodInstance = dealContext.BBQ.foodInstances[middleIndex];
                    FoodInstance middleFoodInstance2 = dealContext.BBQ.foodInstances[middleIndex2];
                    if (Check(middleFoodInstance) || Check(middleFoodInstance2)){
                        return true;
                    }
                    return false;
                }
                else{
                    int middleIndex = dealContext.BBQ.foodInstances.Count / 2;
                    FoodInstance middleFoodInstance = dealContext.BBQ.foodInstances[middleIndex];
                    if (Check(middleFoodInstance)){
                        return true;
                    }
                    return false;
                }
            case SpecificFoodPositionType.最前:
                firstFoodInstance = dealContext.BBQ.foodInstances[0];
                if (Check(firstFoodInstance)){
                    return true;
                }
                return false;
            case SpecificFoodPositionType.最后:
                lastFoodInstance = dealContext.BBQ.foodInstances[dealContext.BBQ.foodInstances.Count - 1];
                if (Check(lastFoodInstance)){
                    return true;
                }
                return false;
            default:
                LogKit.E("【CustomerRequirement】未匹配到位置类型");
                return false;
        }
    }
    protected override void InternalInit(Customer customer, List<object> context, Rng rng){
        int finalAmount = 0;
        Context = new SpecificFoodPositionContext();
        // 选择具体食材站位类型
        Context.PositionType = rng.PickOne(Enum.GetValues(typeof(SpecificFoodPositionType))
            .Cast<SpecificFoodPositionType>()
            .Where(x => x != SpecificFoodPositionType.无)
            .ToList());

        bool isType = rng.NextBool();
        if (isType){
            // 选择食材类型，更宽松
            finalAmount += 2;
            Context.isType = true;
            Context.selectedFoodType = rng.PickOne(Enum.GetValues(typeof(FoodType)).Cast<FoodType>().Where(x => x != FoodType.无 && x != FoodType.肉类).ToList());
        }
        else{
            // 选择食材卡牌，更严格
            finalAmount += 3;
            Context.isType = false;
            Context.selectedFoodCard = rng.PickOne(this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList());
        }

        // 组合得到最终星数
        switch (Context.PositionType){
            case SpecificFoodPositionType.首或尾:
                finalAmount += 1;
                break;
            case SpecificFoodPositionType.正中间:
                finalAmount += 1;
                break;
            case SpecificFoodPositionType.最前:
                finalAmount += 2;
                break;
            case SpecificFoodPositionType.最后:
                finalAmount += 2;
                break;
        }
        StarAmount = finalAmount;
    }
    protected override bool ConcreteConflicted(CustomerRequirement otherRequirement){
        return false;
    }
    public override CustomerRequirement Clone(){
        return new Requirement_具体食材站位();
    }
    public override string GetDescription(){
        if (Context.isType){
            return $"<color=yellow>{Context.selectedFoodType.ToString()}</color> 在 {Context.PositionType.ToString()}";
        }
        else{
            return $"<color=yellow>{Context.selectedFoodCard.foodData.Name}</color> 在 {Context.PositionType.ToString()}";
        }
    }

    private bool Check(FoodInstance foodInstance){
        if (Context.isType){
            return foodInstance.food.foodData.Type == Context.selectedFoodType;
        }
        else{
            return foodInstance.food.foodData.ID == Context.selectedFoodCard.foodData.ID;
        }
    }
}