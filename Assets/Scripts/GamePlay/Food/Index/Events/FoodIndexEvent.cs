using System.Collections.Generic;
using QFramework;

public interface IFoodIndexer{
    List<FoodInstance> GetFoodInstances();
}
public class TriggerFoodIndexEvent : AbstractEvent{
    public IFoodIndexer foodIndexer;
    public TriggerFoodIndexEvent(IFoodIndexer foodIndexer){
        this.foodIndexer = foodIndexer;
    }
}
public class ResetFoodIndexEvent : AbstractEvent{}