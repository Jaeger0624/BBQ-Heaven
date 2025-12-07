using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 2D Sprite网格布局组件，支持灵活的参数调节
/// </summary>
[ExecuteAlways]
public class SpriteGridLayout : MonoBehaviour
{
    [Header("布局模式")]
    [Tooltip("布局约束方式：固定列数、固定行数、固定宽度、固定高度、或自动计算")]
    public LayoutConstraint constraint = LayoutConstraint.FixedColumnCount;
    
    [Tooltip("固定的列数（当约束为固定列数时使用）")]
    public int columnCount = 3;
    
    [Tooltip("固定的行数（当约束为固定行数时使用）")]
    public int rowCount = 3;
    
    [Tooltip("目标宽度（当约束为固定宽度时使用，会自动计算水平间距）")]
    public float targetWidth = 10f;
    
    [Tooltip("目标高度（当约束为固定高度时使用，会自动计算垂直间距）")]
    public float targetHeight = 10f;

    [Header("间距设置")]
    [Tooltip("网格单元之间的间距（当使用固定宽度/高度模式时，此值会被自动计算覆盖）")]
    public Vector2 cellSpacing = new Vector2(0.1f, 0.1f);
    
    [Tooltip("整体偏移量")]
    public Vector2 offset = new Vector2(0, 0);

    [Header("对齐方式")]
    [Tooltip("水平对齐方式")]
    public HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
    
    [Tooltip("垂直对齐方式")]
    public VerticalAlignment verticalAlignment = VerticalAlignment.Center;
    
    [Tooltip("起始角落（网格排列的起始位置）")]
    public StartCorner startCorner = StartCorner.UpperLeft;

    [Header("其他选项")]
    [Tooltip("是否只排列激活的子物体")]
    public bool onlyActiveChildren = true;
    
    [Tooltip("是否保持子物体的Z坐标")]
    public bool preserveZ = true;

    public enum LayoutConstraint
    {
        FixedColumnCount,  // 固定列数
        FixedRowCount,     // 固定行数
        FixedWidth,        // 固定宽度（自动计算水平间距）
        FixedHeight,       // 固定高度（自动计算垂直间距）
        Automatic          // 自动计算（尽量接近正方形）
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum StartCorner
    {
        UpperLeft,   // 左上角开始
        UpperRight,  // 右上角开始
        LowerLeft,   // 左下角开始
        LowerRight   // 右下角开始
    }

    private void Update()
    {
        UpdateGridLayout();
    }

    private void UpdateGridLayout()
    {
        // 获取所有需要排列的子物体
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (!onlyActiveChildren || child.gameObject.activeSelf)
            {
                children.Add(child);
            }
        }

        if (children.Count == 0) return;

        // 计算实际的行列数
        int actualColumns, actualRows;
        CalculateGridSize(children.Count, out actualColumns, out actualRows);

        // 计算每个单元格的大小
        Vector2 cellSize = CalculateCellSize(children);

        // 根据布局模式计算实际的间距（可能需要自动计算）
        Vector2 actualSpacing = CalculateActualSpacing(actualColumns, actualRows, cellSize);

        // 计算总网格尺寸
        Vector2 totalGridSize = new Vector2(
            actualColumns * cellSize.x + (actualColumns - 1) * actualSpacing.x,
            actualRows * cellSize.y + (actualRows - 1) * actualSpacing.y
        );

        // 计算起始位置
        Vector2 startPosition = CalculateStartPosition(totalGridSize, actualColumns, actualRows, cellSize);

        // 排列子物体
        for (int i = 0; i < children.Count; i++)
        {
            int row = i / actualColumns;
            int col = i % actualColumns;

            // 根据起始角落调整行列索引
            int adjustedRow, adjustedCol;
            AdjustIndicesForCorner(row, col, actualRows, actualColumns, out adjustedRow, out adjustedCol);

            // 计算位置
            // startPosition 是网格左上角的位置
            // 从左上角开始，向右和向下排列
            float x = startPosition.x + adjustedCol * (cellSize.x + actualSpacing.x) + cellSize.x * 0.5f + offset.x;
            // Y轴：从上到下，所以用减法
            float y = startPosition.y - adjustedRow * (cellSize.y + actualSpacing.y) - cellSize.y * 0.5f + offset.y;
            
            float z = preserveZ ? children[i].localPosition.z : 0f;
            children[i].localPosition = new Vector3(x, y, z);
        }
    }

    private void CalculateGridSize(int childCount, out int columns, out int rows)
    {
        switch (constraint)
        {
            case LayoutConstraint.FixedColumnCount:
                columns = columnCount;
                rows = Mathf.CeilToInt((float)childCount / columnCount);
                break;
            
            case LayoutConstraint.FixedRowCount:
                rows = rowCount;
                columns = Mathf.CeilToInt((float)childCount / rowCount);
                break;
            
            case LayoutConstraint.FixedWidth:
                // 固定宽度模式：需要先知道列数才能计算，先使用固定列数或自动计算
                // 如果设置了columnCount就用它，否则自动计算
                if (columnCount > 0)
                {
                    columns = columnCount;
                }
                else
                {
                    columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                }
                rows = Mathf.CeilToInt((float)childCount / columns);
                break;
            
            case LayoutConstraint.FixedHeight:
                // 固定高度模式：需要先知道行数才能计算，先使用固定行数或自动计算
                // 如果设置了rowCount就用它，否则自动计算
                if (rowCount > 0)
                {
                    rows = rowCount;
                }
                else
                {
                    rows = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                }
                columns = Mathf.CeilToInt((float)childCount / rows);
                break;
            
            case LayoutConstraint.Automatic:
                // 尽量接近正方形
                columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                rows = Mathf.CeilToInt((float)childCount / columns);
                break;
            
            default:
                columns = columnCount;
                rows = Mathf.CeilToInt((float)childCount / columnCount);
                break;
        }

        // 确保至少为1
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);
    }

    /// <summary>
    /// 根据布局模式计算实际的间距（可能需要自动计算）
    /// </summary>
    private Vector2 CalculateActualSpacing(int columns, int rows, Vector2 cellSize)
    {
        Vector2 spacing = cellSpacing;

        switch (constraint)
        {
            case LayoutConstraint.FixedWidth:
                // 固定宽度：根据目标宽度自动计算水平间距
                // totalWidth = columns * cellSize.x + (columns - 1) * spacing.x
                // => spacing.x = (targetWidth - columns * cellSize.x) / (columns - 1)
                if (columns > 1)
                {
                    float totalCellWidth = columns * cellSize.x;
                    float availableSpace = targetWidth - totalCellWidth;
                    spacing.x = availableSpace / (columns - 1);
                    // 确保间距不为负
                    spacing.x = Mathf.Max(0f, spacing.x);
                }
                else
                {
                    spacing.x = 0f;
                }
                break;

            case LayoutConstraint.FixedHeight:
                // 固定高度：根据目标高度自动计算垂直间距
                // totalHeight = rows * cellSize.y + (rows - 1) * spacing.y
                // => spacing.y = (targetHeight - rows * cellSize.y) / (rows - 1)
                if (rows > 1)
                {
                    float totalCellHeight = rows * cellSize.y;
                    float availableSpace = targetHeight - totalCellHeight;
                    spacing.y = availableSpace / (rows - 1);
                    // 确保间距不为负
                    spacing.y = Mathf.Max(0f, spacing.y);
                }
                else
                {
                    spacing.y = 0f;
                }
                break;
        }

        return spacing;
    }

    private Vector2 CalculateCellSize(List<Transform> children)
    {
        float maxWidth = 0f;
        float maxHeight = 0f;

        foreach (var child in children)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Vector2 size = sr.bounds.size;
                maxWidth = Mathf.Max(maxWidth, size.x);
                maxHeight = Mathf.Max(maxHeight, size.y);
            }
            else
            {
                // 如果没有SpriteRenderer，使用默认尺寸
                maxWidth = Mathf.Max(maxWidth, 1f);
                maxHeight = Mathf.Max(maxHeight, 1f);
            }
        }

        return new Vector2(maxWidth, maxHeight);
    }

    private Vector2 CalculateStartPosition(Vector2 totalGridSize, int columns, int rows, Vector2 cellSize)
    {
        float startX = 0f;
        float startY = 0f;

        // 计算网格左上角的位置（对齐方式决定整体网格位置）
        // 水平对齐：决定网格的左边界位置
        switch (horizontalAlignment)
        {
            case HorizontalAlignment.Left:
                startX = 0f;
                break;
            case HorizontalAlignment.Center:
                startX = -totalGridSize.x * 0.5f;
                break;
            case HorizontalAlignment.Right:
                startX = -totalGridSize.x;
                break;
        }

        // 垂直对齐：决定网格的整体位置（Unity中Y轴向上）
        // startY 是网格左上角（顶部）的Y坐标
        // 网格高度为 totalGridSize.y，所以网格底部在 startY - totalGridSize.y
        switch (verticalAlignment)
        {
            case VerticalAlignment.Top:
                // 顶部对齐：网格顶部在上方
                // 假设顶部对齐到 totalGridSize.y（可以根据需要调整）
                startY = totalGridSize.y;
                break;
            case VerticalAlignment.Center:
                // 居中对齐：网格中心在原点(0,0)
                // 中心在0，顶部在 totalGridSize.y * 0.5f
                startY = totalGridSize.y * 0.5f;
                break;
            case VerticalAlignment.Bottom:
                // 底部对齐：网格底部在下方
                // 假设底部对齐到 -totalGridSize.y（或某个负值位置）
                // 顶部在 -totalGridSize.y + totalGridSize.y = 0
                // 但为了与Center对齐有区别，可以设置为 -totalGridSize.y * 0.5f + totalGridSize.y
                // 更合理的做法：底部在 -totalGridSize.y，顶部在 0
                startY = 0f;
                break;
        }

        // 注意：起始角落只影响排列顺序，不影响起始位置
        // 起始位置始终是网格左上角（逻辑上的左上角）
        return new Vector2(startX, startY);
    }

    private void AdjustIndicesForCorner(int row, int col, int totalRows, int totalCols, out int adjustedRow, out int adjustedCol)
    {
        // 这个方法将逻辑行列索引转换为实际的网格位置索引
        // 始终使用左上角作为参考点，通过调整索引来实现不同起始角落的排列
        
        switch (startCorner)
        {
            case StartCorner.UpperLeft:
                // 从左上角开始：从左到右，从上到下（默认顺序）
                adjustedRow = row;
                adjustedCol = col;
                break;
            
            case StartCorner.UpperRight:
                // 从右上角开始：从右到左，从上到下
                // 列索引反转，行索引不变
                adjustedRow = row;
                adjustedCol = totalCols - 1 - col;
                break;
            
            case StartCorner.LowerLeft:
                // 从左下角开始：从左到右，从下到上
                // 行索引反转，列索引不变
                adjustedRow = totalRows - 1 - row;
                adjustedCol = col;
                break;
            
            case StartCorner.LowerRight:
                // 从右下角开始：从右到左，从下到上
                // 行列索引都反转
                adjustedRow = totalRows - 1 - row;
                adjustedCol = totalCols - 1 - col;
                break;
            
            default:
                adjustedRow = row;
                adjustedCol = col;
                break;
        }
    }
}