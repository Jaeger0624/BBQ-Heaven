using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class BoardEntityMover : ICanGetSystem, ICanSendEvent
{
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>();
    public IArchitecture GetArchitecture() =>
        GameArchitecture.Interface;
    public MoveEntityEvent MoveEntityTo(Vector2Int newPos, BoardEntity entity){
        Debug.Log($"【BoardEntityMover】移动实体: {entity.name} 到 {newPos}");
        Vector2Int originPosition = entity.position;
        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(newPos);

        if (cell == null){ return null; }

        if (!cell.IsEmpty()){
            string targetGuid = cell.instanceGuid;

            // 从棋盘实体系统中获取目标实例
            BoardEntity targetEntity = this.GetSystem<IBoardEntitySystem>().GetEntity(targetGuid);
            
            if (targetEntity != null){
                Debug.Log($"触发碰撞: {entity.name} 和 {targetEntity.name}");
                TriggerCollision(entity, targetEntity);
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
        return this.MoveTo(entity, cell);
    }
    

    // 先将食材实例从棋盘上移除，然后随机移动到棋盘上的空位
    public List<MoveEntityEvent> RandomMoveEntities(List<BoardEntity> entities){
        Debug.Log($"【BoardEntityMover】随机移动实体: {entities.Count}");
        // 1. 先将选中食材实例从棋盘上移除
        entities.ForEach(entity => {
            Vector2Int originPosition = entity.position;
            this.GetSystem<IBoardSystem>().SetCellInstance(originPosition, null);
        });

        // 2. 获取棋盘上的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        List<MoveEntityEvent> moveEvents = new List<MoveEntityEvent>();
        // 3. 随机移动到棋盘上的空位
        foreach (var entity in entities){
            BoardCell emptyCell = rng.PickOne(emptyCells);
            moveEvents.Add(this.PlaceTo(entity, emptyCell));
            emptyCells.Remove(emptyCell);
        }
        return moveEvents;
    }
    private MoveEntityEvent MoveTo(BoardEntity entity, BoardCell cell, bool showAnimDirect = false){
        entity.position = cell.position;
        cell.SetInstance(entity.guid);
        return new MoveEntityEvent(cell.position, entity.position, entity);
    }
    private MoveEntityEvent PlaceTo(BoardEntity entity, BoardCell cell){
        entity.position = cell.position;
        cell.SetInstance(entity.guid);
        return new MoveEntityEvent(cell.position, entity.position, entity);
    }

    public MoveEntityEvent DirectionalMove(BoardEntity entity, Vector2Int direction, int distance){
        Direction directionEnum = direction.ToDirection();
        Debug.Log($"【BoardEntityMover】方向移动实体: {entity.name} 向 {directionEnum} 移动 {distance} 步");
        BoardCell endCell = this.GetSystem<IBoardSystem>().GetStopPosition(entity.position, direction, distance, out BoardEntity target);
        if (target != null){
            Debug.Log($"触发碰撞: {entity.name} 和 {target.name}");
            TriggerCollision(entity, target);
        }
        return MoveEntityTo(endCell.position, entity);
    }

    public List<MoveEntityEvent> ExchangeEntities(BoardEntity first, BoardEntity second){
        List<MoveEntityEvent> moveEvents = new List<MoveEntityEvent>();
        
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
        moveEvents.Add(MoveTo(first, secondCell));
        moveEvents.Add(MoveTo(second, firstCell));
        return moveEvents;
    }

    private void TriggerCollision(BoardEntity initiator, BoardEntity receiver){
        Vector2Int directionVector = receiver.position - initiator.position;
        Direction direction = directionVector.ToDirection();
        // 1. 构建上下文
        CollisionContext context = new CollisionContext(initiator, receiver);
        DirectionContext directionContext = new DirectionContext(direction);

        // 2. 触发碰撞
        HandleCollision(context, directionContext);
    }


    private void HandleCollision(CollisionContext context, DirectionContext directionContext){
        List<object> param = new List<object>{context, directionContext};

        FoodInstance initiatorFoodInstance = (FoodInstance)context.initiator;
        
        if (initiatorFoodInstance != null){
            Debug.Log($"触发主动方的碰撞效果: {initiatorFoodInstance.name}");
            if (initiatorFoodInstance.food.foodGAs.TryGetValue(FoodGAType.碰撞时, out List<CGA> cgas)){
                foreach (var cga in cgas){
                    if (cga.Conditions.Count == 0 || cga.Conditions == null) continue;
                    this.GetSystem<IGASystem>().ApplyCGA(initiatorFoodInstance, cga, param);
                }
            }
        }

        FoodInstance receiverFoodInstance = (FoodInstance)context.receiver;
        if (receiverFoodInstance != null){
            Debug.Log($"触发被动方的被撞效果: {receiverFoodInstance.name}");
            if (receiverFoodInstance.food.foodGAs.TryGetValue(FoodGAType.被碰撞时, out List<CGA> cgas)){
                foreach (var cga in cgas){
                    if (cga.Conditions.Count == 0 || cga.Conditions == null) continue;
                    this.GetSystem<IGASystem>().ApplyCGA(receiverFoodInstance, cga, param);
                }
            }
        }

    }
}

public class MoveEntityEvent : AbstractEvent{
    public Vector2Int newPos;
    public Vector2Int originPosition;
    public string entityGuid;
    public MoveEntityEvent(Vector2Int newPos, Vector2Int originPosition, BoardEntity entity){
        this.newPos = newPos;
        this.originPosition = originPosition;
        this.entityGuid = entity.guid;
    }
}