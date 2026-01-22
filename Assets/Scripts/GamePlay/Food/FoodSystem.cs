using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UniRx;
using System.Linq;
public interface IFoodSystem : ISystem, ISavable{
    int RefreshFoodCost { get; set; }
    FoodPile FoodPile { get; }
    // 食材卡牌相关
    (List<FoodCard> cards, List<FoodInstance> instances) DrawFoodCard(int count);
    void RefreshFoodPile();
    // 获取食材实例
    FoodInstance GetFoodInstance(string guid);
    FoodInstance GetFoodInstance(Vector2Int position);
    Dictionary<string, FoodInstance> GetFoodInstances();
    Dictionary<string, FoodInstance> GetFoodInstancesByState(FoodInstanceState state);
    // 获取食材仓库
    Dictionary<string, FoodCard> FoodRepositorys();
    void AddFoodToRepository(List<FoodPack> foodPacks, bool isTemporary = false);
    void AddFoodToRepository(List<FoodCard> foodCards, bool isTemporary = false);
    void DeleteFoodFromRepository(FoodCard food);
    // 创建食材实例
    FoodInstance CreateFoodInstance(Vector2Int position, FoodCard food);
    void RemoveFoodInstance(string guid);
    void PutFoodInstanceToStick(string guid);
    // 补充食物
    Dictionary<string, int> GetFoodRepositoryAmounts();
    Dictionary<string, int> GetFoodRepositoryDict();
}


/// <summary>
/// 食材系统 - 系统层
/// </summary>
public partial class FoodSystem : AbstractSystem, IFoodSystem
{
    public int RefreshFoodCost { get; set; } = 5;
    private Dictionary<string, FoodCard> foodRepositorys = new Dictionary<string, FoodCard>();  // 食材仓库字典
    public Dictionary<string, FoodCard> FoodRepositorys() => foodRepositorys;
    private Dictionary<string, FoodInstance> FoodInstances = new Dictionary<string, FoodInstance>();

    // 当前剩余补充机会
    public FoodPile foodPile = new FoodPile(new List<FoodCard>());
    public FoodPile FoodPile => foodPile;
    protected override void OnInit()
    {
        if (foodRepositorys == null) foodRepositorys = new Dictionary<string, FoodCard>();

        FoodInstances = new Dictionary<string, FoodInstance>();
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<EndDayEvent>(OnEndDay);

        this.RegisterEvent<FinishCombineBBQEvent>(OnFinishCombineBBQ);
    }
    protected override void OnDeinit()
    {
        FoodInstances.Clear();
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<EndDayEvent>(OnEndDay);

        this.UnRegisterEvent<FinishCombineBBQEvent>(OnFinishCombineBBQ);
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.foodRepositorys = foodRepositorys;
    }

    public void Load(GameArchive archive)
    {
        // ✅ 修复 1：增加判空保护
        // 只有当存档里真的有数据时才加载，否则保持 OnInit 中的默认空字典
        if (archive != null && 
            archive.playerInfoData != null && 
            archive.playerInfoData.foodRepositorys != null)
        {
            foodRepositorys = archive.playerInfoData.foodRepositorys
                .ToDictionary(food => food.Key, food => food.Value);
        }
        else
        {
            // 如果是空存档，确保它是 new 出来的，而不是 null
            if (foodRepositorys == null)
            {
                foodRepositorys = new Dictionary<string, FoodCard>();
            }
        }
    }
    private void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        Debug.Log("【FoodSystem】开始新一天：食材仓库数量：" + foodRepositorys.Count);
        // 初始化食材仓库
        foodPile.Reset(foodRepositorys.Values.ToList());
        foodPile.Init();

        // 创建一个新的食材实例字典
        FoodInstances = new Dictionary<string, FoodInstance>();
        DrawFoodCard(10);
    }
    private void OnEndDay(EndDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        // 清除所有食材实例
        ClearAllFoodFromBoard();

        // 发送更新事件，通知视图更新
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }
    private void OnFinishCombineBBQ(FinishCombineBBQEvent evt)
    {
        // 完成烤串后，补充2张食材卡牌
        DrawFoodCard(2);
    }
    public (List<FoodCard> cards, List<FoodInstance> instances) DrawFoodCard(int count){
        // 1. FoodPile 食材卡的交互逻辑
        List<FoodCard> foodCards = foodPile.DrawFoodCard(count);

        List<FoodInstance> foodInstances = new List<FoodInstance>();
        foreach (var foodCard in foodCards){
            List<FoodInstance> newFoodInstances = foodCard.SpawnInstances();
            foreach (var foodInstance in newFoodInstances){
                if (foodInstance == null) {Debug.LogError($"【FoodSystem】创建食材实例失败: {foodCard.guid} 为空"); continue;}
                FoodInstance newFoodInstance = CreateFoodInstanceRandomPos(foodInstance);
                if (newFoodInstance == null) {Debug.LogError($"【FoodSystem】创建食材实例失败: {foodCard.guid} 为空"); continue;}
                foodInstances.Add(newFoodInstance);
            }
        }
        return (foodCards, foodInstances);
    }
    public void RefreshFoodPile(){
        if (foodPile.JustRefreshed) {Debug.LogWarning("【FoodSystem】食材堆已经刷新过，不能重复刷新"); return;}

        foodPile.RefreshFoodPile();
    }
    public Dictionary<string, FoodInstance> GetFoodInstances(){
        // 只查询，不修改
        Dictionary<string, FoodInstance> foodInstancesCopy = new Dictionary<string, FoodInstance>();
        foreach (var foodInstance in FoodInstances){
            foodInstancesCopy.Add(foodInstance.Key, foodInstance.Value);
        }
        return foodInstancesCopy;
    }
    public Dictionary<string, FoodInstance> GetFoodInstancesByState(FoodInstanceState state){
        Dictionary<string, FoodInstance> foodInstancesCopy = new Dictionary<string, FoodInstance>();
        foreach (var foodInstance in FoodInstances){
            if (foodInstance.Value.state == state){
                foodInstancesCopy.Add(foodInstance.Key, foodInstance.Value);
            }
        }
        return foodInstancesCopy;
    }

    public FoodInstance GetFoodInstance(string guid){
        if (!FoodInstances.TryGetValue(guid, out FoodInstance foodInstance)) {Debug.LogError($"【FoodSystem】获取食材实例失败: {guid} 不存在"); return null;}
        return foodInstance;
    }
    public FoodInstance GetFoodInstance(Vector2Int position){
        if (!FoodInstances.Values.Any(foodInstance => foodInstance.position == position)) {Debug.LogError($"【FoodSystem】获取食材实例失败: {position} 不存在"); return null;}
        return FoodInstances.Values.First(foodInstance => foodInstance.position == position);
    }

    public FoodInstance CreateFoodInstance(Vector2Int position, FoodCard food){
        // 1. 创建食材实例
        FoodInstance foodInstance = new FoodInstance(food, position);
        return CreateFoodInstance(foodInstance, position);
    }
    private FoodInstance CreateFoodInstanceRandomPos(FoodInstance foodInstance) => CreateFoodInstance(foodInstance, GetEmptyCell());
    // 底层实现
    private FoodInstance CreateFoodInstance(FoodInstance foodInstance, Vector2Int position){
        if (foodInstance == null) {Debug.LogError($"【FoodSystem】创建食材实例失败: foodInstance 为空"); return null;}
        if (FoodInstances == null) {Debug.LogError($"【FoodSystem】创建食材实例失败: FoodInstances 为空"); return null;}
        if (position == new Vector2Int(-1, -1)) {Debug.LogError($"【FoodSystem】创建食材实例失败: position 为空"); return null;}
        if (FoodInstances.ContainsKey(foodInstance.guid)) {Debug.LogError($"【FoodSystem】创建食材实例失败: {foodInstance.guid} 已存在"); return null;}
        foodInstance.SetState(FoodInstanceState.棋盘上);
        foodInstance.position = position;
        // 2. 设置位置
        this.GetSystem<IBoardSystem>().SetCellInstance(foodInstance.position, foodInstance.guid);
        // 3. 添加到食材实例列表
        FoodInstances.Add(foodInstance.guid, foodInstance);
        // 4. 注册到棋盘实体系统
        this.GetSystem<IBoardEntitySystem>().RegisterEntity(foodInstance, foodInstance.position);

        this.SendEvent(new CreateFoodInstanceEvent(foodInstance));
        Dictionary<string, int> foodRepositoryAmounts = this.GetSystem<IFoodSystem>().GetFoodRepositoryAmounts();
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(foodRepositoryAmounts));
        return foodInstance;
    }
    public void RemoveFoodInstance(string guid){
        if (!FoodInstances.TryGetValue(guid, out FoodInstance foodInstance)){
            Debug.LogError($"【FoodPile】移除食材实例失败: {guid} 不存在");
            return;
        }
        if (foodInstance.position != new Vector2Int(-1, -1) || foodInstance.state == FoodInstanceState.棋盘上){
            // 从棋盘上移除
            this.GetSystem<IBoardSystem>().SetCellInstance(foodInstance.position, null);
        }
        // 从棋盘实体系统中移除
        this.GetSystem<IBoardEntitySystem>().UnregisterEntity(foodInstance);

        // 发送移除食材实例事件
        this.SendEvent(new RemoveFoodInstanceEvent(foodInstance));

        FoodInstances.Remove(guid);
    }
    
    // 放上烤串
    // 只是将食材实例的position设置为(-1, -1)，不真正移除
    public void PutFoodInstanceToStick(string guid)
    {
        // 获取食材实例
        FoodInstance foodInstance = GetFoodInstance(guid);
        if (foodInstance == null) {Debug.LogError($"【FoodSystem】放上烤串失败: {guid} 不存在"); return;}
        // 从棋盘上移除
        this.GetSystem<IBoardSystem>().SetCellInstance(foodInstance.position, null);
        // 清除食材实例的position
        foodInstance.position = new Vector2Int(-1, -1);
        // 发送事件，通知视图更新
        this.SendEvent(new FoodRemoveFromBoardEvent(guid));
    }
    // 添加食材进构筑
    public void AddFoodToRepository(List<FoodPack> foodPacks, bool isTemporary = false)
    {

        foreach (FoodPack foodPack in foodPacks)
        {
            FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(foodPack.foodId);
            Debug.Log($"【FoodSystem】添加食材到仓库: {foodData.Name} - {foodPack.quantity} 个");
            for (int i = 0; i < foodPack.quantity; i++)
            {
                if (foodData == null) {Debug.LogError($"【FoodSystem】添加食材到仓库失败: {foodPack.foodId} 不存在"); continue;}
                FoodCard food = new FoodCard(foodData, isTemporary);
                foodRepositorys.Add(food.guid, food);

                foreach (var enhancement in foodPack.enhancements){
                    food.AddEnhancement(enhancement);
                }
            }
        }
        // 发送更新事件，通知视图更新
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }
    public void AddFoodToRepository(List<FoodCard> foodCards, bool isTemporary = false)
    {
        foreach (var foodCard in foodCards){
            if (foodCard == null) {Debug.LogError("AddFoodToRepository 的 foodCard 为空"); continue;}

            foodRepositorys.Add(foodCard.guid, foodCard);
        }
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }
    private void ClearAllFoodFromBoard(){
        List<string> foodInstanceGuids = new List<string>();
        foodInstanceGuids.AddRange(FoodInstances.Keys);
        foodInstanceGuids.ForEach(guid => RemoveFoodInstance(guid));
    }
    public Dictionary<string, int> GetFoodRepositoryAmounts(){
        if (foodPile == null) return new Dictionary<string, int>();
        return foodPile.DrawFoodPile.GroupBy(food => food.foodData.ID).ToDictionary(group => group.Key, group => group.Count());
    }
    public Dictionary<string, int> GetFoodRepositoryDict(){
        return foodRepositorys.Values.GroupBy(food => food.foodData.ID).ToDictionary(group => group.Key, group => group.Count());
    }

    public void DeleteFoodFromRepository(FoodCard food)
    {
        if (food == null) {Debug.LogError($"【FoodSystem】删除食材仓库失败: food 为空"); return;}
        if (!foodRepositorys.ContainsKey(food.guid)) {Debug.LogError($"【FoodSystem】删除食材仓库失败: {food.guid} - {food.foodData.Name} 不存在"); return;}
        Debug.Log($"【FoodSystem】删除食材仓库: {food.guid} - {food.foodData.Name}");
        foodRepositorys.Remove(food.guid);
        this.SendEvent(new DeleteFoodFromRepositoryEvent(food.guid));
        this.SendEvent(new UpdateFoodRepositoryAmountEvent(GetFoodRepositoryAmounts()));
    }

    private Vector2Int GetEmptyCell(){
        return this.GetSystem<IBoardSystem>().GetRandomEmptyCell().position;
    }
}
