using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class FoodInstanceMover : ICanGetSystem, ICanSendEvent
{
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
    public IArchitecture GetArchitecture() =>
        GameArchitecture.Interface;

    public MoveFoodInstanceEvent DirectionalMove(FoodInstance foodInstance, Direction direction, bool showAnimDirect = false){
        // 获取目标位置
        Vector2Int newPos = foodInstance.position + direction.ToVector2Int();
        return MoveFoodInstanceTo(newPos, foodInstance, showAnimDirect);
    }
    public MoveFoodInstanceEvent MoveFoodInstanceTo(Vector2Int newPos, FoodInstance foodInstance, bool showAnimDirect = false){
        Vector2Int originPosition = foodInstance.position;
        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(newPos);

        if (cell == null){ return null; }

        if (!cell.IsEmpty()){
            string targetGuid = cell.instanceGuid;

            // 从棋盘实体系统中获取目标实例
            BoardEntity targetEntity = this.GetSystem<IBoardEntitySystem>().GetEntity(targetGuid);
            
            if (targetEntity != null){
                TriggerCollision(foodInstance, targetEntity);
                return null;
            }
            else{
                // 目标实例不存在，直接移到指定位置
                Debug.LogError($"格子{newPos}不为空，但目标实例不存在");
                return null;
            }
        }
        // 2. 从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(originPosition, null);

        // 3. 移到指定位置
        return this.MoveTo(foodInstance, cell, showAnimDirect);
    }

    // 先将食材实例从棋盘上移除，然后随机移动到棋盘上的空位
    public List<MoveFoodInstanceEvent> RandomMoveFoodInstances(List<FoodInstance> foodInstances, bool showAnimDirect = false){
        Debug.Log($"【FoodInstanceMover】随机移动食材实例: {foodInstances.Count}");
        // 1. 先将选中食材实例从棋盘上移除
        foodInstances.ForEach(foodInstance => {
            Vector2Int originPosition = foodInstance.position;
            this.GetSystem<IBoardSystem>().SetCellInstance(originPosition, null);
        });

        // 2. 获取棋盘上的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        List<MoveFoodInstanceEvent> moveEvents = new List<MoveFoodInstanceEvent>();
        // 3. 随机移动到棋盘上的空位
        foreach (var foodInstance in foodInstances){
            BoardCell emptyCell = rng.PickOne(emptyCells);
            moveEvents.Add(this.PlaceTo(foodInstance, emptyCell, showAnimDirect));
            emptyCells.Remove(emptyCell);
        }
        return moveEvents;
    }
    private MoveFoodInstanceEvent MoveTo(FoodInstance foodInstance, BoardCell cell, bool showAnimDirect = false){
        foodInstance.position = cell.position;
        cell.SetInstance(foodInstance.guid);
        return new MoveFoodInstanceEvent(cell.position, foodInstance.position, foodInstance.guid);
    }
    private MoveFoodInstanceEvent PlaceTo(FoodInstance foodInstance, BoardCell cell, bool showAnimDirect = false){
        foodInstance.position = cell.position;
        cell.SetInstance(foodInstance.guid);
        return new MoveFoodInstanceEvent(cell.position, foodInstance.position, foodInstance.guid);
    }

    public MoveFoodInstanceEvent FoodInstanceLineMove(FoodInstance foodInstance, Vector2Int direction, bool showAnimDirect = false){
        BoardCell stopPosition = this.GetSystem<IBoardSystem>().GetStopPosition(foodInstance.position, direction);
        if (stopPosition.position == foodInstance.position){
            return null;
        }
        int distance = (int)Vector2Int.Distance(foodInstance.position, stopPosition.position);
        return MoveTo(foodInstance, stopPosition, showAnimDirect);
    }

    public List<MoveFoodInstanceEvent> ExchangeFoodInstances(FoodInstance first, FoodInstance second, bool showAnimDirect = false){
        List<MoveFoodInstanceEvent> moveEvents = new List<MoveFoodInstanceEvent>();
        
        // 1. 分别从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(first.position, null);
        this.GetSystem<IBoardSystem>().SetCellInstance(second.position, null);
        // 2. 交换位置
        Vector2Int firstPosition = first.position;
        Vector2Int secondPosition = second.position;
        first.position = secondPosition;
        second.position = firstPosition;
        BoardCell firstCell = this.GetSystem<IBoardSystem>().GetCell(firstPosition);
        BoardCell secondCell = this.GetSystem<IBoardSystem>().GetCell(secondPosition);
        // 3. 分别移到新的位置
        moveEvents.Add(MoveTo(first, secondCell, showAnimDirect));
        moveEvents.Add(MoveTo(second, firstCell, showAnimDirect));
        return moveEvents;
    }

    private void TriggerCollision(BoardEntity initiator, BoardEntity receiver){
        Vector2Int directionVector = receiver.position - initiator.position;
        Direction direction = directionVector.ToDirection();
        // 1. 构建上下文
        CollisionContext context = new CollisionContext(initiator, receiver);
        DirectionContext directionContext = new DirectionContext(direction);


        List<object> param = new List<object>{context, directionContext};
        // 2. 触发碰撞
        IGASystem gaSystem = this.GetSystem<IGASystem>();

        // 3. 触发主动方的碰撞效果
        
        // 4. 触发被动方的碰撞效果

    }
}