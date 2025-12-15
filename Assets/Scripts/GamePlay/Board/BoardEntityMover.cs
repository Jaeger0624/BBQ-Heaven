using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class BoardEntityMover : ICanGetSystem, ICanSendEvent
{
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>();
    public IArchitecture GetArchitecture() =>
        GameArchitecture.Interface;
    public MoveEntityEvent MoveEntityTo(Vector2Int newPos, BoardEntity entity, bool showAnimDirect = false){
        Vector2Int originPosition = entity.position;
        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(newPos);

        if (cell == null){ return null; }

        if (!cell.IsEmpty()){
            string targetGuid = cell.instanceGuid;

            // 从棋盘实体系统中获取目标实例
            BoardEntity targetEntity = this.GetSystem<IBoardEntitySystem>().GetEntity(targetGuid);
            
            if (targetEntity != null){
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
        return this.MoveTo(entity, cell, showAnimDirect);
    }

    // 先将食材实例从棋盘上移除，然后随机移动到棋盘上的空位
    public List<MoveEntityEvent> RandomMoveEntities(List<BoardEntity> entities, bool showAnimDirect = false){
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
            moveEvents.Add(this.PlaceTo(entity, emptyCell, showAnimDirect));
            emptyCells.Remove(emptyCell);
        }
        return moveEvents;
    }
    private MoveEntityEvent MoveTo(BoardEntity entity, BoardCell cell, bool showAnimDirect = false){
        entity.position = cell.position;
        cell.SetInstance(entity.guid);
        return new MoveEntityEvent(cell.position, entity.position, entity);
    }
    private MoveEntityEvent PlaceTo(BoardEntity entity, BoardCell cell, bool showAnimDirect = false){
        entity.position = cell.position;
        cell.SetInstance(entity.guid);
        return new MoveEntityEvent(cell.position, entity.position, entity);
    }

    public MoveEntityEvent DirectionalMove(BoardEntity entity, Vector2Int direction, bool showAnimDirect = false){
        BoardCell stopPosition = this.GetSystem<IBoardSystem>().GetStopPosition(entity.position, direction);
        if (stopPosition.position == entity.position){
            return null;
        }
        int distance = (int)Vector2Int.Distance(entity.position, stopPosition.position);
        return MoveTo(entity, stopPosition, showAnimDirect);
    }

    public List<MoveEntityEvent> ExchangeEntities(BoardEntity first, BoardEntity second, bool showAnimDirect = false){
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