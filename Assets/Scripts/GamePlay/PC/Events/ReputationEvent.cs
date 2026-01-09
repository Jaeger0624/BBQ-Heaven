using QFramework;

public class ReputationChangedEvent : AbstractEvent{
    public int amount;
    public ReputationChangedEvent(int amount){
        this.amount = amount;
    }
}