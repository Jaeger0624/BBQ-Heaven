using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 扩展成支持横向和竖向布局
/// </summary>
[ExecuteAlways]
public class SpriteLayout : MonoBehaviour
{
    [HideIf("UseTargetLength")]
    [Tooltip("固定间距（当UseTargetLength为false时使用）")]
    public float spacing = 0.1f;
    [ShowIf("UseTargetLength")]
    [Tooltip("目标长度（当UseTargetLength为true时使用，会将长度等分成n+1段间隔）")]
    public float targetLength = 1f;
    [Tooltip("是否居中对齐")]
    public bool centerAlign = true;
    [Tooltip("是否横向布局（false为纵向布局）")]
    public bool isHorizontal = true;
    [Tooltip("是否使用固定长度模式（将总长度等分成n+1段间隔，物体均匀分布）")]
    public bool UseTargetLength = false;
    [Tooltip("整体偏移量")]
    public Vector2 offset = new Vector2(0, 0);
    
    void Update()
    {
        if (isHorizontal)
        {
            UpdateHorizontalLayout();
        }
        else
        {
            UpdateVerticalLayout();
        }
    }

    private void UpdateHorizontalLayout(){
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                children.Add(child);
        }

        if (children.Count == 0) return;

        if (UseTargetLength)
        {
            // 固定长度模式：将总长度等分成 n+1 段间隔
            int segmentCount = children.Count + 1;
            float segmentLength = targetLength / segmentCount;
            
            // 计算每个物体的宽度
            List<float> widths = new List<float>();
            foreach (var child in children)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                float width = sr ? sr.bounds.size.x : 1f;
                widths.Add(width);
            }
            
            // 计算起始位置
            float startX = centerAlign ? -targetLength / 2f : 0f;
            
            // 排列物体
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float width = widths[i];
                // 第i个物体在第(i+1)段间隔之后
                // 位置 = 起始位置 + (i+1) * 间隔长度 + 物体宽度的一半
                float x = startX + (i + 1) * segmentLength + width / 2f;
                child.localPosition = new Vector3(x + offset.x, offset.y, child.localPosition.z);
            }
        }
        else
        {
            // 原有模式：使用固定间距
            float totalWidth = 0f;
            List<float> widths = new List<float>();

            // 计算每个Sprite的宽度
            foreach (var child in children)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                float width = sr ? sr.bounds.size.x : 1f;
                widths.Add(width);
                totalWidth += width + spacing;
            }
            totalWidth -= spacing;

            float startX = centerAlign ? -totalWidth / 2f : 0f;
            float x = startX;

            // 排列
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float width = widths[i];
                child.localPosition = new Vector3(x + width / 2f + offset.x, offset.y, child.localPosition.z);
                x += width + spacing;
            }
        }
    }

    private void UpdateVerticalLayout(){
        var children = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                children.Add(child);
        }

        if (children.Count == 0) return;

        if (UseTargetLength)
        {
            // 固定长度模式：将总长度等分成 n+1 段间隔
            int segmentCount = children.Count + 1;
            float segmentLength = targetLength / segmentCount;
            
            // 计算每个物体的高度
            List<float> heights = new List<float>();
            foreach (var child in children)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                float height = sr ? sr.bounds.size.y : 1f;
                heights.Add(height);
            }
            
            // 计算起始位置（从上方开始，Y轴向下）
            float startY = centerAlign ? targetLength / 2f : 0f;
            
            // 排列物体（从上方开始往下）
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float height = heights[i];
                // 第i个物体在第(i+1)段间隔之后（从上往下）
                // 位置 = 起始位置 - (i+1) * 间隔长度 - 物体高度的一半
                float y = startY - (i + 1) * segmentLength - height / 2f;
                child.localPosition = new Vector3(offset.x, y + offset.y, child.localPosition.z);
            }
        }
        else
        {
            // 原有模式：使用固定间距
            float totalHeight = 0f;
            List<float> heights = new List<float>();

            // 计算每个Sprite的高度
            foreach (var child in children)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                float height = sr ? sr.bounds.size.y : 1f;
                heights.Add(height);
                totalHeight += height + spacing;
            }
            totalHeight -= spacing;

            float startY = centerAlign ? totalHeight / 2f : 0f;
            float y = startY;

            // 排列（从上方开始往下）
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                float height = heights[i];
                child.localPosition = new Vector3(offset.x, y - height / 2f + offset.y, child.localPosition.z);
                y -= height + spacing;
            }
        }
    }
}
