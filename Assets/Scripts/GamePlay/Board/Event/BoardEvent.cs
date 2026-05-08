using QFramework;
using UnityEngine;

public class ResetBoardEvent : AbstractEvent{
    public int width;
    public int height;
    public ResetBoardEvent(int width, int height){
        this.width = width;
        this.height = height;
    }
}

public class MoveEntityEvent : AbstractEvent
{
    public BoardEntity entity;
    public Vector2Int newPos;
    public Vector2Int originPosition;
    public Vector2Int direction;
    public MoveEntityEvent(Vector2Int newPos, Vector2Int originPosition, BoardEntity entity, Vector2Int direction = default)
    {
        this.newPos = newPos;
        this.originPosition = originPosition;
        this.entity = entity;
        this.direction = direction;
    }
}

public class PlaceEntityEvent : AbstractEvent{
    public BoardEntity entity;
    public Vector2Int fromPos;
    public Vector2Int newPos;
    public PlaceEntityEvent(Vector2Int fromPos, Vector2Int newPos, BoardEntity entity){
        this.fromPos = fromPos;
        this.newPos = newPos;
        this.entity = entity;
    }
}

public class SwapEntityEvent : AbstractEvent{
    public BoardEntity entity1;
    public BoardEntity entity2;
    public SwapEntityEvent(BoardEntity entity1, BoardEntity entity2){
        this.entity1 = entity1;
        this.entity2 = entity2;
    }
}

public class CollisionEntityEvent : AbstractEvent{
    public BoardEntity initiator;
    public BoardEntity receiver;
    public Vector2Int direction;
    public CollisionEntityEvent(BoardEntity initiator, BoardEntity receiver, Vector2Int direction){
        this.initiator = initiator;
        this.receiver = receiver;
        this.direction = direction;
    }
}

public static class DirectionExtensions{
    public static bool IsStandardDirection(this Vector2Int direction){
        return direction == Vector2Int.up || direction == Vector2Int.down || direction == Vector2Int.left || direction == Vector2Int.right;
    }
}


public class UpdateCellTileEvent : AbstractEvent{
    public Vector2Int position;
    public string tileID;
    public UpdateCellTileEvent(Vector2Int position, string tileID){
        this.position = position;
        this.tileID = tileID;
    }
}


public class CreateEntityEvent : AbstractEvent{
    public BoardEntity entity;
    public CreateEntityEvent(BoardEntity entity){
        this.entity = entity;
    }
}