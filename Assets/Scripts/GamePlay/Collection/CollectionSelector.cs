using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public class CollectionSelector : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    /// <summary>
    /// 获取食材UI上下文
    /// </summary>
    /// <param name="param">参数：0-所有食材，1-零食，2-水果，3-蔬菜，4-海鲜</param>
    public List<FoodUIContext> GetFoodUIContexts(int param){
        List<FoodUIContext> foodUIContexts = new List<FoodUIContext>();
        switch (param){
            case 0:
                foreach (var foodData in this.GetSystem<IDataSystem>().GetAllFoodData()){
                    foodUIContexts.Add(new FoodUIContext(foodData, CollectionState.Locked));
                }
                break;
            case 1:
                foreach (var foodData in this.GetSystem<IDataSystem>().GetAllFoodData().Where(x => x.Type == FoodType.零食)){
                    foodUIContexts.Add(new FoodUIContext(foodData, CollectionState.Locked));
                }
                break;
            case 2:
                foreach (var foodData in this.GetSystem<IDataSystem>().GetAllFoodData().Where(x => x.Type == FoodType.水果)){
                    foodUIContexts.Add(new FoodUIContext(foodData, CollectionState.Locked));
                }
                break;
            case 3:
                foreach (var foodData in this.GetSystem<IDataSystem>().GetAllFoodData().Where(x => x.Type == FoodType.蔬菜)){
                    foodUIContexts.Add(new FoodUIContext(foodData, CollectionState.Locked));
                }
                break;
            case 4:
                foreach (var foodData in this.GetSystem<IDataSystem>().GetAllFoodData().Where(x => x.Type == FoodType.海鲜)){
                    foodUIContexts.Add(new FoodUIContext(foodData, CollectionState.Locked));
                }
                break;
            default:
                Debug.LogError("Invalid param: " + param);
                break;
        }
        return foodUIContexts;
    }
}