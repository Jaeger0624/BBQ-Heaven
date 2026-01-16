using QFramework;

public class TriggerFoodIndexEvent : AbstractEvent{
    public FoodIndexer foodIndexer;
    public TriggerFoodIndexEvent(FoodIndexer foodIndexer){
        this.foodIndexer = foodIndexer;
    }
}
public class ResetFoodIndexEvent : AbstractEvent{}