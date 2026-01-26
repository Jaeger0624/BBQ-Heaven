using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;

public abstract class IndexRuleBase : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract List<FoodInstance> GetFoodInstances();
}
public enum FoodProperty{
    Rarity,
    Taste,
}
/// <summary>
/// 获取前K个食材实例
/// </summary>
public class IndexRule_TopK : IndexRuleBase{
    public int k;
    public FoodProperty property;
    public IndexRule_TopK(int k, FoodProperty property){
        this.k = k;
        this.property = property;
    }
    public override List<FoodInstance> GetFoodInstances(){
        List<FoodInstance> foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstancesByState(FoodInstanceState.棋盘上).Values.ToList();
        foodInstances.Sort((x, y) => {
            if (property == FoodProperty.Rarity){
                return y.rarity.CompareTo(x.rarity);
            }
            else{
                return y.taste.CompareTo(x.taste);
            }
        });
        if (foodInstances.Count < k){
            return foodInstances;
        }
        int value = foodInstances[k-1].rarity;
        return foodInstances.Where(x => x.rarity >= value).ToList();
    }
}
public class IndexRule_Rank : IndexRuleBase{
    public Rank rank;
    public IndexRule_Rank(Rank rank){
        this.rank = rank;
    }
    public override List<FoodInstance> GetFoodInstances(){
        List<FoodInstance> foodInstances = this.GetSystem<IFoodSystem>().GetFoodInstancesByState(FoodInstanceState.棋盘上).Values.ToList();
        return foodInstances.Where(x => x.food.foodData.Rank == rank).ToList();
    }
}

public class IndexRule_FoodType : IndexRuleBase{
    public FoodType foodType;
    public IndexRule_FoodType(FoodType foodType){
        this.foodType = foodType;
    }
    public override List<FoodInstance> GetFoodInstances(){
        // 获取所有食材实例，并过滤出指定类型的食材实例
        return this.GetSystem<IFoodSystem>().GetFoodInstances().Values.ToList().Where(x => x.food.foodType == foodType).ToList();
    }
}