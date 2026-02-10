using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(IShowTooltip))]
public class TooltipParent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    public Transform targetTransform;
    public TooltipAnchor anchor = TooltipAnchor.中心;
    public TooltipAlignType alignType = TooltipAlignType.右;
    public IDisposable showTask;
    public IShowTooltip tooltip;
    public bool followMouse => targetTransform == null;
    private float waitTime => SettingManager.Instance.DevSettings.tooltipWaitTime;
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 不受时间缩放影响
        // 等待0.5s
        showTask = Observable
            .Timer(TimeSpan.FromSeconds(waitTime), Scheduler.MainThread)
            .Subscribe(_ => {
                ShowTooltip();
            });
        showTask.AddTo(this);
    }
    void Start()
    {
        tooltip = GetComponent<IShowTooltip>();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //立即隐藏 
        TooltipManager.Instance.Hide();
        showTask?.Dispose();
    }

    private void ShowTooltip(){
        TooltipManager.Instance.ShowTooltip(this);
    }
}


public enum TooltipAlignType{
    左,
    右,
}