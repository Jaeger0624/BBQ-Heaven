using System.Collections.Generic;
using System.Linq;
using QFramework;
using UniRx;

public class FoodSupplyEvent{
    public List<FoodInstance> newInstances;
    public FoodSupplyEvent(List<FoodInstance> newInstances){
        this.newInstances = newInstances;
    }
}

public interface IFoodSupplyStrategy : IController{
    int supplyCount {get; set;}
    int initialSupplyCount {get; set;}
    List<FoodInstance> SupplyFood(Dictionary<string, Food> foodRepositorys, bool isInit);
    void SetSupplyCount(int supplyCount);
    void SetInitialSupplyCount(int initialSupplyCount);
}
public abstract class FoodSupplyStrategyBase : IFoodSupplyStrategy
{
    public int supplyCount { get; set; }
    public int initialSupplyCount { get; set; }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    public void SetInitialSupplyCount(int initialSupplyCount) => this.initialSupplyCount = initialSupplyCount;

    public void SetSupplyCount(int supplyCount) => this.supplyCount = supplyCount;

    public abstract List<FoodInstance> SupplyFood(Dictionary<string, Food> foodRepositorys, bool isInit);
}

public class FoodSupplyStrategy_随机填满 : FoodSupplyStrategyBase
{

    public override List<FoodInstance> SupplyFood(Dictionary<string, Food> foodRepositorys, bool isInit){
        IFoodSystem foodSystem = this.GetSystem<IFoodSystem>();
        // 随机中仓库中获取一个食物并尝试将其加入地图中的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        List<FoodInstance> newInstances = new List<FoodInstance>();

        List<Food> foodList = foodRepositorys.Values.ToList();

        Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        while (foodList.Count > 0){
            // 如果空位为0，则退出循环
            if (emptyCells.Count == 0) break;

            // 随机获取一个空位
            BoardCell emptyCell = rng.PickOne(emptyCells);    
            
            // 随机获取一个食物
            Food food = rng.PickOne(foodList);
            foodList.Remove(food);
            
            // 创建食材实例
            FoodInstance foodInstance = foodSystem.CreateFoodInstance(emptyCell.position, food);
            emptyCells.Remove(emptyCell);
            newInstances.Add(foodInstance);
        }

        // 发送事件
        return newInstances;
    }
}

public class FoodSupplyStrategy_随机指定数量 : FoodSupplyStrategyBase
{
    public override List<FoodInstance> SupplyFood(Dictionary<string, Food> foodRepositorys, bool isInit){
        if (isInit){
            return SupplyFoodAmount(foodRepositorys, initialSupplyCount);
        }
        else{
            return SupplyFoodAmount(foodRepositorys, supplyCount);
        }
    }
            
    private List<FoodInstance> SupplyFoodAmount(Dictionary<string, Food> foodRepositorys, int amount){
        IFoodSystem foodSystem = this.GetSystem<IFoodSystem>();
        // 随机中仓库中获取指定数量的食物并尝试将其加入地图中的空位
        List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
        List<FoodInstance> newInstances = new List<FoodInstance>();

        List<Food> foodList = foodRepositorys.Values.ToList();
        Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>();
        int foodAmount = 0;
        while (foodList.Count > 0 && foodAmount < amount){
            // 如果空位为0，则退出循环
            if (emptyCells.Count == 0) break;
            // 随机获取一个空位
            BoardCell emptyCell = rng.PickOne(emptyCells);    
            
            // 随机获取一个食物
            Food food = rng.PickOne(foodList);
            foodList.Remove(food);
            
            // 创建食材实例
            FoodInstance foodInstance = foodSystem.CreateFoodInstance(emptyCell.position, food);
            newInstances.Add(foodInstance);
            emptyCells.Remove(emptyCell);
            foodAmount++;
        }
        return newInstances;
    }
}
