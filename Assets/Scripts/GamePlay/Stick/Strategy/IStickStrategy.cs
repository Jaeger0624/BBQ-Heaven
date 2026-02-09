using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;


// 只负责提供逻辑，视图和交互由StickController负责
public interface IStickStrategy : ICanGetSystem{
    List<BoardCell> GetRange(Vector2Int cellPosition, Stick stick);
    List<FoodInstance> GetFood(Vector2Int hoveredCellPos, Stick stick);
    public static int directionValue{
        get{
            return direction % 4;
        }
        set{
            direction = value % 4;
        }
    }
    private static int direction = 0;
}

public class StickStrategy_默认 : IStickStrategy
{
    private Vector2Int startPos;
    private Vector2Int direction;

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public List<BoardCell> GetRange(Vector2Int cellPosition, Stick stick)
    {
        // 如果是horizontal，则返回所有同一排的cell
        if (IStickStrategy.directionValue == 1 || IStickStrategy.directionValue == 3)
        {
            return this.GetArchitecture().GetSystem<IBoardSystem>().GetGrid().GetAllCells().Where(x => x.position.y == cellPosition.y).ToList();
        }
        else
        {
            return this.GetArchitecture().GetSystem<IBoardSystem>().GetGrid().GetAllCells().Where(x => x.position.x == cellPosition.x).ToList();
        }
    }

    public List<FoodInstance> GetFood(Vector2Int hoveredCellPos, Stick stick)
    {
        List<FoodInstance> foodInstances = new List<FoodInstance>();
        // 方向：0向上，1向右，2向下，3向左
        // 横向
        if (IStickStrategy.directionValue == 1 || IStickStrategy.directionValue == 3)
        {
            int arg = IStickStrategy.directionValue == 1 ? 1 : -1;
            // 选取所有同y的食材，按x排序
            List<FoodInstance> temp = this.GetSystem<IFoodSystem>()
                .GetFoodInstances().Values
                .ToList()
                .Where(x => x.position.y == hoveredCellPos.y)
                .OrderBy(x => x.position.x * arg)
                .ToList();
            int sizeSum = 0;
            foreach (var foodInstance in temp){
                sizeSum += foodInstance.foodSize;
                if (sizeSum > stick.maxFoodCount){
                    sizeSum -= foodInstance.foodSize;
                    continue;
                }
                foodInstances.Add(foodInstance);
            }
        }
        else{
            int arg = IStickStrategy.directionValue == 0 ? 1 : -1;
            List<FoodInstance> temp = this.GetSystem<IFoodSystem>()
                .GetFoodInstances().Values
                .ToList()
                .Where(x => x.position.x == hoveredCellPos.x)
                .OrderBy(x => x.position.y * arg)
                .ToList();
            int sizeSum = 0;
            foreach (var foodInstance in temp){
                sizeSum += foodInstance.foodSize;
                if (sizeSum > stick.maxFoodCount){
                    sizeSum -= foodInstance.foodSize;
                    continue;
                }
                foodInstances.Add(foodInstance);
            }
        }
        return foodInstances;
    }
}
