using QFramework;

public class MoneyDontEnoughEvent : AbstractEvent{
    public int price;
    public MoneyDontEnoughEvent(int price){
        this.price = price;
    }
}

public class CreateShopPanelEvent : AbstractEvent
{
    public ShopInitContext shopInitContext;
    public CreateShopPanelEvent(ShopInitContext shopInitContext){
        this.shopInitContext = shopInitContext;
    }
}

public class CloseShopPanelEvent : AbstractEvent
{
    public CloseShopPanelEvent(){}
}