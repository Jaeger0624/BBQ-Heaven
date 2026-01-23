using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipManager : MonoBehaviour
{
    private static TooltipManager instance;
    public static TooltipManager Instance => instance;
    [SerializeField] private TooltipUI tooltipUI;
    [SerializeField] private bool useTooltip = true;
    private bool isShowing = false;
    public bool IsShowing => isShowing;
    private IDisposable disposable;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Show(List<TooltipInfo> tooltipInfos){
        if (!useTooltip){
            Debug.Log("ShowTooltip: useTooltip is false");
            return;
        }
        if (isShowing){
            Debug.LogError("ShowTooltip: isShowing is true");
            return;
        }
        isShowing = true;
        // TODO: 先负责显示一个TooltipInfo
        tooltipUI.Show(tooltipInfos[0].description);
    }
    public void Hide(){
        if (!useTooltip) return;
        if (!isShowing) return;
        isShowing = false;
        tooltipUI.Hide();
    }

    void Start()
    {
        if (!useTooltip) return;
        
        tooltipUI.Hide();
        isShowing = false;
    }
    public void ShowTooltip(TooltipParent tooltipComponent){
        IShowTooltip tooltip = tooltipComponent.tooltip;
        List<TooltipInfo> tooltipInfos = tooltip.GetTooltipInfo();

        if (tooltipComponent.followMouse){
            // 设置跟随鼠标
            tooltipUI.SetParent(null);
            tooltipUI.ChangeFollowMouse(true);
        }
        else{
            // 设置跟随目标
            tooltipUI.SetParent(tooltipComponent);
            tooltipUI.ChangeFollowMouse(false);
        }

        Show(tooltipInfos);
    }    
}
