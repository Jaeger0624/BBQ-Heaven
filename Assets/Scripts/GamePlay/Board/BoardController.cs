using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

// 用于处理鼠标悬浮逻辑
public class BoardController : MonoBehaviour, IController, ICanSendEvent
{
    private IStickSystem stickSystem => this.GetSystem<IStickSystem>();
    [SerializeField] private BoardViewUGUI boardViewUGUI;
    private float cellSize => boardViewUGUI.gridLayoutGroup.cellSize.x;
    private RectTransform gridContainer => boardViewUGUI.gridLayoutGroup.transform as RectTransform;
    BoardCell hoveredCell;
    void Start()
    {
        this.RegisterEvent<ChangePanelEvent>(OnChangePanel).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    void Update()
    {
        if (this.GetSystem<ICardSystem>().State != CardSystemState.正常) return;
        if (stickSystem.selectedStick == null) {boardViewUGUI.UnhighlightAll(); return;}

        Vector2 screenPos = Input.mousePosition;
        if (TryGetGridIndex(screenPos, out Vector2Int gridIndex)){
            // Debug.Log($"gridIndex: {gridIndex}");
            BoardCell newHoveredCell = boardViewUGUI.boardCellDict[gridIndex].cell;

            // 2. 如果鼠标在棋盘上，且按下右键，则修改横竖方向
            if (Input.GetMouseButtonDown(1)){
                IStickStrategy.IsHorizontal = !IStickStrategy.IsHorizontal;
                // Debug.Log($"IsHorizontal: {IStickStrategy.IsHorizontal}");
                UpdateView(hoveredCell.position);
            }
            else if (hoveredCell != newHoveredCell && newHoveredCell != null){
                hoveredCell = newHoveredCell;
                UpdateView(hoveredCell.position);
            }
        }
        else{
            boardViewUGUI.UnhighlightAll();
            this.SendEvent(new HideBBQPreviewEvent());
            this.SendEvent(new TimePreviewEvent(0));
            hoveredCell = null;
        }

        this.GetSystem<BlackboardSystem>().hoveredCell = hoveredCell;
        
    }

    public void UpdateView(Vector2Int hoveredCellPos){
        // 如果未选中烤串，则不更新视图
        if (this.GetSystem<IStickSystem>().selectedStick == null){
            // Debug.Log("未选中烤串");
            this.SendEvent(new HideBBQPreviewEvent());
            this.SendEvent(new TimePreviewEvent(0));
            return;
        }
        //TODO: 暂时写死，后续需要优化
        IStickStrategy updateStrategy = this.GetSystem<IStickSystem>().selectedStick.strategy;

        // 1. 计算时间消耗
        List<FoodInstance> foodInstances = updateStrategy.GetFood(hoveredCellPos);
        Stick selectedStick = this.GetSystem<IStickSystem>().selectedStick;
        int amount = this.GetSystem<ITimeSystem>().GetCostTime(foodInstances, selectedStick);
        

        List<BoardCell> highlightedCells = updateStrategy.GetRange(hoveredCellPos);
        boardViewUGUI.HighlightCells(highlightedCells.Select(x => x.position).ToList(), true);

        BBQPreview preview = PreviewBBQ(selectedStick, foodInstances, highlightedCells);

        // 2. 设置预览
        this.GetSystem<IBBQSystem>().SetPreview(preview);

        // 3. 发送预览事件
        this.SendEvent(new ShowBBQPreviewEvent(preview));
        this.SendEvent(new TimePreviewEvent(amount));
    }
    // 预览BBQ结果
    private BBQPreview PreviewBBQ(Stick selectedStick, List<FoodInstance> foodInstances, List<BoardCell> highlightedCells){
        int totalRarity = 0;
        int totalTaste = 0;
        int totalTimeCost = 0;
        foreach (var foodInstance in foodInstances){
            totalRarity += foodInstance.rarity;
            totalTaste += foodInstance.taste;
        }
        totalTimeCost = this.GetSystem<ITimeSystem>().GetCostTime(foodInstances, selectedStick);
        return new BBQPreview(totalRarity, totalTaste, totalTimeCost, highlightedCells);
    }

    private void OnChangePanel(ChangePanelEvent evt){
        if (evt.newPanel == ProcessPanel.Kitchen){
            boardViewUGUI.Show();
        }else{
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

        gridIndex = new Vector2Int(indexX, indexY);
        return true;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
public class AddSelectedViewEvent{
    public Vector2Int pos;
    public AddSelectedViewEvent(Vector2Int pos){
        this.pos = pos;
    }
}
