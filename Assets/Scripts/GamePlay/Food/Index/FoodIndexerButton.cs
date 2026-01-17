using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;

public class FoodIndexerButton : SerializedMonoBehaviour, ICanSendEvent, IPointerEnterHandler, IPointerExitHandler, IFoodIndexer{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [OdinSerialize]
    public IndexRuleBase indexRule;
    public List<FoodInstance> GetFoodInstances()
    {
        return indexRule.GetFoodInstances();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.SendEvent(new TriggerFoodIndexEvent(this));

        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        this.SendEvent(new ResetFoodIndexEvent());
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }
}