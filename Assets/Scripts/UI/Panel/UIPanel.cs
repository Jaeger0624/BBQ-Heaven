using System;
using QFramework;
using UnityEngine;
using UnityEngine.Events;

public class UIPanel : MonoBehaviour, IController{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UIPanelType currentPanelType;
    public UnityEvent onShow;
    public UnityEvent onHide;
    void Awake()
    {
        Hide();
        this.RegisterEvent<UIPanelEvent>(OnUIPanelEvent);
    }
    void Start()
    {
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<UIPanelEvent>(OnUIPanelEvent);
    }
    private void OnUIPanelEvent(UIPanelEvent e){
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
        onShow.Invoke();

        if (canvasGroup == null) return;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void Hide(){
        onHide.Invoke();

        if (canvasGroup == null) return;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}