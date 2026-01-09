using System;
using System.Collections.Generic;
using System.Linq;

public class SelectionContext<TData>
{
    public List<TData> SelectedItems { get; private set; } = new List<TData>();
    public int MaxCount { get; private set; }

    // --- 事件 ---
    // 数量变更事件 (供 Panel 监听以更新 UI)
    public Action<int> OnCountChanged; 
    // 提交事件 (供 Panel 监听以关闭自己)
    public Action<List<TData>> OnConfirm; 

    public SelectionContext(int maxCount)
    {
        MaxCount = maxCount;
    }

    // --- 操作方法 (供 Strategy 调用) ---
    public bool Toggle(TData item)
    {
        bool isSelected = false;
        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);
            isSelected = false;
        }
        else
        {
            if (SelectedItems.Count < MaxCount)
            {
                SelectedItems.Add(item);
                isSelected = true;
            }
            else return false; // 超过上限，操作无效
        }
        
        // 通知 UI 更新
        OnCountChanged?.Invoke(SelectedItems.Count);
        return isSelected;
    }
    // 提交结果
    public void Confirm()
    {
        OnConfirm?.Invoke(SelectedItems);
    }
}
