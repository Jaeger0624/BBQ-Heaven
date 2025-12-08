
using QFramework;

public class PileUpdateEvent : IEvent{}
public class CreateCardViewEvent : IEvent{
    public Card card;
    public CreateCardViewEvent(Card card){
        this.card = card;
    }
}
public class RemoveCardViewEvent : IEvent{
    public Card card;
    public RemoveCardViewEvent(Card card){
        this.card = card;
    }
}
public class CardViewUseEvent : IEvent{
    public Card card;
    public CardViewUseEvent(Card card){
        this.card = card;
    }
}