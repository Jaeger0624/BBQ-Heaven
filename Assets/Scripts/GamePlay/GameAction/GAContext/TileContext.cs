using cfg;

public class TileContext{
    public readonly BoardEntity boardEntity;
    public readonly BoardCell boardCell;
    /// <summary>
    /// 方向，用来判断事件的触发方向，如果为无，则说明不是移动性事件
    /// </summary>
    public readonly Direction direction;
    public TileContext(BoardEntity boardEntity, BoardCell boardCell, Direction direction){
        this.boardEntity = boardEntity;
        this.boardCell = boardCell;
        this.direction = direction;
    }
}