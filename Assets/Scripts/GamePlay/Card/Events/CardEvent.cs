
using System.Collections.Generic;
using QFramework;

#region 卡牌事件
public class PileUpdateEvent : AbstractEvent{}
public class CreateCardViewEvent : AbstractEvent{
    public Card card;
    public CreateCardViewEvent(Card card){
        this.card = card;
    }
}
public class RemoveCardViewEvent : AbstractEvent{
    public Card card;
    public RemoveCardViewEvent(Card card){
        this.card = card;
    }
}
public class CardViewUseEvent : AbstractEvent{
    public Card card;
    public CardViewUseEvent(Card card){
        this.card = card;
    }
}

public class DrawMultiCardsEvent : AbstractEvent{
    public List<Card> cards;
    public DrawMultiCardsEvent(List<Card> cards){
        this.cards = cards;
    }
}

public class DrawSingleCardEvent : AbstractEvent, IMascotEvent{
    public Card card;
    public DrawSingleCardEvent(Card card){
        this.card = card;
    }
    public List<object> parameters => new List<object>{};
}
#endregion