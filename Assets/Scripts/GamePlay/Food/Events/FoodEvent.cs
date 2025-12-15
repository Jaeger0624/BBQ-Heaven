
#region 事件

using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class CreateFoodInstanceEvent : AbstractEvent{
    public FoodInstance foodInstance;
    public CreateFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

public class RemoveFoodInstanceEvent : AbstractEvent{
    public FoodInstance foodInstance;
    public RemoveFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

public class AddNewFoodToRepositoryEvent : AbstractEvent{
    public readonly string foodId;
    public readonly int amount;
    public AddNewFoodToRepositoryEvent(string foodId, int amount){
        this.foodId = foodId;
        this.amount = amount;
    }
}

public class DeleteFoodFromRepositoryEvent : AbstractEvent{
    public readonly string id;
    public DeleteFoodFromRepositoryEvent(string id){
        this.id = id;
    }
}


public class UpdateFoodRepositoryAmountEvent : AbstractEvent{
    public readonly Dictionary<string, int> foodRepositoryAmounts;
    public UpdateFoodRepositoryAmountEvent(Dictionary<string, int> foodRepositoryAmounts){
        this.foodRepositoryAmounts = foodRepositoryAmounts;
    }
}
#endregion

