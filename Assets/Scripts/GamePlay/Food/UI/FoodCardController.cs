using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class FoodCardController : MonoBehaviour, IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI foodCardAmountText;
    [SerializeField] private Transform foodCardContainer;
    private DisplayFoodView foodCardPrefab => SettingManager.Instance.PrefabSettings.foodCardItemPrefab;
    [SerializeField] private float foodCardAnimDuration = 0.25f;
    void Start()
    {
        this.RegisterEvent<FoodCardPileUpdateEvent>(OnFoodCardPileUpdate);
        this.RegisterEvent<DrawFoodCardEvent>(OnDrawFoodCardEvent);
    }

    private void OnDestroy() 
    {
        this.UnRegisterEvent<FoodCardPileUpdateEvent>(OnFoodCardPileUpdate);
        this.UnRegisterEvent<DrawFoodCardEvent>(OnDrawFoodCardEvent);

        DOTween.Kill(this);
    }

    private void OnFoodCardPileUpdate(FoodCardPileUpdateEvent evt)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (foodCardAmountText == null) {Debug.LogError("FoodCardController 的 foodCardAmountText 为空"); return;}
        if (this.GetSystem<IFoodSystem>().FoodPile == null) {Debug.LogError("FoodPile 为空"); return;}
        if (this.GetSystem<IFoodSystem>().FoodPile.DrawFoodPile == null) {Debug.LogError("DrawFoodPile 为空"); return;}

        foodCardAmountText.text = this.GetSystem<IFoodSystem>().FoodPile.DrawFoodPile.Count.ToString();
    }
    private void OnDrawFoodCardEvent(DrawFoodCardEvent e)
    {
        SingleFoodCard(e.foodCards);
    }
    private void SingleFoodCard(List<FoodCard> foodCards){
        Sequence seq = DOTween.Sequence();

        foreach (var foodCard in foodCards){
            DisplayFoodView foodCardView = Instantiate(foodCardPrefab, foodCardContainer);
            foodCardView.gameObject.SetActive(false);

            // 1. 绑定数据
            foodCardView.Bind(new FoodUIContext(foodCard));

            // 一个向上出现后缩放消失的动画
            // 创建一个动作动画，将foodCardView setActive(true) 并执行一个动作动画，然后执行一个缩放动画，然后执行一个销毁动画
            seq.AppendCallback(() => {
                foodCardView.gameObject.SetActive(true);
            });
            seq.Append(foodCardView.transform.DOLocalMoveY(100, foodCardAnimDuration).SetEase(Ease.OutSine)).SetUpdate(true);
            seq.Join(foodCardView.transform.DOScale(0, foodCardAnimDuration).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                Destroy(foodCardView.gameObject);
            }));
        }
        seq.Play();
    }
    public void OnRefreshFoodPileButtonClick()
    {
        this.GetSystem<IFoodSystem>().RefreshFoodPile();
    }

    public void OnButtonEnter(){
        if (this.GetSystem<IFoodSystem>().FoodPile.JustRefreshed) return;
        this.SendEvent(new TimePreviewEvent(this.GetSystem<IFoodSystem>().RefreshFoodCost));
    }
    public void OnButtonExit(){
        if (this.GetSystem<IFoodSystem>().FoodPile.JustRefreshed) return;
        this.SendEvent(new TimePreviewEvent(0));
    }
}