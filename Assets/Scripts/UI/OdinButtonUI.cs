
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
public interface IButton{
    public void AddListener(Action action);
    public void RemoveListener(Action action);
}
public class OdinButtonUI : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IButton{
    [OdinSerialize]
    public Action OnEnter;
    [OdinSerialize, ShowIf("isCustom")]
    public Action OnExit;
    [OdinSerialize, ShowIf("isCustom")]
    public Action OnClick;
    [SerializeField] private bool isCustom = false;
    [SerializeField] private ButtonAnim anim = ButtonAnim.Scale;

    public void AddListener(Action action)
    {
        OnClick += action;
    }
    public void RemoveListener(Action action)
    {
        OnClick -= action;
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
}