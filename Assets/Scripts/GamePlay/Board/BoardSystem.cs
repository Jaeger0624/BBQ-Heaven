using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using PrimeTweenDemo;
using QFramework;
using UniRx;
using UnityEngine;

public interface IBoardSystem : ISystem{
    IObservable<Unit> OnBoardStateChanged { get; }
    IObservable<BoardCell> OnCellClicked { get; }
    Grid<BoardCell> GetGrid();
    List<BoardCell> GetEmptyCells();
    void ClickCell(Vector2Int position);
    BoardCell GetCell(Vector2Int position);
    List<BoardCell> GetAdjacentCells(Vector2Int position);
    BoardCell GetRandomEmptyCell();
    BoardCell GetStopPosition(Vector2Int origin, Vector2Int direction, int distance, out BoardEntity entity);

    // SelectorSystem 需要用到的方法 
    void HighlightCells(List<Vector2Int> positions);
    void ClearHighlight();
    // 和棋盘实例相关的操作
    void SetCellInstance(Vector2Int position, string instanceGuid);
    // 和地块相关的操作
    void SetCellTile(Vector2Int position, string tileID);
}
/// <summary>
/// 棋盘系统，提供与棋盘有关的信息与操作
/// </summary>
public class BoardSystem : AbstractSystem, IBoardSystem
{
    private Grid<BoardCell> grid;
    // 1. 定义一个 Subject (它是幕后的“广播站”)
    private Subject<BoardCell> _cellClickedSubject = new Subject<BoardCell>();
    private Subject<Unit> _boardStateChangedSubject = new Subject<Unit>();
    private TileHandler tileHandler = new TileHandler();

    // 2. 实现接口：将 Subject 也就是这个管道暴露出去供人订阅
    public IObservable<BoardCell> OnCellClicked => _cellClickedSubject;
    public IObservable<Unit> OnBoardStateChanged => _boardStateChangedSubject;
    public List<BoardCell> GetEmptyCells() => grid.GetAllCells().Where(x => x.IsEmpty()).ToList();
    public BoardCell GetCell(Vector2Int position) => grid.GetCell(position.x, position.y);
    public Grid<BoardCell> GetGrid() => grid;
    public List<BoardCell> GetAdjacentCells(Vector2Int position){

        return new List<BoardCell>{
            grid.GetCell(position.x + 1, position.y),
            grid.GetCell(position.x - 1, position.y),
            grid.GetCell(position.x, position.y + 1),
            grid.GetCell(position.x, position.y - 1)
        }.Where(x => x != null).ToList();
    }
    public BoardCell GetRandomEmptyCell(){
        return this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>().PickOne(GetEmptyCells());
    }
    protected override void OnInit()
    {
        grid = new Grid<BoardCell>(8, 8, () => new BoardCell(null));

        _cellClickedSubject = new Subject<BoardCell>();
        _boardStateChangedSubject = new Subject<Unit>();


        this.RegisterEvent<MoveEntityEvent>(tileHandler.EntityMove);
        this.RegisterEvent<PlaceEntityEvent>(tileHandler.EntityPlaced);   
    }
    protected override void OnDeinit()
    {
        // 3. 记得销毁，防止内存泄漏
        _cellClickedSubject?.Dispose();
        _cellClickedSubject = null;
        _boardStateChangedSubject?.Dispose();
        _boardStateChangedSubject = null;

        this.UnRegisterEvent<MoveEntityEvent>(tileHandler.EntityMove);
        this.UnRegisterEvent<PlaceEntityEvent>(tileHandler.EntityPlaced);
    }
    public void ClickCell(Vector2Int position){
        // 获取逻辑层的数据
        BoardCell cell = grid.GetCell(position.x, position.y);
        
        if (cell != null)
        {
            // 4. 核心逻辑：向管道里发射数据！
            // 所有订阅了 OnCellClicked 的地方都会立刻收到这个消息
            _cellClickedSubject.OnNext(cell);
        
        }
    }
        
    public void SetCellInstance(Vector2Int position, string instanceGuid)
    {
        BoardCell cell = grid.GetCell(position.x, position.y);

        if (instanceGuid == null){
            cell.SetInstance(null);
        }
        else{
            cell.SetInstance(instanceGuid);
        }
        _boardStateChangedSubject.OnNext(Unit.Default);
    }

    public BoardCell GetStopPosition(Vector2Int origin, Vector2Int direction, int distance, out BoardEntity entity){
        Vector2Int res = origin;
        entity = null;
        for (int i = 0; i < distance; i++){
            res += direction;

            // 到墙边要停止
            if (res.x < 0 || res.x >= grid.width || res.y < 0 || res.y >= grid.height){
                res -= direction;
                break;
            }

            // 碰到食材要停止（但可以到食材上）
            if (!grid.GetCell(res.x, res.y).IsEmpty()){
                entity = this.GetSystem<IBoardEntitySystem>().GetEntity(grid.GetCell(res.x, res.y).instanceGuid);
                res -= direction;
                break;
            }
        }
        return grid.GetCell(res.x, res.y);
    }

    public void HighlightCells(List<Vector2Int> positions){
        // Debug.Log($"HighlightCells: {positions.Count}");
        this.SendEvent(new HighlightCellsEvent(positions));
    }
    public void ClearHighlight(){
        this.SendEvent(new ClearAllBoardsHighlight());
    }

    public void SetCellTile(Vector2Int position, string tileID)
    {
        BoardCell cell = grid.GetCell(position.x, position.y);
        cell.SetTile(tileID);
        _boardStateChangedSubject.OnNext(Unit.Default);
        this.SendEvent(new UpdateCellTileEvent(position, tileID));
    }
}

