public interface IPriceStrategy{
    int GetPrice(IShopItem shopItem);
}

public class Price_免费 : IPriceStrategy{
    public int GetPrice(IShopItem shopItem){
        return 0;
    }
}


public class PriceStrategy_原值 : IPriceStrategy
{
    public int GetPrice(IShopItem shopItem)
    {
        throw new System.NotImplementedException();
    }
}
