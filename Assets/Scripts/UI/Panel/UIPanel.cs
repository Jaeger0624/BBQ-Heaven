using System;
using QFramework;
using UnityEngine;
using UnityEngine.Events;

public class UIPanel : MonoBehaviour, IController{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UIPanelType currentPanelType;
    public UnityEvent onShow;
    public UnityEvent onHide;
    [SerializeField] private bool isVisible = true;
    void Awake()
    {
        Hide();
        this.RegisterEvent<UIPanelEvent>(OnUIPanelEvent);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<UIPanelEvent>(OnUIPanelEvent);
    }
    private void OnUIPanelEvent(UIPanelEvent e){
        if (e.panelType == UIPanelType.无) {Debug.LogError("UIPanelEvent 永远不该触发“无”面板事件"); return;}
        if (e.panelType == currentPanelType){
            if (e.action == UIPanelAction.Show){
                Show();
            } else if (e.action == UIPanelAction.Hide){
                Hide();
            }
            else{
                Debug.LogError($"Unknown panel action: {e.action}");
                return;
            }
        }
    }
    public void Show(){
        if (isVisible) return;
        isVisible = true;    
        onShow.Invoke();

        if (canvasGroup == null) return;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void Hide(){
        if (!isVisible) return;
        isVisible = false;
        onHide.Invoke();

        if (canvasGroup == null) return;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}