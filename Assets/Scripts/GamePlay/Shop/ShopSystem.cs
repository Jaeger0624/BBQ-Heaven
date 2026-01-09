using System.Collections.Generic;
using System.Linq;
using System.Text;
using cfg;
using QFramework;
using UnityEngine;

public interface IShopSystem : ISystem, ICanSendQuery{
    /// <summary>
    /// 购买商品
    /// </summary>
    /// <param name="shopItem">商品</param>
    public void Buy(IShopItem shopItem);
    /// <summary>
    /// 生成食材补充商品
    /// </summary>
    public void GenerateFoodSupplyShopItems();
    /// <summary>
    /// 生成每日商品
    /// </summary>
    public void GenerateDailyShopItems();
    public List<IShopItem> BuildConcreteShopItem(List<ShopTask> originTasks, ShopItemType shopItemType, IRandomStrategy strategy);
    public Dictionary<ShopItemType, List<IShopItem>> BuildShopItems(List<ShopTask> shopTasks, IRandomStrategy strategy);
}
public class ShopSystem : AbstractSystem, IShopSystem{
    private int foodShopItemAmount_每日 = 6;
    private int mascotShopItemAmount_每日 = 3;
    private int stickShopItemAmount_每日 = 2;

    protected override void OnInit()
    {
        
    }

    public void Buy(IShopItem shopItem){
        shopItem.OnBuy();
    }
    public void GenerateFoodSupplyShopItems(){
        // 1. 构建ShopTasks
        List<ShopTask> shopTasks = BuildShopTasks();
        // 2. 获取ShopItems
        List<IShopItem> foodShopItems = BuildConcreteShopItem(shopTasks, ShopItemType.Food, new Random_纯随机());
        // 3. 发送事件
        this.SendEvent(new CreateShop_日前提升(foodShopItems));
    }
    public void GenerateDailyShopItems(){

        //TODO: 先用纯随机逻辑生成
        // 1. 构建ShopTasks
        List<ShopTask> shopTasks = BuildShopTasks();
        Dictionary<ShopItemType, List<IShopItem>> shopItemAmounts = BuildShopItems(shopTasks, new Random_纯随机());
        
        // Debug.Log($"生成每日商品: {ShopItemAmountsToString(shopItemAmounts)}");
        this.SendEvent(new CreateShopEvent_日间商店(shopItemAmounts));
    }

    private string ShopItemAmountsToString(Dictionary<ShopItemType, List<IShopItem>> shopItemAmounts){
        StringBuilder sb = new StringBuilder();
        foreach (var shopItemAmount in shopItemAmounts){
            string names = string.Join(",", shopItemAmount.Value.Select(item => item.name));
            sb.Append($"\n{shopItemAmount.Key}: {names} ({shopItemAmount.Value.Count})");
        }
        return sb.ToString();
    }

    private List<ShopTask> BuildShopTasks(){
        List<ShopTask> shopTasks = new List<ShopTask>
        {
            new ShopTask(ShopItemType.Food, foodShopItemAmount_每日, new PriceStrategy_原值()),
            new ShopTask(ShopItemType.Mascot, mascotShopItemAmount_每日, new PriceStrategy_原值()),
            new ShopTask(ShopItemType.Stick, stickShopItemAmount_每日, new PriceStrategy_原值()),
        };
        return shopTasks;
    }

    public List<IShopItem> BuildConcreteShopItem(List<ShopTask> originTasks, ShopItemType shopItemType, IRandomStrategy strategy){
        List<ShopTask> shopTasks = originTasks.Where(task => task.type == shopItemType).ToList();
        List<IShopItem> shopItems = new List<IShopItem>();
        foreach (var shopTask in shopTasks){
            List<IShopItem> items = FinishShopTask(shopTask, strategy);
            shopItems.AddRange(items);
        }
        return shopItems;
    }

    public Dictionary<ShopItemType, List<IShopItem>> BuildShopItems(List<ShopTask> shopTasks, IRandomStrategy strategy){
        // 1. 创建空字典
        Dictionary<ShopItemType, List<IShopItem>> shopItems = new Dictionary<ShopItemType, List<IShopItem>>();

        // 2. 遍历ShopTasks
        foreach (var shopTask in shopTasks){
            List<IShopItem> items = FinishShopTask(shopTask, strategy);
            if (!shopItems.ContainsKey(shopTask.type)){
                shopItems[shopTask.type] = new List<IShopItem>();
            }
            shopItems[shopTask.type].AddRange(items);
        }
        return shopItems;
    }

    private List<IShopItem> FinishShopTask(ShopTask shopTask, IRandomStrategy strategy){
        IRandomSystem randomSystem = this.GetSystem<IRandomSystem>();
        switch (shopTask.type){
            case ShopItemType.Food:
                return ConcreteItemConverter<FoodData>(randomSystem.RandomGet<FoodData>(shopTask.amount, strategy));
            case ShopItemType.Mascot:
                return ConcreteItemConverter<MascotData>(randomSystem.RandomGet<MascotData>(shopTask.amount, strategy));
            case ShopItemType.Stick:
                return ConcreteItemConverter<StickData>(randomSystem.RandomGet<StickData>(shopTask.amount, strategy));
            default:
                Debug.LogError($"无法生成商品: {shopTask.type}");
                return new List<IShopItem>();
        }
    }
    
    /// <summary>
    /// 将Data转换为IShopItem
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="datas"></param>
    /// <returns></returns>
    public static List<IShopItem> ConcreteItemConverter<T>(List<T> datas){
        List<IShopItem> shopItems = new List<IShopItem>();
        if (typeof(T) == typeof(FoodData)){
            //TODO: 现在是随机生成数量，之后可能要根据食材强度程度（稀有度）来生成数量
            int amount = 1;
            datas.ForEach(item => shopItems.Add(new FoodShopItem(item as FoodData, amount)));
        }
        else if (typeof(T) == typeof(MascotData)){
            datas.ForEach(item => shopItems.Add(new MascotShopItem(item as MascotData)));
        }
        else if (typeof(T) == typeof(StickData)){
            datas.ForEach(item => shopItems.Add(new StickShopItem(item as StickData)));
        }
        else{
            Debug.LogError($"无法转换 {typeof(T)} 为 IShopItem");
            return new List<IShopItem>();
        }
        return shopItems;
    }
}


#region 事件

#endregion