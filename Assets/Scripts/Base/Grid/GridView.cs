using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IGridView{
    /// <summary>
    /// 只暴露一个方法，用于设置/更新网格
    /// </summary>
    /// <param name="grid">网格</param>
    void SetGrid(GridBase grid);
}

public class GridView : MonoBehaviour,IGridView
{
    
    private Dictionary<Vector2Int, TextMeshPro> textDict = new Dictionary<Vector2Int, TextMeshPro>();
    private GridBase grid;
    [SerializeField]private float cellSize = 1;
    [SerializeField]private bool autoScale = true;
    [SerializeField]private float textSize = 1;
    [SerializeField]private float ratio = 2f;
    [SerializeField]private Vector2 offset = new Vector2(0, 0);
    private Vector2 centerOffset = new Vector2(0, 0);
    public void SetGrid(GridBase grid)
    {
        this.grid = grid;
        ClearTextDict();
        textDict = new Dictionary<Vector2Int, TextMeshPro>();
        OnShow();
    }
    private void OnShow()
    {
        // 画网格
        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                if (!textDict.ContainsKey(new Vector2Int(i, j)))
                {
                    CreateTextMeshProUGUI(i, j, grid.OnShow(i, j));
                }
            }
        }
    }
    private void ClearTextDict()
    {
        foreach (var text in textDict)
        {
            Destroy(text.Value.gameObject);
        }
        textDict.Clear();
    }
    private void CreateTextMeshProUGUI(int x, int y, string text)
    {
        GameObject gameObject = new GameObject($"CellInfo({x},{y})");
        gameObject.transform.parent = transform;
        gameObject.transform.position = new Vector3(x, y, 0);
        TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
        textMeshPro.text = text;
        textMeshPro.fontSize = textSize;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textDict[new Vector2Int(x, y)] = textMeshPro;
    }
    protected virtual void Update()
    {
        if (grid == null) return;



        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClick();
        }
    }
    void LateUpdate()
    {
        BasicUpdate();
    }

    private void BasicUpdate(){
        if (autoScale)
        {
            textSize = cellSize * ratio;
        }
        centerOffset = new Vector2(-grid.width * cellSize / 2, -grid.height * cellSize / 2);
        foreach (var text in textDict)
        {
            text.Value.transform.position = (Vector3)GetCenteredPos(text.Key.x, text.Key.y);
            text.Value.fontSize = textSize;
        }
    }
    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int i = 0; i < grid.width; i++)
        {
            for (int j = 0; j < grid.height; j++)
            {
                Gizmos.DrawWireCube((Vector3)GetCenteredPos(i, j), new Vector3(cellSize, cellSize, 0));
            }
        }
    }
    private Vector2 GetCenteredPos(int x, int y) => new Vector2(x + 0.5f, y + 0.5f)* cellSize + centerOffset + offset;
    private void OnMouseClick(){
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        OnTrigger(worldPos);
    }
    private void OnTrigger(Vector3 worldPos){
        Vector2Int cellPos = WorldToCell(worldPos);
        if (!IsInGrid(cellPos)) return;
        Debug.Log($"OnClick: {cellPos}");
    }
    private Vector2Int WorldToCell(Vector3 worldPos){
        Vector2 originPos = new Vector2(worldPos.x, worldPos.y) - centerOffset - offset;
        if (originPos.x < 0 || originPos.y < 0 || originPos.x > grid.width * cellSize || originPos.y > grid.height * cellSize)
        {
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int((int)(originPos.x / cellSize), (int)(originPos.y / cellSize));
    }

    private Vector3 CellToWorld(Vector2Int cellPos) => new Vector2(cellPos.x + 0.5f, cellPos.y + 0.5f)* cellSize + centerOffset + offset;
    private bool IsInGrid(Vector2Int cellPos) => cellPos.x >= 0 && cellPos.y >= 0 && cellPos.x < grid.width && cellPos.y < grid.height;
}
