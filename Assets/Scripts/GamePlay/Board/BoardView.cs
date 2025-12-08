using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoardView : MonoBehaviour, IController
{
    private Dictionary<Vector2Int, BoardCellView> cellViewDict = new Dictionary<Vector2Int, BoardCellView>();
    [SerializeField] private GameObject boardCellPrefab;
    private IBoardSystem boardSystem => this.GetSystem<IBoardSystem>();
    private Grid<BoardCell> grid => boardSystem.GetGrid();
    [SerializeField]private float cellSize = 1;
    private Vector3 offset;
    [SerializeField]private Vector3 showOffset = new Vector3(0, 0, 0);
    [SerializeField]private Vector3 hideOffset = new Vector3(0, 0, 0);
    private Vector3 centerOffset = new Vector3(0, 0, 0);
    void Start()
    {
        this.RegisterEvent<HideBoardEvent>(OnHide);
        this.RegisterEvent<ShowBoardEvent>(OnShow);
        offset = showOffset;
    }
    void Oestroy()
    {
        this.UnRegisterEvent<HideBoardEvent>(OnHide);
        this.UnRegisterEvent<ShowBoardEvent>(OnShow);
    }
    void Update()
    {
        if (grid == null) return;

        centerOffset = new Vector3(-grid.width * cellSize / 2, -grid.height * cellSize / 2, 0);
        foreach (var cellView in cellViewDict)
        {
            cellView.Value.transform.position = (Vector3)GetCenteredPos(cellView.Key.x, cellView.Key.y);
        }
    }
    private Vector3 GetCenteredPos(int x, int y) => new Vector3(x + 0.5f, y + 0.5f, 0)* cellSize + centerOffset + offset;
    public void Reset(){
        ClearCellViewDict();
        cellViewDict = new Dictionary<Vector2Int, BoardCellView>();
        OnShow();
    }
    private void OnShow(){
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                if (!cellViewDict.ContainsKey(new Vector2Int(i, j)))
                {
                    CreateBoardCellView(i, j);
                }
            }
        }
    }
    private void CreateBoardCellView(int x, int y){
        BoardCellView boardCellView = Instantiate(boardCellPrefab, transform).GetComponentInChildren<BoardCellView>();
        boardCellView.transform.position = new Vector3(x, y, 0);
        boardCellView.Init(grid.GetCell(x, y));
        cellViewDict[new Vector2Int(x, y)] = boardCellView;
    }
    private void ClearCellViewDict(){
        foreach (var cellView in cellViewDict)
        {
            Destroy(cellView.Value.gameObject);
        }
        cellViewDict.Clear();
    }
    public Transform GetCellTransform(Vector2Int position){
        return cellViewDict[position].transform;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    private void OnHide(HideBoardEvent evt) => OnHideBoard();
    private void OnShow(ShowBoardEvent evt) => OnShowBoard();
    [Button("隐藏")]
    private void OnHideBoard(){
        DOTween.To(() => offset, x => offset = x, hideOffset, 0.5f);
    }
    [Button("显示")]
    private void OnShowBoard(){
        DOTween.To(() => offset, x => offset = x, showOffset, 0.5f);
    }
}

public class HideBoardEvent : AbstractEvent{
}
public class ShowBoardEvent : AbstractEvent{
}