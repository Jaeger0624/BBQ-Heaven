public enum ShopItemType{
    Food,
    Mascot,
    Stick,
    Recipe,
}


public class ShopTask{
    public ShopItemType type;
    public int amount;
    public IPriceStrategy priceStrategy;
    public ShopTask(ShopItemType type, int amount, IPriceStrategy priceStrategy){
        this.type = type;
        this.amount = amount;
        this.priceStrategy = priceStrategy;
    }
}