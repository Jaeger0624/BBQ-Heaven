using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class FoodIndexer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanSendEvent, IFoodIndexer
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private FoodType foodType;
    [SerializeField] public Transform ViewTransform;
    [SerializeField] private TextMeshProUGUI foodTypeName;
    [SerializeField] private TextMeshProUGUI numberText;
    private bool isHovered = false;
    private IndexRuleBase indexRule;
    public void Init(FoodType foodType){
        this.foodType = foodType;
        UpdateVisual(1);

        indexRule = new IndexRule_FoodType(foodType);
    }
    public void UpdateVisual(int count){
        foodTypeName.text = foodType.ToString();
        numberText.text = count.ToString();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        ViewTransform.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);

        this.SendEvent(new ResetFoodIndexEvent());
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ViewTransform.DOLocalMoveX(-15, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);

        this.SendEvent(new TriggerFoodIndexEvent(this));
    }

    public List<FoodInstance> GetFoodInstances() => indexRule.GetFoodInstances();
}
