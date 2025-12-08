using System;
using QFramework;
using UnityEngine;

public class UIPanel : MonoBehaviour, IController{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UIPanelType currentPanelType;
    void Start()
    {
        this.RegisterEvent<UIPanelEvent>(OnUIPanelEvent);
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
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void Hide(){
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}