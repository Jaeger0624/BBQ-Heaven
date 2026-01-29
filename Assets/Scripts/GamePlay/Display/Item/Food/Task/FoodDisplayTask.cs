using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public class FoodDisplayTask_随机获取若干 : IDisplayTask<FoodUIContext, DisplayFoodView>{
    private IItemInteractStrategy<FoodUIContext, DisplayFoodView> _strategy;
    private int _amount;
    private bool _isMultiple;
    private bool _isDiscount;
    private bool _hasEnhancement;
    public FoodDisplayTask_随机获取若干(IItemInteractStrategy<FoodUIContext, DisplayFoodView> strategy, int amount, bool isMultiple, bool isDiscount, bool hasEnhancement){
        _strategy = strategy;
        _amount = amount;
        _isMultiple = isMultiple;
        _isDiscount = isDiscount;
        _hasEnhancement = hasEnhancement;
    }
    public IItemInteractStrategy<FoodUIContext, DisplayFoodView> GetStrategy() => _strategy;
    public IEnumerable<FoodUIContext> GetList(){
        IArchitecture architecture = GameArchitecture.Interface;
        Rng rng = architecture.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();

        List<FoodCard> foodCards = rng.PickMany<FoodData>(architecture.GetSystem<IDataSystem>().GetAllFoodData(), _amount).Select(x => new FoodCard(x)).ToList();

        // 1. 添加强化效果
        if (_hasEnhancement){
            foreach (var foodCard in foodCards){
                foodCard.AddEnhancement(FoodCardEnhancement.GetRandomEnhancement(rng));
            }
        }

        // 2. 生成FoodUIContext
        List<FoodUIContext> foodUIContexts = foodCards.Select(x => new FoodUIContext(x)).ToList();

        foreach (var foodUIContext in foodUIContexts){
            // 3. 设置价格
            PriceContext priceContext = new PriceContext(rng.NextInt(4,6));
            if (_isDiscount){
                priceContext.SetDiscount(rng.NextInt(2, 4));
            }
            foodUIContext.SetPrice(priceContext);

            // 4. 设置数量
            if (_isMultiple){
                foodUIContext.SetAmount(rng.NextInt(2, 5));
            }else{
                foodUIContext.SetAmount(1);
            }
        }
        return foodUIContexts;
    }
    // 集中生成商店商品的Task
    public static IDisplayTask<FoodUIContext, DisplayFoodView> Build(ShopType shopType){
        switch (shopType){
            case ShopType.普通:
                return new FoodDisplayTask_随机获取若干(
                    strategy: new BuyFoodStrategy(false),
                    amount: 3,
                    isDiscount: false,
                    isMultiple: false,
                    hasEnhancement: false
                );
            case ShopType.批发:
                return new FoodDisplayTask_随机获取若干(
                    strategy: new BuyFoodStrategy(true),
                    amount: 3,
                    isDiscount: false,
                    isMultiple: true,
                    hasEnhancement: false);
            case ShopType.折扣:
                return new FoodDisplayTask_随机获取若干(
                    strategy: new BuyFoodStrategy(false),
                    amount: 3,
                    isDiscount: true,
                    isMultiple: false,
                    hasEnhancement: false);
            case ShopType.强化:
                return new FoodDisplayTask_随机获取若干(
                    strategy: new BuyFoodStrategy(false),
                    amount: 3,
                    isDiscount: false,
                    isMultiple: false,
                    hasEnhancement: true);
            default:
                Debug.LogError("不支持的商店类型：" + shopType);
                return null;
        }
    }
}