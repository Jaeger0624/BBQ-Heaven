
using QFramework;

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