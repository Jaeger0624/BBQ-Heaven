using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public interface IShopItem{
    public abstract string id { get; }
    public abstract string name { get; }
    public int originPrice { get; }
    public int price { get; }
    public ShopItemType type { get; }
    public TooltipInfo tooltipInfo { get; set; }
    void OnBuy();

    
}

public abstract class AbstractShopItem : IShopItem, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string id { get; }
    public abstract string name { get; }
    public int originPrice { get; set; }
    public int price { get; set; }
    public abstract ShopItemType type { get; }
    public TooltipInfo tooltipInfo { get; set; }
    public abstract void OnBuy();
}




public class FoodShopItem : AbstractShopItem{
    public override ShopItemType type => ShopItemType.Food; 
    public int amount { get; set; }
    public override string id => foodData.ID;
    public override string name => foodData.Name;
    private FoodData foodData;  
    public FoodShopItem(FoodData foodData, int amount, int price){
        this.foodData = foodData;
        this.originPrice = price;
        this.price = price;
        this.amount = amount;
        this.tooltipInfo = new TooltipInfo(foodData.Description);
    }

    public FoodShopItem(FoodData foodData, int amount){
        this.foodData = foodData;
        this.originPrice = 0;
        this.price = 0;
        this.amount = amount;
        this.tooltipInfo = new TooltipInfo(foodData.Description);
    }
    public override void OnBuy(){
        // Debug.Log($"购买食材包: {name} 数量: {amount}");

        // 1. 扣除金币
        this.GetSystem<IEconomySystem>().CostCoin(price);

        // 2. 添加食材
        this.GetSystem<IFoodSystem>().AddFoodToRepository(new List<FoodPack>(){new FoodPack(id, amount)});
    }
}
public class MascotShopItem : AbstractShopItem{
    public override ShopItemType type => ShopItemType.Mascot;
    public override string id => mascotData.ID;
    public override string name => mascotData.Name;
    private MascotData mascotData;
    public MascotShopItem(MascotData mascotData, int price){
        this.mascotData = mascotData;
        this.originPrice = price;
        this.price = price;
        this.tooltipInfo = new TooltipInfo(mascotData.Description);
    }
    public MascotShopItem(MascotData mascotData){
        this.mascotData = mascotData;
        this.originPrice = 0;
        this.price = 0;
        this.tooltipInfo = new TooltipInfo(mascotData.Description);
    }
    public override void OnBuy(){
        Debug.Log($"购买吉祥物{name}");
    }
}

public class StickShopItem : AbstractShopItem{
    public override ShopItemType type => ShopItemType.Stick;
    public override string id => stickData.ID;
    public override string name => stickData.Name;
    private StickData stickData;
    public StickShopItem(StickData stickData, int price){
        this.stickData = stickData;
        this.originPrice = price;
        this.price = price;
        this.tooltipInfo = new TooltipInfo(stickData.Description);
    }
    public StickShopItem(StickData stickData){
        this.stickData = stickData;
        this.originPrice = 0;
        this.price = 0;
        this.tooltipInfo = new TooltipInfo(stickData.Description);
    }
    public override void OnBuy(){
        Debug.Log("购买烤串");
    }
}
