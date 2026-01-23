using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

// 目标选择器，用于选择目标
public class TargetSelector{
    // 高亮目标
    public static void Highlight(CardTargetType targetType){
        if (targetType == CardTargetType.无) return;
        else if (targetType == CardTargetType.食材){
            List<FoodInstance> foodInstances = GameArchitecture.Interface.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList();
            Debug.Log($"【TargetSelector】高亮食材: {foodInstances.Count}");
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
        else if (targetType == CardTargetType.任意格子){
            List<BoardCell> cells = GameArchitecture.Interface.GetSystem<IBoardSystem>().GetGrid().GetAllCells().ToList();
            List<Vector2Int> positions = cells.Select(x => x.position).ToList();
            GameArchitecture.Interface.GetSystem<IBoardSystem>().HighlightCells(positions);
        }
        else if (targetType == CardTargetType.空格子){
            List<BoardCell> cells = GameArchitecture.Interface.GetSystem<IBoardSystem>().GetEmptyCells();
            List<Vector2Int> positions = cells.Select(x => x.position).ToList();
            GameArchitecture.Interface.GetSystem<IBoardSystem>().HighlightCells(positions);
        }
        else if (targetType == CardTargetType.任意顾客){
            GameArchitecture.Interface.SendEvent(new HighlightCustomersEvent(GameArchitecture.Interface.GetSystem<ICustomerSystem>().OrderingCustomers.ToList()));
        }
        else{
            Debug.LogError("【Highlighter】不支持的目标类型: {targetType}");
        }
    }

    // 获取目标（UI -> 逻辑对象）
    public static List<object> GetParam(CardTargetType targetType, GameObject targetObject){
        if (targetType == CardTargetType.无) return new List<object>();
        else if (targetType == CardTargetType.食材){
            IEntityView targetView = targetObject.GetComponent<IEntityView>();
            if (targetView == null){
                return null;
            }
            if (targetView.Entity is not FoodInstance foodInstance){
                Debug.LogError($"【TargetSelector】目标不是食材: {targetView.Entity.name}");
                return null;
            }
            return new List<object>{foodInstance};
        }
        else if (targetType == CardTargetType.周围有空位的食材){
            IEntityView targetView = targetObject.GetComponent<IEntityView>();
            if (targetView == null){
                return null;
            }
            if (targetView.Entity is not FoodInstance foodInstance){
                Debug.LogError($"【TargetSelector】目标不是食材: {targetView.Entity.name}");
                return null;
            }
            if (foodInstance.GetSurroundingEmptyCellsCount() == 0){
                return null;
            }
            return new List<object>{foodInstance};
        }
        else if (targetType == CardTargetType.周围有食材的食材){
            IEntityView targetView = targetObject.GetComponent<IEntityView>();
            if (targetView == null) return null;
            
            if (targetView.Entity is not FoodInstance foodInstance){
                Debug.LogError($"【TargetSelector】目标不是食材: {targetView.Entity.name}");
                return null;
            }
            if (foodInstance.GetSurroundingFoodInstances().Count == 0) return null;
            
            return new List<object>{foodInstance};
        }
        else if (targetType == CardTargetType.任意格子){
            CellViewUI targetCellView = targetObject.GetComponentInParent<CellViewUI>();
            if (targetCellView == null){
                return null;
            }
            BoardCell targetCell = targetCellView.cell;
            if (targetCell == null){
                return null;
            }
            return new List<object>{targetCell};
        }
        else if (targetType == CardTargetType.空格子){
            CellViewUI targetCellView = targetObject.GetComponentInParent<CellViewUI>();
            if (targetCellView == null){
                return null;
            }
            BoardCell targetCell = targetCellView.cell;
            if (targetCell == null){
                return null;
            }
            if (!targetCell.IsEmpty()){
                return null;
            }
            return new List<object>{targetCell};
        }
        else if (targetType == CardTargetType.任意顾客){
            OrderView targetOrderView = targetObject.GetComponentInParent<OrderView>();
            if (targetOrderView == null){
                return null;
            }
            Debug.Log($"GetParam: 任意顾客");
            return new List<object>{targetOrderView.customer};
        }
        else{
            Debug.LogError("【TargetSelector】不支持的目标类型: {targetType}");
            return null;
        }
    }
}