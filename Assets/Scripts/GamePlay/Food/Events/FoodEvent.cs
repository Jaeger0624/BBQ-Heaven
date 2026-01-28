
#region 事件

using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

// 创建食材
public class CreateFoodInstanceEvent : AbstractEvent{
    public FoodInstance foodInstance;
    public CreateFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

// 移除食材
public class RemoveFoodInstanceEvent : AbstractEvent{
    public FoodInstance foodInstance;
    public RemoveFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

// 从棋盘上移除食材（放上烤串/移除）
public class FoodRemoveFromBoardEvent : AbstractEvent{
    public readonly string guid;
    public FoodRemoveFromBoardEvent(string guid){
        this.guid = guid;
    }
}

// 
public class AddNewFoodToRepositoryEvent : AbstractEvent{
    public readonly string foodId;
    public readonly int amount;
    public AddNewFoodToRepositoryEvent(string foodId, int amount){
        this.foodId = foodId;
        this.amount = amount;
    }
}

/// <summary>
/// 删除食材仓库事件
/// </summary>
public class DeleteFoodFromRepositoryEvent : AbstractEvent{
    public readonly string guid;
    public DeleteFoodFromRepositoryEvent(string guid){
        this.guid = guid;
    }
}

/// <summary>
/// 更新食材仓库数量事件
/// </summary>
public class UpdateFoodRepositoryAmountEvent : AbstractEvent{
    public readonly Dictionary<string, int> foodRepositoryAmounts;
    public UpdateFoodRepositoryAmountEvent(Dictionary<string, int> foodRepositoryAmounts){
        this.foodRepositoryAmounts = foodRepositoryAmounts;
    }
}

public class ConsumeFoodInstanceEvent : AbstractEvent{
    public FoodInstance foodInstance;
    public ConsumeFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}
#endregion

