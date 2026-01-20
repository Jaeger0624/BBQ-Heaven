using QFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IController, ICanSendEvent, IPointerEnterHandler, IPointerExitHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public UnityEvent OnPointerEnterEvent = new UnityEvent();
    public UnityEvent OnPointerExitEvent = new UnityEvent();
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}