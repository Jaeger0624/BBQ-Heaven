using cfg;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class FoodIndexer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private FoodType foodType;
    [SerializeField] public Transform ViewTransform;
    [SerializeField] private TextMeshProUGUI foodTypeName;
    [SerializeField] private TextMeshProUGUI numberText;
    private bool isHovered = false;
    public void Init(FoodType foodType){
        this.foodType = foodType;
        UpdateVisual(1);
    }
    public void UpdateVisual(int count){
        foodTypeName.text = foodType.ToString();
        numberText.text = count.ToString();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        ViewTransform.DOLocalMoveX(0, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        ViewTransform.DOLocalMoveX(-15, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
    }
}
