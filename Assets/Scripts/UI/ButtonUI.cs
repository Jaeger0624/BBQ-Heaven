using System;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum ButtonAnim
{
    None,
    Scale,
}


[RequireComponent(typeof(RectTransform))]
public class ButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IController, ICanSendEvent, IButton
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI buttonText;
    public UnityEvent OnClick;
    [ShowIf("isCustom")]
    public UnityEvent OnEnter;
    [ShowIf("isCustom")]
    public UnityEvent OnExit;
    public bool isCustom = false;
    private bool isHovering = false;
    public ButtonAnim anim = ButtonAnim.Scale;
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (isCustom)
        {
            OnEnter?.Invoke();
            return;
        }
        // 轻微放大
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1.1f, 0.12f)
                .SetEase(Ease.OutSine)
                .SetUpdate(true)
                .SetLink(this.gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (isCustom)
        {
            OnExit?.Invoke();
            return;
        }
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1f, 0.12f)
                .SetEase(Ease.OutSine)
                .SetUpdate(true)
                .SetLink(this.gameObject);
        }
    }

    public void SetText(string text){
        if (buttonText == null) return;
        buttonText.text = text;
    }

    public void AddListener(Action action)
    {
        OnClick.AddListener(new UnityAction(action));
    }

    public void RemoveListener(Action action)
    {
        OnClick.RemoveListener(new UnityAction(action));
    }
}
