using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UniRx;
using System.Linq;
public interface IFoodSystem : ISystem{
    FoodSupplyer foodSupplyer {get; set;}  // 承担补充食物的职责
    FoodInstanceMover foodInstanceMover {get; set;}  // 承担移动食材实例的职责
    FoodInstance GetFoodInstance(string guid);
    FoodInstance GetFoodInstance(Vector2Int position);
    Dictionary<string, FoodInstance> GetFoodInstances();
    // 获取食材仓库
    Dictionary<string, Food> FoodInRepositorys();
    Dictionary<string, Food> FoodRepositorys();
    // 创建食材实例
    FoodInstance CreateFoodInstance(Vector2Int position, Food food);
    void RemoveFoodInstance(string guid);
    void PutFoodInstanceToStick(string guid);
    void AddFoodToRepository(List<FoodPack> foodPacks, bool isTemporary = false);
    // 补充食物
    List<FoodInstance> SupplyFood(FoodSupplyer foodSupplyer, bool UseTime);
    ReactiveProperty<int> GetCurrentSupplyCount();
    Dictionary<string, int> GetFoodRepositoryAmounts();
    Dictionary<string, int> GetFoodRepositoryDict();
}


/// <summary>
/// 食材系统 - 系统层
/// </summary>
public class FoodSystem : AbstractSystem, IFoodSystem
{
    public Dictionary<string, Food> foodRepositorys;  // 食材仓库字典
    public Dictionary<string, Food> FoodRepositorys() => foodRepositorys;
    // ID -> 食材数量
    public Dictionary<string, int> totalRespository;  // 食材仓库食材数量字典
    // 当前剩余补充机会
    public FoodSupplyer foodSupplyer {get; set;}
    public FoodInstanceMover foodInstanceMover {get; set;}
    public FoodPile foodPile;
    protected override void OnInit()
    {
        foodRepositorys = new Dictionary<string, Food>();
        totalRespository = new Dictionary<string, int>();
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<EndDayEvent>(OnEndDay);

        foodSupplyer = new FoodSupplyer(new FoodSupplyStrategy_随机指定数量(), 40, 20, 10);
        foodInstanceMover = new FoodInstanceMover();

        foodPile = null;
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<EndDayEvent>(OnEndDay);

        foodPile = null;
    }
    private void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        // 初始化食材仓库
        foodPile = new FoodPile(foodRepositorys.Values.ToList());
        foodPile.Init();

        // 补充食物
        SupplyFoodInit(foodSupplyer);

        // 重置补充机会
        ResetSupplyCount();
    }
    private void OnEndDay(EndDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        // 清除所有食材实例
        ClearAllFoodFromBoard();

        // 发送更新事件，通知视图更新
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }

    public Dictionary<string, FoodInstance> GetFoodInstances(){
        // 只查询，不修改
        Dictionary<string, FoodInstance> foodInstancesCopy = new Dictionary<string, FoodInstance>();
        foreach (var foodInstance in foodPile.FoodInstances){
            foodInstancesCopy.Add(foodInstance.Key, foodInstance.Value);
        }
        return foodInstancesCopy;
    }

    public FoodInstance GetFoodInstance(string guid){
        return foodPile.GetFoodInstance(guid);
    }
    public FoodInstance GetFoodInstance(Vector2Int position){
        return foodPile.GetFoodInstance(position);
    }

    public Dictionary<string, Food> FoodInRepositorys() => foodRepositorys.Values.ToDictionary(food => food.guid, food => food);
    public FoodInstance CreateFoodInstance(Vector2Int position, Food food) => foodPile.CreateFoodInstance(food, position);
    /// <summary>
    /// 彻底移除食材实例
    /// </summary>
    /// <param name="guid"></param>
    public void RemoveFoodInstance(string guid) => foodPile.RemoveFoodInstance(guid);
    // 只是将食材实例的position设置为(-1, -1)，不真正移除
    public void PutFoodInstanceToStick(string guid)
    {
        // 获取食材实例
        FoodInstance foodInstance = foodPile.GetFoodInstance(guid);
        if (foodInstance == null) return;
        // 从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(foodInstance.position, null);
        // 清除食材实例的position
        foodInstance.position = new Vector2Int(-1, -1);
    }
    // 添加食材进构筑
    public void AddFoodToRepository(List<FoodPack> foodPacks, bool isTemporary = false)
    {

        foreach (FoodPack foodPack in foodPacks)
        {
            FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(foodPack.foodId);
            // Debug.Log($"【FoodSystem】添加食材到仓库: {foodData.Name} - {foodPack.quantity} 个");
            for (int i = 0; i < foodPack.quantity; i++)
            {
                Food food = new Food(foodData, isTemporary);
                foodRepositorys.Add(food.guid, food);
            }

            if (totalRespository.TryGetValue(foodPack.foodId, out int amount)){
                totalRespository[foodPack.foodId] = amount + foodPack.quantity;
            }
            else{
                totalRespository.Add(foodPack.foodId, foodPack.quantity);
                this.SendEvent(new AddNewFoodToRepositoryEvent(foodPack.foodId, foodPack.quantity));
            }
        }
        // 发送更新事件，通知视图更新
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }
    private void DeleteFoodFromTotalRepository(string guid, int amount){
        Food food = foodRepositorys[guid];
        foodRepositorys.Remove(guid);
        if (totalRespository.TryGetValue(food.foodData.ID, out int count)){
            if (count - amount <= 0){
                totalRespository.Remove(food.foodData.ID);
                this.SendEvent(new DeleteFoodFromRepositoryEvent(food.foodData.ID));
            }
            else{
                totalRespository[food.foodData.ID] = count - amount;
            }
        }
        
        // 发送更新事件，通知视图更新
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }
    private List<FoodInstance> SupplyFoodInit(FoodSupplyer foodSupplyer){
        List<FoodInstance> foodInstances = foodSupplyer.SupplyFoodInit(foodPile.DrawFoodPile.ToDictionary(food => food.guid, food => food));
        ExcecuteFoodInstancesOnBoard(foodInstances);
        return foodInstances;
    }
    // 补充食物
    public List<FoodInstance> SupplyFood(FoodSupplyer foodSupplyer, bool UseTime){
        List<FoodInstance> foodInstances = foodSupplyer.SupplyFood(foodPile.DrawFoodPile.ToDictionary(food => food.guid, food => food), UseTime);
        ExcecuteFoodInstancesOnBoard(foodInstances);
        return foodInstances;
    }
    private void ExcecuteFoodInstancesOnBoard(List<FoodInstance> foodInstances){
        foreach (var foodInstance in foodInstances){
            if (!foodInstance.food.foodGAs.ContainsKey(FoodGAType.放上棋盘时)) continue;
            foreach (var cga in foodInstance.food.foodGAs[FoodGAType.放上棋盘时]){
                this.GetSystem<IGASystem>().ApplyCGA(foodInstance, cga, null);
            }
        }
    }
    private void ResetSupplyCount() => foodSupplyer.ResetSupplyCount();
    private void ClearAllFoodFromBoard(){
        List<string> foodInstanceGuids = new List<string>();
        foodInstanceGuids.AddRange(foodPile.FoodInstances.Keys);
        foodInstanceGuids.ForEach(guid => RemoveFoodInstance(guid));
    }
    public Dictionary<string, int> GetFoodRepositoryAmounts(){
        if (foodPile == null) return new Dictionary<string, int>();
        return foodPile.DrawFoodPile.GroupBy(food => food.foodData.ID).ToDictionary(group => group.Key, group => group.Count());
    }
    public Dictionary<string, int> GetFoodRepositoryDict(){
        return foodRepositorys.Values.GroupBy(food => food.foodData.ID).ToDictionary(group => group.Key, group => group.Count());
    }
    public ReactiveProperty<int> GetCurrentSupplyCount() => foodSupplyer.supplyChance;

}

#region 食材系统事件

public class CreateFoodInstanceEvent{
    public FoodInstance foodInstance;
    public CreateFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

public class RemoveFoodInstanceEvent{
    public FoodInstance foodInstance;
    public RemoveFoodInstanceEvent(FoodInstance foodInstance){
        this.foodInstance = foodInstance;
    }
}

public class AddNewFoodToRepositoryEvent{
    public readonly string foodId;
    public readonly int amount;
    public AddNewFoodToRepositoryEvent(string foodId, int amount){
        this.foodId = foodId;
        this.amount = amount;
    }
}

public class DeleteFoodFromRepositoryEvent{
    public readonly string id;
    public DeleteFoodFromRepositoryEvent(string id){
        this.id = id;
    }
}


public class UpdateFoodRepositoryAmountEvent{
    public readonly Dictionary<string, int> foodRepositoryAmounts;
    public UpdateFoodRepositoryAmountEvent(Dictionary<string, int> foodRepositoryAmounts){
        this.foodRepositoryAmounts = foodRepositoryAmounts;
    }
}


public class MoveFoodInstanceEvent{
    public readonly Vector2Int targetPosition;
    public readonly Vector2Int originPosition;
    public readonly string guid;
    public MoveFoodInstanceEvent(Vector2Int targetPosition, Vector2Int originPosition, string guid){
        this.targetPosition = targetPosition;
        this.originPosition = originPosition;
        this.guid = guid;
    }
}
#endregion

