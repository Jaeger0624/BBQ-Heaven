using QFramework;

public class MoneyDontEnoughEvent : AbstractEvent{
    public int price;
    public MoneyDontEnoughEvent(int price){
        this.price = price;
    }
}