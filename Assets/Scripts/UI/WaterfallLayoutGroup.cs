using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("Layout/Waterfall Layout Group")]
public class WaterfallLayoutGroup : LayoutGroup
{
    public enum ConstraintType
    {
        FixedColumnCount, // 固定列数 (例如: 3列)
        FixedColumnWidth  // 固定宽度 (例如: 200px宽, 能放下几列放几列)
    }

    [Header("Settings")]
    public ConstraintType constraintType = ConstraintType.FixedColumnCount;
    [Min(1)] public int columnCount = 3;
    [Min(10)] public float columnWidth = 200f;
    
    public Vector2 spacing = Vector2.zero;

    // 缓存每一列当前的高度
    private float[] _columnHeights;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        // 1. 计算实际列数
        int actualColumnCount = 0;
        float actualCellWidth = 0;
        float parentWidth = rectTransform.rect.width;

        if (constraintType == ConstraintType.FixedColumnCount)
        {
            actualColumnCount = columnCount;
            // 宽度 = (总宽 - Padding - 间隙) / 列数
            float availableWidth = parentWidth - padding.left - padding.right - (spacing.x * (actualColumnCount - 1));
            actualCellWidth = availableWidth / actualColumnCount;
        }
        else
        {
            // 基于宽度的计算
            float availableWidth = parentWidth - padding.left - padding.right;
            actualColumnCount = Mathf.FloorToInt((availableWidth + spacing.x) / (columnWidth + spacing.x));
            actualColumnCount = Mathf.Max(1, actualColumnCount);
            // 这种模式下，我们要反推CellWidth来填满剩余空间，或者保持固定
            // 这里选择填满模式：
            float totalSpacing = spacing.x * (actualColumnCount - 1);
            actualCellWidth = (availableWidth - totalSpacing) / actualColumnCount;
        }

        // 2. 初始化列高度数组
        _columnHeights = new float[actualColumnCount];
        for (int i = 0; i < actualColumnCount; i++)
        {
            _columnHeights[i] = padding.top;
        }

        // 3. 遍历子物体，设置宽度并计算位置
        // 注意：这里我们使用 rectChildren，它会自动过滤掉不活跃(Inactive)的物体
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];

            // 决定这个物体放在哪一列
            // 策略A：顺序排列 (0->Col0, 1->Col1, 2->Col2, 3->Col0...) -> 符合Grid阅读习惯
            // 策略B：找最短列 (Pinterest风格) -> 适合图片流，但文字阅读顺序会乱
            // 这里我们采用【策略A】，因为你说是“Grid信息面板”
            int columnIndex = i % actualColumnCount;

            // X坐标
            float xPos = padding.left + (actualCellWidth + spacing.x) * columnIndex;
            
            // 设置子物体的宽度 (这是关键，TMP需要有了宽度才能计算换行后的高度)
            SetChildAlongAxis(child, 0, xPos, actualCellWidth);
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        // 这一步在 Horizontal 之后执行，此时子物体的 Width 已经设定好了
        // TMP 等组件应该已经计算出了 PreferredHeight

        if (_columnHeights == null || _columnHeights.Length == 0) return;

        float maxColumnHeight = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            int columnIndex = i % _columnHeights.Length;

            // 获取子物体在当前宽度下想要的高度
            float childHeight = LayoutUtility.GetPreferredHeight(child);

            // 当前列的高度就是子物体的Y轴起始点
            float yPos = _columnHeights[columnIndex];

            // 设置子物体高度和Y坐标
            SetChildAlongAxis(child, 1, yPos, childHeight);

            // 更新该列的高度
            _columnHeights[columnIndex] += childHeight + spacing.y;
            
            // 记录最高的一列
            if (_columnHeights[columnIndex] > maxColumnHeight)
            {
                maxColumnHeight = _columnHeights[columnIndex];
            }
        }

        // 加上底部的Padding
        maxColumnHeight += padding.bottom;

        // 设置自身的高度，以便 ContentSizeFitter 能够撑开 ScrollView 的 Content
        SetLayoutInputForAxis(maxColumnHeight, maxColumnHeight, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        // LayoutGroup 标准流程，触发计算
    }

    public override void SetLayoutVertical()
    {
        // LayoutGroup 标准流程，触发计算
    }
}