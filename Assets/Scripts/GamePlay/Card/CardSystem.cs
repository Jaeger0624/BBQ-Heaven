using UnityEngine;
using QFramework;
using System.Collections.Generic;
using System.Linq;
using cfg;
public enum CardSystemState{
    正常,
    选择目标,
    卡牌使用中,
}
public interface ICardSystem : ISystem, ISavable{
    CardPile CardPile { get; }
    CardSystemState State { get; set; }
    void InitCardPile();
    // 添加卡牌到卡牌库
    void AddCardToRepository(string cardId);
    void AddCardToHand(string cardId);
    // 从卡牌库中移除卡牌
    void RemoveCardFromRepository(string guid);
    List<Card> DrawCard(int count);
    void DiscardCard(List<Card> cards);
    void RefreshHandCards();
    void UseCard(Card card, List<object> param);

}
public class CardSystem : AbstractSystem, ICardSystem
{
    // 卡牌系统负责维护一个卡牌仓库，用于存储所有卡牌
    // Guid -> Card
    private Dictionary<string, Card> cardRepository;
    private int drawAmount = 3;
    public CardSystemState State { get; set; } = CardSystemState.正常;
    // 每单日运营开始时，卡牌系统负责初始化卡牌堆
    private CardPile cardPile;
    public CardPile CardPile => cardPile;
    protected override void OnInit()
    {
        cardRepository = new Dictionary<string, Card>();
        cardPile = null;

        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<FinishCombineBBQEvent>(OnFinishCombineBBQ);
        this.RegisterEvent<EndDayEvent>(OnEndDay);
    }
    protected override void OnDeinit()
    {
        cardRepository.Clear();
        cardPile = null;

        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<FinishCombineBBQEvent>(OnFinishCombineBBQ);
        this.UnRegisterEvent<EndDayEvent>(OnEndDay);
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.cardRepositorys = cardRepository;
        // Debug.Log($"卡牌仓库数量: {cardRepository.Count}");
    }
    public void Load(GameArchive archive)
    {
        Dictionary<string, Card> newCardRepository = archive.playerInfoData.cardRepositorys.ToDictionary(card => card.Key, card => card.Value);
        if (newCardRepository.Count == 0){
            Debug.LogError("卡牌仓库为空，无法加载");
            return;
        }
        cardRepository = newCardRepository;
    }
    private void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        
        InitCardPile();
        // 抽卡
        DrawCard(drawAmount);
    }
    private void OnFinishCombineBBQ(FinishCombineBBQEvent evt)
    {
        // 1. 刷新手牌
        RefreshHandCards();

        DrawCard(drawAmount);
    }


    private void OnEndDay(EndDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        cardPile.DiscardCard(cardPile.handPile);
        cardPile = null;
    }

    public void AddCardToRepository(string cardId)
    {
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData(cardId);
        if (cardData == null) {Debug.LogError("卡牌数据不存在：" + cardId); return;}
        Card card = new Card(cardData);
        cardRepository.Add(card.guid, card);
    }

    public void AddCardToHand(string cardId)
    {
        CardData cardData = this.GetSystem<IDataSystem>().GetCardData(cardId);
        if (cardData == null) return;
        Card card = new Card(cardData);
        cardPile.AddCardToHand(card);
    }
    public void RemoveCardFromRepository(string guid)
    {
        cardRepository.Remove(guid);
    }

    public void InitCardPile()
    {
        if (cardRepository.Count == 0){
            Debug.LogError("卡牌仓库为空，无法初始化卡牌堆");
            return;
        }
        cardPile = new CardPile(cardRepository.Values.ToList());
        this.SendEvent(new PileUpdateEvent());
    }

    public List<Card> DrawCard(int count)
    {
        if (cardPile == null) {
            Debug.LogWarning("CardPile not initialized");
            InitCardPile();
        }
        return cardPile.DrawCard(count);
    }

    public void DiscardCard(List<Card> cards)
    {
        if (cardPile == null) {
            Debug.LogWarning("CardPile not initialized");
            InitCardPile();
        }
        cardPile.DiscardCard(cards);
    }

    public void UseCard(Card card, List<object> param)
    {
        if (cardPile == null) {Debug.LogError("CardPile not initialized"); return;}
        cardPile.UseCard(card, param);
    }
    public void RefreshHandCards()
    {
        if (cardPile == null) {Debug.LogError("CardPile not initialized"); return;}
        cardPile.DiscardCard(cardPile.handPile);
    }

}
