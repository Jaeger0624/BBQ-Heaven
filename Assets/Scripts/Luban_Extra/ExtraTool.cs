using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;

public class ExtraTool : IController{
    public static bool BoolValue(BoolSign sign, int value1, int value2){
        switch (sign){
            case BoolSign.等于:
                return value1 == value2;
            case BoolSign.不等于:
                return value1 != value2;
            case BoolSign.大于:
                return value1 > value2;
            case BoolSign.小于:
                return value1 < value2;
            case BoolSign.大于等于:
                return value1 >= value2;
            case BoolSign.小于等于:
                return value1 <= value2;
        }
        return false;
    }

    /// <summary>
    /// 根据策略获取食材实例列表
    /// </summary>
    /// <param name="types">食材状态类型列表（当strategy为串上某类型食材时，用作FoodType列表）</param>
    /// <param name="strategy">选择策略</param>
    /// <param name="amount">数量（-1表示全部）</param>
    /// <param name="sender">发起者</param>
    /// <param name="param">参数列表</param>
    /// <returns>食材实例列表</returns>
    public static List<FoodInstance> GetFoodInstances(List<FoodInstanceState> types, GetFoodInstanceStrategy strategy, int amount, object sender, List<object> param){
        IFoodSystem foodSystem = GameArchitecture.Interface.GetSystem<IFoodSystem>();
        IBoardSystem boardSystem = GameArchitecture.Interface.GetSystem<IBoardSystem>();
        Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        List<FoodInstance> foods = new List<FoodInstance>();
        FoodInstance origin = sender as FoodInstance;
        switch (strategy){
            case GetFoodInstanceStrategy.随机:
                // 已经排除了串上的食材实例
                foods = foodSystem.GetFoodInstances().Values.ToList();
                break;

            case GetFoodInstanceStrategy.周围随机:
                if (origin == null){
                    Debug.LogError("origin is null");
                    return new List<FoodInstance>();
                }
                Vector2Int originPos = origin.position;
                List<BoardCell> adjacentCells = boardSystem.GetAdjacentCells(originPos);
                foreach (BoardCell adjacentCell in adjacentCells){
                    string instanceGuid = adjacentCell.instanceGuid;
                    if (instanceGuid == null) continue;
                    FoodInstance foodInstance = foodSystem.GetFoodInstances().Values.FirstOrDefault(x => x.guid == instanceGuid);
                    if (foodInstance == null) continue;
                    foods.Add(foodInstance);
                }
                break;

            case GetFoodInstanceStrategy.九宫格随机:
                if (origin == null){
                    Debug.LogError("origin is null");
                    return new List<FoodInstance>();
                }
                foods = foodSystem.GetFoodInstances().Values.ToList();
                break;


            case GetFoodInstanceStrategy.自己:
                if (origin == null){
                    Debug.LogError("origin is null");
                    return new List<FoodInstance>();
                }
                foods = new List<FoodInstance>{origin}; // 把自己放进去
                break;

                
            case GetFoodInstanceStrategy.选取:
                if (param == null){
                    Debug.LogError("param is null");
                    return new List<FoodInstance>();
                }
                FoodInstance food = param.FirstOrDefault(x => x is FoodInstance) as FoodInstance;
                if (food == null){
                    Debug.LogError("foodInstance is null");
                    return new List<FoodInstance>();
                }
                foods = new List<FoodInstance>{food};
                break;
        }

        // 过滤掉不在类型列表中的食材实例（仅对非类型筛选策略生效）
        if (!types.Contains(FoodInstanceState.棋盘上)) {
            foods.RemoveAll(x => x.state == FoodInstanceState.棋盘上);
        }
        if (!types.Contains(FoodInstanceState.烤串上)) {
            foods.RemoveAll(x => x.state == FoodInstanceState.烤串上);
        }
        if (!types.Contains(FoodInstanceState.被选中)) {
            foods.RemoveAll(x => x.state == FoodInstanceState.被选中);
        }

        // 筛选数量
        if (foods.Count == 0) return new List<FoodInstance>();
        // 如果数量为-1，则返回所有食材实例
        if (amount == -1) return foods;
        if (foods.Count < amount) return foods;
        return rng.PickMany<FoodInstance>(foods, amount);
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}

