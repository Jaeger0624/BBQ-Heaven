using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;
[Serializable]
public class FoodPile : ICanGetSystem, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    /// <summary>
    /// 当前食材仓库
    /// </summary>
    public List<Food> FoodSet = new List<Food>();
    public List<Food> DrawFoodPile;
    public Dictionary<string, FoodInstance> FoodInstances;
    public FoodPile(List<Food> foodInventory){
        FoodSet = foodInventory.ToList();
        DrawFoodPile = new List<Food>();
        FoodInstances = new Dictionary<string, FoodInstance>();
    }
    public void LoadFoodPile(FoodPile foodPile){
        // 1. 设置FoodSet
        FoodSet = foodPile.FoodSet.ToList();
        
        // 2. 创建新的FoodInstances
        foreach (var foodInstance in foodPile.FoodInstances){
            CreateFoodInstance(foodInstance.Value.food, foodInstance.Value.position);
        }
    }
    public void Init(){
        DrawFoodPile = FoodSet.ToList();
        FoodInstances = new Dictionary<string, FoodInstance>();
    }
    public void AddFoodToSet(Food food, bool alsoToDrawPile){
        FoodSet.Add(food);
        if (alsoToDrawPile){
            DrawFoodPile.Add(food);
        }
    }
    public FoodInstance CreateFoodInstance(Food food, Vector2Int position){
        // 1. 创建食材实例
        FoodInstance foodInstance = new FoodInstance(food, position);
        // 2. 设置食材实例状态
        foodInstance.SetState(FoodInstanceState.棋盘上);
        // 3. 设置位置
        this.GetSystem<IBoardSystem>().SetCellInstance(position, foodInstance.guid);
        // 4. 添加到食材实例列表
        FoodInstances.Add(foodInstance.guid, foodInstance);
        // 5. 注册到棋盘实体系统
        this.GetSystem<IBoardEntitySystem>().RegisterEntity(foodInstance, position);

        this.SendEvent(new CreateFoodInstanceEvent(foodInstance));
        Dictionary<string, int> foodRepositoryAmounts = this.GetSystem<IFoodSystem>().GetFoodRepositoryAmounts();
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(foodRepositoryAmounts));
        
        if (DrawFoodPile.Contains(food)){
            DrawFoodPile.Remove(food);
        }
        else{
            Debug.LogError($"【FoodPile】创建食材实例失败: {food.name} 不在抽牌堆中");
            return null;
        }
        return foodInstance;
    }

    public void RemoveFoodInstance(string guid){
        if (!FoodInstances.TryGetValue(guid, out FoodInstance foodInstance)){
            Debug.LogError($"【FoodPile】移除食材实例失败: {guid} 不存在");
            return;
        }
        if (foodInstance.position != new Vector2Int(-1, -1) || foodInstance.state == FoodInstanceState.棋盘上){
            // 从棋盘上移除
            this.GetSystem<IBoardSystem>().SetCellInstance(foodInstance.position, null);
        }
        // 从棋盘实体系统中移除
        this.GetSystem<IBoardEntitySystem>().UnregisterEntity(foodInstance);

        // 发送移除食材实例事件
        this.SendEvent(new RemoveFoodInstanceEvent(foodInstance));

        FoodInstances.Remove(guid);
        if (DrawFoodPile.Contains(foodInstance.food)){
            DrawFoodPile.Remove(foodInstance.food);
        }
    }

    public FoodInstance GetFoodInstance(string guid){
        if (!FoodInstances.TryGetValue(guid, out FoodInstance foodInstance)){
            Debug.LogError($"【FoodPile】获取食材实例失败: {guid} 不存在");
            return null;
        }
        return foodInstance;
    }
    public FoodInstance GetFoodInstance(Vector2Int position){
        FoodInstance foodInstance = FoodInstances.Values.FirstOrDefault(x => x.position == position);
        if (foodInstance == null){
            Debug.LogError($"【FoodPile】获取食材实例失败: {position} 不存在");
            return null;
        }
        return foodInstance;
    }


    // // 加载FoodPile数据
    // public FoodInstance LoadFoodInstance(FoodInstanceData foodInstanceData){
    //     return CreateFoodInstance(foodInstanceData.food, foodInstanceData.position);
    // }
}