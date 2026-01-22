using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;

public class FoodDisplayTask_随机获取若干 : IDisplayTask<FoodUIContext, DisplayFoodView>{
    private IItemInteractStrategy<FoodUIContext, DisplayFoodView> _strategy;
    private int _amount;
    public FoodDisplayTask_随机获取若干(IItemInteractStrategy<FoodUIContext, DisplayFoodView> strategy, int amount){
        _strategy = strategy;
        _amount = amount;
    }
    public IItemInteractStrategy<FoodUIContext, DisplayFoodView> GetStrategy() => _strategy;
    public IEnumerable<FoodUIContext> GetList(){
        IArchitecture architecture = GameArchitecture.Interface;
        Rng rng = architecture.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();

        List<FoodUIContext> foodUIContexts = rng.PickMany<FoodData>(architecture.GetSystem<IDataSystem>().GetAllFoodData(), _amount)
            .Select(x => new FoodUIContext(new FoodCard(x)))
            .ToList();

        foreach (var foodUIContext in foodUIContexts){
            foodUIContext.SetPrice(rng.NextInt(4,6));
            foodUIContext.SetAmount(1);
        }
        return foodUIContexts;
    }
}
