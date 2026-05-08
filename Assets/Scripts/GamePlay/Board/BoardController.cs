using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

// 用于处理鼠标悬浮逻辑
public class BoardController : MonoBehaviour, IController, ICanSendEvent
{
    private IStickSystem stickSystem => this.GetSystem<IStickSystem>();
    [SerializeField] private BoardViewUGUI boardViewUGUI;
    private float cellSize => boardViewUGUI.gridLayoutGroup.cellSize.x;
    private RectTransform gridContainer => boardViewUGUI.gridLayoutGroup.transform as RectTransform;
    BoardCell hoveredCell;
    private List<BoardCell> highlightedCells = new List<BoardCell>();
    void Start()
    {
        this.RegisterEvent<ChangePanelEvent>(OnChangePanel).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<ResetBoardEvent>(OnResetBoard).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }

    private void OnResetBoard(ResetBoardEvent evt)
    {
        // Reset 时清掉鼠标悬浮/高亮/预览状态，避免 frame 内继续用旧 cell/数据进行交互计算
        boardViewUGUI.UnhighlightAll();
        highlightedCells.Clear();
        SendClearEvents();
        hoveredCell = null;
        boardViewUGUI.ChangeArrowVisible(false);
        this.GetSystem<BlackboardSystem>().hoveredCell = null;
    }
    void Update()
    {
        if (this.GetSystem<ICardSystem>().State != CardSystemState.正常)
        {
            boardViewUGUI.ChangeArrowVisible(false);
            return;
        }
        Stick selectedStick = stickSystem.selectedStick;
        if (selectedStick == null)
        {
            boardViewUGUI.UnhighlightAll();
            boardViewUGUI.ChangeArrowVisible(false);
            return;
        }

        Vector2 screenPos = Input.mousePosition;
        if (TryGetGridIndex(screenPos, out Vector2Int gridIndex))
        {
            // Debug.Log($"gridIndex: {gridIndex}");
            if (!boardViewUGUI.boardCellDict.TryGetValue(gridIndex, out var cellViewUI) || cellViewUI == null)
            {
                boardViewUGUI.UnhighlightAll();
                highlightedCells.Clear();
                SendClearEvents();
                hoveredCell = null;
                boardViewUGUI.ChangeArrowVisible(false);
                return;
            }

            BoardCell newHoveredCell = cellViewUI.cell;

            // 如果鼠标右键点击，则增加方向值
            if (Input.GetMouseButtonDown(1))
            {
                IStickStrategy.directionValue += 1;
            }

            // 1. 高亮范围
            IStickStrategy updateStrategy = selectedStick.strategy;
            List<BoardCell> newHighlightedCells = updateStrategy.GetRange(newHoveredCell.position, selectedStick);

            // 2. 修改方向箭头
            ShowDirectionArrow(IStickStrategy.directionValue, newHoveredCell.position);
            boardViewUGUI.ChangeArrowVisible(true);

            if (Input.GetMouseButtonDown(1))
            {
                UpdateView(newHoveredCell.position, newHighlightedCells, selectedStick);
            }
            else if (hoveredCell != newHoveredCell && newHoveredCell != null)
            {
                hoveredCell = newHoveredCell;
                if (!newHighlightedCells.SequenceEqual(this.highlightedCells))
                {
                    UpdateView(hoveredCell.position, newHighlightedCells, selectedStick);
                }
            }
            hoveredCell = newHoveredCell;
        }
        else
        {
            boardViewUGUI.UnhighlightAll();
            highlightedCells.Clear();
            SendClearEvents();
            hoveredCell = null;
            boardViewUGUI.ChangeArrowVisible(false);
        }

        this.GetSystem<BlackboardSystem>().hoveredCell = hoveredCell;
    }
    private void SendClearEvents()
    {
        this.SendEvent(new HideBBQPreviewEvent());
        this.SendEvent(new TimePreviewEvent(0));
        this.SendEvent(new ResetRecipePreviewViewsEvent());
    }
    public void UpdateView(Vector2Int hoveredCellPos, List<BoardCell> highlightedCells, Stick selectedStick)
    {
        // 如果未选中烤串，则不更新视图
        if (selectedStick == null)
        {
            // Debug.Log("未选中烤串");
            SendClearEvents();
            return;
        }
        //TODO: 暂时写死，后续需要优化

        // 1. 计算时间消耗
        List<FoodInstance> foodInstances = selectedStick.strategy.GetFood(hoveredCellPos, selectedStick);
        int amount = this.GetSystem<ITimeSystem>().GetCostTime(foodInstances, selectedStick);

        // 2. 更新高亮单元格
        this.highlightedCells = highlightedCells;
        boardViewUGUI.HighlightCells(highlightedCells.Select(x => x.position).ToList(), true);


        BBQPreview preview = PreviewBBQ(selectedStick, foodInstances, highlightedCells);

        // 2. 设置预览
        this.GetSystem<IBBQSystem>().SetPreview(preview);

        // 3. 发送预览事件
        this.SendEvent(new ShowBBQPreviewEvent(preview));
        this.SendEvent(new TimePreviewEvent(amount));
    }
    // 预览BBQ结果
    private BBQPreview PreviewBBQ(Stick selectedStick, List<FoodInstance> foodInstances, List<BoardCell> highlightedCells)
    {
        int totalRarity = 0;
        int totalTaste = 0;
        int totalTimeCost = 0;
        foreach (var foodInstance in foodInstances)
        {
            totalRarity += foodInstance.rarity;
            totalTaste += foodInstance.taste;
        }
        totalTimeCost = this.GetSystem<ITimeSystem>().GetCostTime(foodInstances, selectedStick);
        return new BBQPreview(totalRarity, totalTaste, totalTimeCost, highlightedCells);
    }

    private void OnChangePanel(ChangePanelEvent evt)
    {
        if (evt.newPanel == ProcessPanel.Kitchen)
        {
            boardViewUGUI.Show();
        }
        else
        {
            boardViewUGUI.Hide();
        }
    }

    private bool TryGetGridIndex(Vector2 screenPos, out Vector2Int gridIndex)
    {
        gridIndex = Vector2Int.zero;

        // 1. 将屏幕坐标转换为 GridContainer 内部的局部坐标
        Vector2 localPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainer,
            screenPos,
            Camera.main, // 如果是 Overlay 模式传 null，Camera 模式传 UI Camera
            out localPos))
        {
            return false;
        }

        // 2. 坐标系修正 (假设 GridContainer 的 Pivot 是 0.5, 0.5)
        // 需要把原点从中心移到左下角，方便计算
        float pivotOffsetX = gridContainer.rect.width * gridContainer.pivot.x;
        float pivotOffsetY = gridContainer.rect.height * gridContainer.pivot.y;

        float x = localPos.x + pivotOffsetX;
        float y = localPos.y + pivotOffsetY;

        // 3. 检查边界
        if (x < 0 || x > gridContainer.rect.width || y < 0 || y > gridContainer.rect.height)
        {
            return false;
        }

        // 4. 计算索引
        int indexX = Mathf.FloorToInt(x / cellSize);
        int indexY = Mathf.FloorToInt(y / cellSize);

        // 5. 再对照逻辑层棋盘尺寸做边界校验（Reset 后 UI 容器尺寸可能滞后）
        var grid = this.GetSystem<IBoardSystem>().GetGrid();
        if (grid == null) return false;
        if (indexX < 0 || indexX >= grid.width || indexY < 0 || indexY >= grid.height) return false;

        gridIndex = new Vector2Int(indexX, indexY);
        return true;
    }
    private void ShowDirectionArrow(int directionValue, Vector2Int hoveredCellPos) => boardViewUGUI.UpdateDirectionArrow(directionValue, hoveredCellPos);
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }


    [Button]
    private void ResetBoard(int width, int height)
    {
        this.GetSystem<IBoardSystem>().ResetGrid(width, height);
    }
}
public class AddSelectedViewEvent{
    public Vector2Int pos;
    public AddSelectedViewEvent(Vector2Int pos){
        this.pos = pos;
    }
}
