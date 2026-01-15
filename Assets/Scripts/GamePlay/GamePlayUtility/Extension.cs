using System.Collections.Generic;
using QFramework;
using UnityEngine;

public static class Extension{

    #region FoodSystem
    /// <summary>
    /// 扩展方法：获取食材周围食材实例
    /// </summary>
    /// <param name="foodInstance">食材实例</param>
    /// <returns>周围食材实例列表</returns>
    public static List<FoodInstance> GetSurroundingFoodInstances(this FoodInstance foodInstance){
        IFoodSystem foodSystem = GameArchitecture.Interface.GetSystem<IFoodSystem>();
        IBoardSystem boardSystem = GameArchitecture.Interface.GetSystem<IBoardSystem>();
        List<FoodInstance> surroundingFoodInstances = new List<FoodInstance>();
        foreach (BoardCell surroundingCell in boardSystem.GetAdjacentCells(foodInstance.position)){
            if (surroundingCell.instanceGuid == null) continue;
            BoardEntity surroundingEntity = GameArchitecture.Interface.GetSystem<IBoardEntitySystem>().GetEntity(surroundingCell.instanceGuid);
            if (surroundingEntity.type == BoardEntityType.食材){
                surroundingFoodInstances.Add(surroundingEntity as FoodInstance);
            }
        }
        return surroundingFoodInstances;
    }

    public static int GetSurroundingEmptyCellsCount(this FoodInstance foodInstance){
        IBoardSystem boardSystem = GameArchitecture.Interface.GetSystem<IBoardSystem>();
        List<BoardCell> surroundingEmptyCells = new List<BoardCell>();
        foreach (BoardCell surroundingCell in boardSystem.GetAdjacentCells(foodInstance.position)){
            if (surroundingCell.instanceGuid != null) continue;
            surroundingEmptyCells.Add(surroundingCell);
        }
        return surroundingEmptyCells.Count;
    }

    #endregion
}