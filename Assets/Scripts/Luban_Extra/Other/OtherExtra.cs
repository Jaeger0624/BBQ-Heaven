using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

public class Highlighter{
    public static void Highlight(CardTargetType targetType){
        if (targetType == CardTargetType.无) return;
        else if (targetType == CardTargetType.食材){
            List<FoodInstance> foodInstances = GameArchitecture.Interface.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList();
            List<Vector2Int> positions = foodInstances.Select(x => x.position).ToList();
            GameArchitecture.Interface.GetSystem<IBoardSystem>().HighlightCells(positions);
        }
        else if (targetType == CardTargetType.周围有空位的食材){
            // 获取所有食材
            List<FoodInstance> foodInstances = GameArchitecture.Interface.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList().Where(x => x.GetSurroundingEmptyCellsCount() > 0).ToList();
            List<Vector2Int> positions = foodInstances.Select(x => x.position).ToList();
            GameArchitecture.Interface.GetSystem<IBoardSystem>().HighlightCells(positions);
        }
        else if (targetType == CardTargetType.周围有食材的食材){
            List<FoodInstance> foodInstances = GameArchitecture.Interface.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList().Where(x => x.GetSurroundingFoodInstances().Count > 0).ToList();
            List<Vector2Int> positions = foodInstances.Select(x => x.position).ToList();
            GameArchitecture.Interface.GetSystem<IBoardSystem>().HighlightCells(positions);
        }
        else{
            Debug.LogError("【Highlighter】不支持的目标类型: {targetType}");
        }
    }



}
