
using QFramework;


public interface IShopSystem : ISystem, ICanSendQuery{
    void GenerateShop(ShopInitContext shopInitContext);
}
public class ShopSystem : AbstractSystem, IShopSystem{
    protected override void OnInit()
    {
        
    }
    public void GenerateShop(ShopInitContext shopInitContext)
    {
        this.SendEvent(new CreateShopPanelEvent(shopInitContext));
    }
}
