using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using UnityEngine;

public class FoodIndexController : MonoBehaviour, IController
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject foodIndexerPrefab;
    private Dictionary<FoodType, FoodIndexer> foodIndexers = new Dictionary<FoodType, FoodIndexer>();
    private Dictionary<FoodType, int> Counts = new Dictionary<FoodType, int>();
    private List<FoodInstance> triggeredFoodInstances = new List<FoodInstance>();
    void Start()
    {
        // 注册事件
        // 1. 监听食材创建事件
        this.RegisterEvent<CreateFoodInstanceEvent>(OnCreateFoodInstance).UnRegisterWhenGameObjectDestroyed(this);
        this.RegisterEvent<FoodRemoveFromBoardEvent>(OnFoodRemoveFromBoard).UnRegisterWhenGameObjectDestroyed(this);
        // 棋盘 Reset 时食材视图会被销毁，需要清掉触发缓存避免后续动画访问失效引用
        this.RegisterEvent<ResetBoardEvent>(OnResetBoard).UnRegisterWhenGameObjectDestroyed(this);

        // 2. 监听Indexer触发事件
        this.RegisterEvent<TriggerFoodIndexEvent>(TriggerIndexer).UnRegisterWhenGameObjectDestroyed(this);
        this.RegisterEvent<ResetFoodIndexEvent>(ResetIndexer).UnRegisterWhenGameObjectDestroyed(this);
    }
    private void OnCreateFoodInstance(CreateFoodInstanceEvent e)
    {
        FoodInstance foodInstance = e.foodInstance;
        // 数值更新
        if (Counts.TryGetValue(foodInstance.food.foodData.Type, out int count)){
            Counts[foodInstance.food.foodData.Type] = count + 1;
        }
        else{
            Counts.Add(foodInstance.food.foodData.Type, 1);
            CreateFoodIndexer(foodInstance.food.foodData.Type);
        }

        // 视觉更新
        if (foodIndexers.TryGetValue(foodInstance.food.foodData.Type, out FoodIndexer foodIndexer)){
            foodIndexer.UpdateVisual(Counts[foodInstance.food.foodData.Type]);
        }
    }
    private void OnFoodRemoveFromBoard(FoodRemoveFromBoardEvent e)
    {
        FoodInstance foodInstance = this.GetSystem<IFoodSystem>().GetFoodInstance(e.guid);

        if (Counts.TryGetValue(foodInstance.food.foodData.Type, out int count)){
            Counts[foodInstance.food.foodData.Type] = count - 1;
            if (foodIndexers.TryGetValue(foodInstance.food.foodData.Type, out FoodIndexer foodIndexer)){
                foodIndexer.UpdateVisual(Counts[foodInstance.food.foodData.Type]);
            }
        }
        else{
            Debug.LogError($"【FoodIndexController】食材索引计算存在错误: {foodInstance.food.foodData.Type} 类型食材数量为0，但尝试减少");
            return;
        }
        if (Counts[foodInstance.food.foodData.Type] == 0){
            Counts.Remove(foodInstance.food.foodData.Type);
            RemoveFoodIndexer(foodInstance.food.foodData.Type);
        }
    }
    // 创建食材索引器
    private void CreateFoodIndexer(FoodType foodType){
        FoodIndexer foodIndexer = Instantiate(foodIndexerPrefab, content).GetComponent<FoodIndexer>();
        foodIndexer.Init(foodType);
        foodIndexers.Add(foodType, foodIndexer);
    }

    // 移除食材索引器
    private void RemoveFoodIndexer(FoodType foodType){
        if (foodIndexers.TryGetValue(foodType, out FoodIndexer foodIndexer)){
            Destroy(foodIndexer.gameObject);
            foodIndexers.Remove(foodType);
        }
        else{
            Debug.LogError($"【FoodIndexController】食材索引器不存在: {foodType}");
            return;
        }
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    private void TriggerIndexer(TriggerFoodIndexEvent e){
        triggeredFoodInstances.AddRange(e.foodIndexer.GetFoodInstances());
        foreach (var foodInstance in triggeredFoodInstances){
            foodInstance.foodInstanceView.GO().transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(foodInstance.foodInstanceView.GO());
        }
    }
    private void ResetIndexer(ResetFoodIndexEvent e){
        foreach (var foodInstance in triggeredFoodInstances){
            foodInstance.foodInstanceView.GO().transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(foodInstance.foodInstanceView.GO());
        }
        triggeredFoodInstances.Clear();
    }

    private void OnResetBoard(ResetBoardEvent evt)
    {
        triggeredFoodInstances.Clear();
    }
}
