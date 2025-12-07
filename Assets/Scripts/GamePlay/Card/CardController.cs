using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private GameObject cardHandViewPrefab;
    [SerializeField] private Transform cardContainer;
    private Dictionary<string, CardHandView> cardViews = new Dictionary<string, CardHandView>();
    [Header("手牌位置")]
    public Vector3 showPosition;
    public Vector3 hidePosition;
    void OnEnable()
    {
        this.RegisterEvent<CreateCardViewEvent>(OnCreateCardViewEvent);
        this.RegisterEvent<CardViewUseEvent>(OnUseCardHandViewEvent);
        this.RegisterEvent<RemoveCardViewEvent>(OnRemoveCardViewEvent);
        this.RegisterEvent<CombineBBQEvent>(OnCombineBBQEvent);
        this.RegisterEvent<FinishCombineBBQEvent_动画>(OnFinishCombineBBQEvent_动画);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CreateCardViewEvent>(OnCreateCardViewEvent);
        this.UnRegisterEvent<RemoveCardViewEvent>(OnRemoveCardViewEvent);
        this.UnRegisterEvent<CombineBBQEvent>(OnCombineBBQEvent);
        this.UnRegisterEvent<FinishCombineBBQEvent_动画>(OnFinishCombineBBQEvent_动画);
    }
    private void OnCombineBBQEvent(CombineBBQEvent e) => Hide();
    private void OnFinishCombineBBQEvent_动画(FinishCombineBBQEvent_动画 e) => Show();
    private void OnCreateCardViewEvent(CreateCardViewEvent e) => CreateCard(e.card);
    private void OnRemoveCardViewEvent(RemoveCardViewEvent e) => RemoveCard(e.card);
    private void OnUseCardHandViewEvent(CardViewUseEvent e) => UseCard(e.card);
    [Button]
    private void Show(){
        transform.DOLocalMove(showPosition, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
    }
    [Button]
    private void Hide(){
        transform.DOLocalMove(hidePosition, 0.5f).SetUpdate(true).SetEase(Ease.InBack);
    }
    private void CreateCard(Card card){
        CardHandView cardHandView = HandVisualManager.Instance.AddCard(card, cardHandViewPrefab);
        cardViews.Add(card.guid, cardHandView);
    }
    private void RemoveCard(Card card){
        if (!cardViews.TryGetValue(card.guid, out CardHandView cardHandView)) {Debug.LogWarning($"Card {card.guid} not found"); return;}
        // 1. 移除手牌视图
        HandVisualManager.Instance.RemoveCard(cardHandView);
        // 2. 移除字典中的记录
        cardViews.Remove(cardHandView.card.guid);
    }

    private void UseCard(Card card){
        // 1. 传给卡牌系统使用（后续处理等卡牌系统反应）
        this.GetSystem<ICardSystem>().UseCard(card, new List<object>());
    }


    [Button]
    private void AddCard(){
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData("1");
        if (cardData == null){
            Debug.LogError("CardData not found");
            return;
        }
        Card card = new Card(cardData);
        CreateCard(card);
    }
}