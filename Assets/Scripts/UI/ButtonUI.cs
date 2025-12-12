using System;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum ButtonAnim
{
    None,
    Scale,
}

[RequireComponent(typeof(RectTransform))]
public class ButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public UnityEvent OnClick;
    public ButtonAnim anim = ButtonAnim.Scale;
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 轻微放大
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1.1f, 0.12f).SetEase(Ease.OutSine).SetUpdate(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1f, 0.12f).SetEase(Ease.OutSine).SetUpdate(true);
        }
    }
}
