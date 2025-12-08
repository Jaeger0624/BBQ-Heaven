using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

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
    private void Start() {
        this.RegisterEvent<HideBoardEvent>(OnHide);
        this.RegisterEvent<ShowBoardEvent>(OnShow);
        this.RegisterEvent<HighlightCellsEvent>(OnHighlightCells);
        this.RegisterEvent<ClearAllBoardsHighlight>(OnClearAllBoardsHighlight);
        UpdateBoardCell();

        Hide();

    }
    private void OnDestroy()
    {
        this.UnRegisterEvent<HideBoardEvent>(OnHide);
        this.UnRegisterEvent<ShowBoardEvent>(OnShow);
        this.UnRegisterEvent<HighlightCellsEvent>(OnHighlightCells);
        this.UnRegisterEvent<ClearAllBoardsHighlight>(OnClearAllBoardsHighlight);
    }
    public void HighlightCells(List<Vector2Int> positions, bool reset){
        // Debug.Log($"HighlightCells: {positions.Count}");
        if (reset) UnhighlightAll();
        foreach (var position in positions){
            highlightedCells.Add(position);
            boardCellDict[position].Highlight();
        }
    }
    public void UnhighlightAll(){
        // Debug.Log($"UnhighlightAll: {highlightedCells.Count}");
        foreach (var position in highlightedCells){
            boardCellDict[position].Unhighlight();
        }
        highlightedCells.Clear();
    }
    
    // 根据棋盘更新棋盘格
    public void UpdateBoardCell(){
        ClearBoardCellDict();
        foreach (var cell in this.GetSystem<IBoardSystem>().GetGrid().GetAllCells()){
            CellViewUI boardCell = Instantiate(boardCellPrefab, boardCellParent).GetComponent<CellViewUI>();
            boardCell.Init(cell);
            boardCell.transform.position = new Vector3(cell.position.x, cell.position.y, 0);
            boardCellDict.Add(cell.position, boardCell);
        }
    }
    private void ClearBoardCellDict(){
        foreach (var cell in boardCellDict){
            Destroy(cell.Value.gameObject);
        }
        boardCellDict.Clear();
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

    private void OnHide(HideBoardEvent evt) => Hide();  
    private void OnShow(ShowBoardEvent evt) => Show();
    public Transform GetCellTransform(Vector2Int position){
        return boardCellDict[position].gameObject.transform;
    }

    [Button]
    public void TestHighlight(Vector2Int position){
        boardCellDict[position].Highlight();
    }
    private void OnHighlightCells(HighlightCellsEvent evt) => HighlightCells(evt.positions, true);
    private void OnClearAllBoardsHighlight(ClearAllBoardsHighlight evt) => UnhighlightAll();
}
public class HighlightCellsEvent : IEvent{
    public List<Vector2Int> positions;
    public HighlightCellsEvent(List<Vector2Int> positions){
        this.positions = positions;
    }
}
public class ClearAllBoardsHighlight : IEvent{}