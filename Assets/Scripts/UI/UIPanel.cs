using System;
using QFramework;
using UnityEngine;

public class UIPanel : MonoBehaviour, IController{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] public Type showEventType;
    [SerializeField] private Type hideEventType;
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