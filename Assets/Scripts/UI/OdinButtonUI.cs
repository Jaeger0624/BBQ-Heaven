
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public interface IButton{
    public void AddListener(Action action);
    public void RemoveListener(Action action);
}
public class OdinButtonUI : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IButton{
    [ShowIf("isCustom")]
    public UnityEvent OnEnter;
    [ShowIf("isCustom")]
    public UnityEvent OnExit;
    public UnityEvent OnClick;
    [SerializeField] private bool isCustom = false;
    [SerializeField] private ButtonAnim anim = ButtonAnim.Scale;

    public void AddListener(Action action)
    {
        OnClick.AddListener(new UnityAction(action));
    }
    public void RemoveListener(Action action)
    {
        OnClick.RemoveListener(new UnityAction(action));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isCustom)
        {
            OnEnter?.Invoke();
            return;
        }
        // 轻微放大
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1.1f, 0.12f).SetEase(Ease.OutSine).SetUpdate(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isCustom)
        {
            OnExit?.Invoke();
            return;
        }
        if (anim == ButtonAnim.Scale)
        {
            transform.DOScale(1f, 0.12f).SetEase(Ease.OutSine).SetUpdate(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}