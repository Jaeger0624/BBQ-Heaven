using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;


// 只负责提供逻辑，视图和交互由StickController负责
public interface IStickStrategy : ICanGetSystem{
    List<BoardCell> GetRange(Vector2Int cellPosition);
    List<FoodInstance> GetFood(Vector2Int hoveredCellPos);

    static bool IsHorizontal { get; set; } = false;
}

public class StickStrategy_默认 : IStickStrategy
{
    private Vector2Int startPos;
    private Vector2Int direction;

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public List<BoardCell> GetRange(Vector2Int cellPosition)
    {
        // 如果是horizontal，则返回所有同一排的cell
        if (IStickStrategy.IsHorizontal)
        {
            return this.GetArchitecture().GetSystem<IBoardSystem>().GetGrid().GetAllCells().Where(x => x.position.y == cellPosition.y).ToList();
        }
        else
        {
            return this.GetArchitecture().GetSystem<IBoardSystem>().GetGrid().GetAllCells().Where(x => x.position.x == cellPosition.x).ToList();
        }
    }

    public List<FoodInstance> GetFood(Vector2Int hoveredCellPos)
    {
        List<FoodInstance> foodInstances = new List<FoodInstance>();
        if (IStickStrategy.IsHorizontal)
        {
            bool isLeft = hoveredCellPos.x <= this.GetSystem<IBoardSystem>().GetGrid().width/2;
            int arg = isLeft ? 1 : -1;
            foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList()
            .Where(x => x.position.y == hoveredCellPos.y)
            .OrderBy(x => x.position.x * arg).ToList();
        }
        else{
            bool isBottom = hoveredCellPos.y <= this.GetSystem<IBoardSystem>().GetGrid().height/2;
            int arg = isBottom ? 1 : -1;
            foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList()
            .Where(x => x.position.x == hoveredCellPos.x)
            .OrderBy(x => x.position.y * arg).ToList();
        }
        return foodInstances;
    }
}
