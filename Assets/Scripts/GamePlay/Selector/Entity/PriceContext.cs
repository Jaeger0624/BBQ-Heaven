[System.Serializable]
public class PriceContext{
    public int Price;
    public int OriginalPrice;
    public bool IsDiscount;
    public PriceContext(int price){
        Price = price;
        OriginalPrice = price;
        IsDiscount = false;
    }
    public void SetDiscount(int discount){
        OriginalPrice = Price;
        Price = discount;
        IsDiscount = true;
    }
}