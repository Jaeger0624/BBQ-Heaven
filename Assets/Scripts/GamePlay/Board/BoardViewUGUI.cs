using System.Collections.Generic;
using MoreMountains.Feedbacks;
using QFramework;
using UnityEngine;
using UnityEngine.UI;


// 实际显示棋盘的UI
public class BoardViewUGUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public GameObject boardCellPrefab;
    public Transform boardCellParent;
    public GridLayoutGroup gridLayoutGroup;
    public Dictionary<Vector2Int, CellViewUI> boardCellDict = new Dictionary<Vector2Int, CellViewUI>();
    public List<Vector2Int> highlightedCells = new List<Vector2Int>();
    [Header("Feedback")]
    [SerializeField] private MMF_Player showFeedback;
    [Header("Other Components")]
    [SerializeField] private GameObject arrowParent;
    private void Start()
    {
        this.RegisterEvent<HideBoardEvent>(OnHide);
        this.RegisterEvent<ShowBoardEvent>(OnShow);
        this.RegisterEvent<HighlightCellsEvent>(OnHighlightCells);
        this.RegisterEvent<ClearAllBoardsHighlight>(OnClearAllBoardsHighlight);
        this.RegisterEvent<UpdateCellTileEvent>(OnUpdateCellTileEvent);
        this.RegisterEvent<ResetBoardEvent>(OnResetBoard);

        Hide();

    }
    private void OnDestroy()
    {
        this.UnRegisterEvent<HideBoardEvent>(OnHide);
        this.UnRegisterEvent<ShowBoardEvent>(OnShow);
        this.UnRegisterEvent<HighlightCellsEvent>(OnHighlightCells);
        this.UnRegisterEvent<ClearAllBoardsHighlight>(OnClearAllBoardsHighlight);
        this.UnRegisterEvent<UpdateCellTileEvent>(OnUpdateCellTileEvent);
        this.UnRegisterEvent<ResetBoardEvent>(OnResetBoard);
    }
    private void OnResetBoard(ResetBoardEvent evt)
    {
        UpdateBoardCell();
    }

    #region 公共API
    public void HighlightCells(List<Vector2Int> positions, bool reset)
    {
        // Debug.Log($"HighlightCells: {positions.Count}");
        if (reset) UnhighlightAll();
        foreach (var position in positions)
        {
            if (position == new Vector2Int(-1, -1)) continue;
            if (boardCellDict.TryGetValue(position, out var cellViewUI))
            {
                highlightedCells.Add(position);
                cellViewUI.Highlight();
            }
        }
    }
    public void UnhighlightAll()
    {
        // Debug.Log($"UnhighlightAll: {highlightedCells.Count}");
        foreach (var position in highlightedCells)
        {
            if (position == new Vector2Int(-1, -1)) continue;
            if (boardCellDict.TryGetValue(position, out var cellViewUI))
            {
                cellViewUI.Unhighlight();
            }
        }
        highlightedCells.Clear();
    }

    // 根据棋盘更新棋盘格
    public void UpdateBoardCell()
    {
        ClearBoardCellDict();

        foreach (var cell in this.GetSystem<IBoardSystem>().GetGrid().GetAllCells())
        {
            CellViewUI boardCell = Instantiate(boardCellPrefab, boardCellParent).GetComponent<CellViewUI>();
            boardCell.Init(cell);
            boardCell.transform.position = new Vector3(cell.position.x, cell.position.y, 0);
            boardCellDict.Add(cell.position, boardCell);
        }

        // 更新网格布局
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = this.GetSystem<IBoardSystem>().GetGrid().width;
    }
    public void Show()
    {
        // 播放方向
        showFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
        showFeedback.PlayFeedbacks();
    }
    public void Hide()
    {
        showFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
        showFeedback.PlayFeedbacks();
    }
    public void UpdateDirectionArrow(int directionValue, Vector2Int hoveredCellPos)
    {
        // 根据directionValue更新方向箭头
        switch (directionValue)
        {
            case 0:
                // 获取hoveredCellPos的最下方的cell
                BoardCell bottomCell = this.GetSystem<IBoardSystem>().GetGrid().GetCell(hoveredCellPos.x, 0);
                if (bottomCell != null)
                {
                    var tf = GetCellTransform(bottomCell.position);
                    if (tf != null) arrowParent.transform.position = tf.position;
                }
                arrowParent.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case 1:
                // 获取hoveredCellPos的最左方的cell
                BoardCell leftCell = this.GetSystem<IBoardSystem>().GetGrid().GetCell(0, hoveredCellPos.y);
                if (leftCell != null)
                {
                    var tf = GetCellTransform(leftCell.position);
                    if (tf != null) arrowParent.transform.position = tf.position;
                }
                arrowParent.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 2:
                // 获取hoveredCellPos的最上方的cell
                BoardCell TopCell = this.GetSystem<IBoardSystem>().GetGrid().GetCell(hoveredCellPos.x, this.GetSystem<IBoardSystem>().GetGrid().height - 1);
                if (TopCell != null)
                {
                    var tf = GetCellTransform(TopCell.position);
                    if (tf != null) arrowParent.transform.position = tf.position;
                }
                arrowParent.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 3:
                // 获取hoveredCellPos的最右方的cell
                BoardCell rightCell = this.GetSystem<IBoardSystem>().GetGrid().GetCell(this.GetSystem<IBoardSystem>().GetGrid().width - 1, hoveredCellPos.y);
                if (rightCell != null)
                {
                    var tf = GetCellTransform(rightCell.position);
                    if (tf != null) arrowParent.transform.position = tf.position;
                }
                arrowParent.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            default:
                Debug.LogError("【UpdateDirectionArrow】directionValue不合法");
                break;
        }
    }
    public void ChangeArrowVisible(bool visible) => arrowParent.SetActive(visible);
    #endregion
    private void ClearBoardCellDict()
    {
        foreach (var cell in boardCellDict)
        {
            Destroy(cell.Value.gameObject);
        }
        boardCellDict.Clear();
        // Reset 后清理旧的高亮引用，避免访问已销毁/已不存在的格子 key
        highlightedCells.Clear();
    }
    private void OnHide(HideBoardEvent evt) => Hide();
    private void OnShow(ShowBoardEvent evt) => Show();
    public Transform GetCellTransform(Vector2Int position)
    {
        if (boardCellDict.TryGetValue(position, out var cellViewUI))
        {
            return cellViewUI.gameObject.transform;
        }
        return null;
    }
    private void OnHighlightCells(HighlightCellsEvent evt) => HighlightCells(evt.positions, true);
    private void OnClearAllBoardsHighlight(ClearAllBoardsHighlight evt) => UnhighlightAll();
    private void OnUpdateCellTileEvent(UpdateCellTileEvent evt)
    {
        if (boardCellDict.TryGetValue(evt.position, out CellViewUI cellViewUI))
        {
            cellViewUI.UpdateVisual();
        }
    }
}
public class HighlightCellsEvent : AbstractEvent{
    public List<Vector2Int> positions;
    public HighlightCellsEvent(List<Vector2Int> positions){
        this.positions = positions;
    }
}
public class ClearAllBoardsHighlight : AbstractEvent{}

#region 事件
public class HideBoardEvent : AbstractEvent{}
public class ShowBoardEvent : AbstractEvent{}
#endregion