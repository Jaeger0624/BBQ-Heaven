using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

[Serializable]
public class CardPile : ICanSendEvent, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public List<Card> handPile;
    public List<Card> drawPile;
    public List<Card> discardPile;

    private int maxDrawAmount = 10;

    public CardPile(List<Card> initialCards){
        // 初始时将卡牌加入抽牌堆
        drawPile = initialCards.ToList();
        handPile = new List<Card>();
        discardPile = new List<Card>();
    }

    /// <summary> 抽取卡牌 </summary>
    public List<Card> DrawCard(int count){
        List<Card> cards = new List<Card>();
        for (int i = 0; i < count; i++) {
            Card card = DrawSingleCard();
            if (card == null) break;
            cards.Add(card);
        }
        this.SendEvent(new DrawMultiCardsEvent(cards));
        return cards;
    }

    private Card DrawSingleCard(){
        if (handPile.Count >= maxDrawAmount){
            Debug.LogWarning("手牌数量已达到最大值，无法抽取卡牌");
            return null;
        }
        if (drawPile.Count == 0) {
            RefillDrawPile();
        }
        if (drawPile.Count == 0){
            Debug.LogWarning("重置抽牌堆后卡牌堆仍为空，无法抽取卡牌");
            return null;
        }
        Card card = drawPile[0];
        drawPile.RemoveAt(0);
        handPile.Add(card);
        this.SendEvent(new CreateCardViewEvent(card));
        this.SendEvent(new DrawSingleCardEvent(card));
        this.SendEvent(new PileUpdateEvent());
        return card;
    }

    private void RefillDrawPile(){
        // 1. 将弃牌堆中的卡牌转移到抽牌堆
        drawPile.AddRange(discardPile);
        discardPile.Clear();

        // 2. 洗牌
        ShuffleDrawPile();
    }

    private void ShuffleDrawPile(){
        // 使用子系统种子完成洗牌
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICardSystem>();

        drawPile = rng.PickMany<Card>(drawPile, drawPile.Count).ToList();
    }

    /// <summary> 弃置卡牌 </summary>
    public void DiscardCard(List<Card> cards){
        List<Card> cardsToDiscard = cards.ToList();

        discardPile.AddRange(cardsToDiscard);
        handPile.RemoveAll(card => cardsToDiscard.Contains(card));

        cardsToDiscard.ForEach(card => this.SendEvent(new RemoveCardViewEvent(card)));
        this.SendEvent(new PileUpdateEvent());
    }

    /// <summary> 使用卡牌 </summary>
    public void UseCard(Card card, List<object> param){
        // 2. 执行卡牌效果
        card.OnUse(param);

        // 3. 将卡牌从手牌中移除
        handPile.Remove(card);
        discardPile.Add(card);

        // 4. 发送移除卡牌事件
        this.SendEvent(new RemoveCardViewEvent(card));
        this.SendEvent(new PileUpdateEvent());
    }

    public void AddCardToHand(Card card){
        handPile.Add(card);
        this.SendEvent(new CreateCardViewEvent(card));
        this.SendEvent(new PileUpdateEvent());
    }
}


