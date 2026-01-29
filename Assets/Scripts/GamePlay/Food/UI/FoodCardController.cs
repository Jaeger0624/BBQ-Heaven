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


            // 2. 执行动画
            // 随机发射方向
            float randomAngle = UnityEngine.Random.Range(-10, 10);
            foodCardView.transform.localRotation = Quaternion.Euler(0, 0, randomAngle);

            seq.AppendCallback(() => {
                foodCardView.gameObject.SetActive(true);
            });
            Vector2 localMove = (Vector2.up * 100).Rotate(randomAngle);
            seq.Append(foodCardView.transform.DOLocalMove(localMove, foodCardAnimDuration).SetEase(Ease.OutSine)).UnScaledKill(foodCardView.gameObject);
            seq.Join(foodCardView.transform.DOScale(0, foodCardAnimDuration).SetEase(Ease.InBack).UnScaledKill(foodCardView.gameObject).OnComplete(() => {
                if (this != null && gameObject != null) {
                    Destroy(foodCardView.gameObject);
                }
            })).UnScaledKill(foodCardView.gameObject);
        }
        seq.Play();

        int amount = foodCards.Count;
        if (amount > 0){
            FloatingTextManager.Instance.Show(foodCardContainer.position + new Vector3(0, 0.5f, 0), $"抽取{amount}张食材卡牌", Color.white, new FloatingTextInfo("", 1.2f, Color.white));
        }
        else{
            FloatingTextManager.Instance.Show(foodCardContainer.position + new Vector3(0, 0.5f, 0), $"食材仓储已空\n<color=yellow>请补充食材</color>", Color.white, new FloatingTextInfo("", 1.2f, Color.white));
        }
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