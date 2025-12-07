using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public class FoodRepositoryController : MonoBehaviour, IController{

    [SerializeField] private Transform content;
    [SerializeField] private GameObject foodRepositoryViewPrefab;

    // ID -> FoodRepositoryView
    public Dictionary<string, int> foodRepositoryAmounts = new Dictionary<string, int>();
    public Dictionary<string, FoodRepositoryView> foodRepositoryViews = new Dictionary<string, FoodRepositoryView>();
    void OnEnable()
    {
        this.RegisterEvent<UpdateFoodRepositoryAmountEvent>(OnUpdateFoodRepositoryAmount).UnRegisterWhenDisabled(this);
        // 初始化时立即更新视图
        InitializeRepositoryView();
    }
    
    private void InitializeRepositoryView(){
        IFoodSystem foodSystem = this.GetSystem<IFoodSystem>();
        if (foodSystem != null){
            Dictionary<string, int> amounts = foodSystem.GetFoodRepositoryAmounts();
            OnUpdateFoodRepositoryAmount(new UpdateFoodRepositoryAmountEvent(amounts));
        }
    }
    
    private void OnUpdateFoodRepositoryAmount(UpdateFoodRepositoryAmountEvent evt){
        List<string> newFoodIds = evt.foodRepositoryAmounts.Keys.Where(foodId => !foodRepositoryViews.ContainsKey(foodId)).ToList();

        foreach (var foodId in newFoodIds){
            CreateFoodRepositoryView(foodId, evt.foodRepositoryAmounts[foodId]);
        }
        
        // 遍历当前存在的视图，更新或删除
        List<string> existingFoodIds = foodRepositoryViews.Keys.ToList();
        foreach (var foodId in existingFoodIds){
            if (evt.foodRepositoryAmounts.TryGetValue(foodId, out int amount)){
                if (foodRepositoryViews.ContainsKey(foodId)){
                    foodRepositoryViews[foodId].UpdateVisual(amount);
                    foodRepositoryAmounts[foodId] = amount;
                }
            }
            else{
                DestroyFoodRepositoryView(foodId);
            }
        }
    }
    public void CreateFoodRepositoryView(string foodId, int amount){
        FoodRepositoryView foodRepositoryView = GameObject.Instantiate(foodRepositoryViewPrefab, content).GetComponent<FoodRepositoryView>();
        foodRepositoryViews.Add(foodId, foodRepositoryView);
        foodRepositoryAmounts[foodId] = amount;
        foodRepositoryView.Init(foodId, amount);
    }
    public void DestroyFoodRepositoryView(string foodId){
        if (!foodRepositoryViews.TryGetValue(foodId, out FoodRepositoryView foodRepositoryView)){
            return;
        }
        Destroy(foodRepositoryView.gameObject);
        foodRepositoryViews.Remove(foodId);
        foodRepositoryAmounts.Remove(foodId);
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}