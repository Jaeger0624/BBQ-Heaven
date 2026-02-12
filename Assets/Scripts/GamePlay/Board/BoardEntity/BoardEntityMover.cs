using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class BoardEntityMover : ICanGetSystem, ICanSendEvent
{
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>();
    public IArchitecture GetArchitecture() =>
        GameArchitecture.Interface;

    # region API Functions
    public void DirectionalMove(BoardEntity entity, Vector2Int direction, int distance){
        if (distance <= 0) return;
        Direction directionEnum = direction.ToDirection();
        // Debug.Log($"【BoardEntityMover】方向移动实体: {entity.name} 向 {directionEnum} 移动 {distance} 步");
        BoardCell endCell = this.GetSystem<IBoardSystem>().GetStopPosition(entity.position, direction, distance, out BoardEntity target);
        if (target != null){
            Debug.Log($"<color=orange>触发碰撞: {entity.name} 和 {target.name}</color>");
            TriggerCollision(entity, target);
            this.SendEvent(new CollisionEntityEvent(entity, target, direction));
        }

        if (endCell.position != entity.position){
            PerformMove(entity, endCell, direction);
        }
    }

    public void PlaceEntity(BoardEntity entity, BoardCell targetCell){
        BoardCell cell = this.GetSystem<IBoardSystem>().GetCell(targetCell.position);
        PerformPlace(entity, cell);
    }
    public void RandomPlaceEntities(List<BoardEntity> entities){
        // 1. 先移除
        entities.ForEach(entity => {
            this.GetSystem<IBoardSystem>().SetCellInstance(entity.position, null);
        });
        // 2. 随机移动到棋盘上的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        foreach (var entity in entities){
            BoardCell targetCell = rng.PickOne(emptyCells);
            emptyCells.Remove(targetCell);

            UpdateEntityData(entity, targetCell);
            this.SendEvent(new PlaceEntityEvent(entity.position, targetCell.position, entity));
        }
    }
    public void SwapEntities(BoardEntity first, BoardEntity second){
        Vector2Int posA = first.position;
        Vector2Int posB = second.position;

        this.GetSystem<IBoardSystem>().SetCellInstance(posA, second.guid);
        this.GetSystem<IBoardSystem>().SetCellInstance(posB, first.guid);

        first.position = posB;
        second.position = posA;

        this.SendEvent(new SwapEntityEvent(first, second));
    }
    #endregion
    # region Internal Functions
    private void PerformMove(BoardEntity entity, BoardCell endCell, Vector2Int direction){
        Vector2Int originPosition = entity.position;

        UpdateEntityData(entity, endCell);
        entity.UpdateLastMoveDirection(direction);

        this.SendEvent(new MoveEntityEvent(endCell.position, originPosition, entity, direction));
    }
    private void PerformPlace(BoardEntity entity, BoardCell cell){
        Vector2Int oldPos = entity.position;

        if (oldPos.x != -1){
            this.GetSystem<IBoardSystem>().SetCellInstance(oldPos, null);
        }
        UpdateEntityData(entity, cell);
        this.SendEvent(new PlaceEntityEvent(oldPos, cell.position, entity));
    }

    private void UpdateEntityData(BoardEntity entity, BoardCell cell){
        if (entity.position.x != -1){
            BoardCell originCell = this.GetSystem<IBoardSystem>().GetCell(entity.position);
            if (originCell != null && originCell.instanceGuid == entity.guid){
                this.GetSystem<IBoardSystem>().SetCellInstance(entity.position, null);
            }
        }
        entity.position = cell.position;
        this.GetSystem<IBoardSystem>().SetCellInstance(cell.position, entity.guid);
    }


    private void TriggerCollision(BoardEntity initiator, BoardEntity receiver){
        Vector2Int directionVector = receiver.position - initiator.position;
        Direction direction = directionVector.ToDirection();
        // 1. 构建上下文
        CollisionContext context = new CollisionContext(initiator, receiver);
        DirectionContext directionContext = new DirectionContext(direction);

        List<object> param = new List<object>{context, directionContext};

        FoodInstance initiatorFoodInstance = (FoodInstance)context.initiator;
        
        if (initiatorFoodInstance != null){
            Debug.Log($"触发主动方的碰撞效果: {initiatorFoodInstance.name}");
            if (initiatorFoodInstance.food.foodGAs.TryGetValue(FoodGAType.碰撞时, out List<CGA> cgas)){
                foreach (var cga in cgas){
                    this.GetSystem<IGASystem>().TriggerReaction(cga, initiatorFoodInstance, param);
                }
            }
        }

        FoodInstance receiverFoodInstance = (FoodInstance)context.receiver;
        if (receiverFoodInstance != null){
            Debug.Log($"触发被动方的被撞效果: {receiverFoodInstance.name}");
            if (receiverFoodInstance.food.foodGAs.TryGetValue(FoodGAType.被碰撞时, out List<CGA> cgas)){
                Debug.Log($"触发被动方的被撞效果: {receiverFoodInstance.name} 的碰撞效果数量: {cgas.Count}");
                foreach (var cga in cgas){
                    this.GetSystem<IGASystem>().TriggerReaction(cga, receiverFoodInstance, param);
                }
            }
        }

    }
    
    # endregion


}
