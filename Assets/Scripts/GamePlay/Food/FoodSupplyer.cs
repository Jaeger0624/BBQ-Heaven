using System.Collections.Generic;
using QFramework;
using UniRx;

public class FoodSupplyer : IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public IFoodSupplyStrategy supplyStrategy;
    public int initialSupplyAmount {get; private set;} // 初始补充数量
    public int supplyAmount {get; private set;} // 每次补充数量
    public ReactiveProperty<int> supplyChance {get; private set;} // 当前补充机会
    public int maxSupplyChance {get; private set;} // 最大补充次数

    public FoodSupplyer(IFoodSupplyStrategy supplyStrategy, int initialSupplyAmount, int supplyAmount, int maxSupplyCount){
        this.initialSupplyAmount = initialSupplyAmount;
        this.supplyAmount = supplyAmount;
        this.maxSupplyChance = maxSupplyCount;
        this.supplyChance = new ReactiveProperty<int>(0);

        SetSupplyStrategy(supplyStrategy);
    }

    public List<FoodInstance> SupplyFood(Dictionary<string, Food> foodRepositorys, bool UseTime){
        UpdateStrategy();
        if (supplyStrategy.initialSupplyCount <= 0) return new List<FoodInstance>();
        if (foodRepositorys.Count == 0) return new List<FoodInstance>();
        if (this.GetSystem<IBoardSystem>().GetEmptyCells().Count == 0) return new List<FoodInstance>();
        supplyChance.Value --;
        List<FoodInstance> newInstances = supplyStrategy.SupplyFood(foodRepositorys, false);

        // 耗时 
        if (UseTime){
            int supplyFoodTime = SettingManager.Instance.GameplaySettings.supplyFoodTime_默认;
            this.GetSystem<ITimeSystem>().PushTimePoint(supplyFoodTime);
        }
        // 播放动画序列
        this.GetSystem<IAnimationSystem>().Play();

        return newInstances;
    }

    public List<FoodInstance> SupplyFoodInit(Dictionary<string, Food> foodRepositorys){
        UpdateStrategy();
        if (supplyStrategy.initialSupplyCount <= 0) return new List<FoodInstance>();
        if (foodRepositorys.Count == 0) return new List<FoodInstance>();
        if (this.GetSystem<IBoardSystem>().GetEmptyCells().Count == 0) return new List<FoodInstance>();
        List<FoodInstance> newInstances = supplyStrategy.SupplyFood(foodRepositorys, true);
        // 播放动画序列
        this.GetSystem<IAnimationSystem>().Play();

        return newInstances;
    }

    private void UpdateStrategy(){
        supplyStrategy.SetSupplyCount(supplyAmount);
        supplyStrategy.SetInitialSupplyCount(initialSupplyAmount);
    }
    public void SetSupplyStrategy(IFoodSupplyStrategy supplyStrategy){
        this.supplyStrategy = supplyStrategy;
        UpdateStrategy();
    }
    public void SetInitialSupplyAmount(int initialSupplyAmount){
        this.initialSupplyAmount = initialSupplyAmount;
        UpdateStrategy();
    }
    public void SetSupplyAmount(int supplyAmount){
        this.supplyAmount = supplyAmount;
        UpdateStrategy();
    }
    public void SetMaxSupplyCount(int maxSupplyCount){
        this.maxSupplyChance = maxSupplyCount;
    }
    public void ResetSupplyCount(){
        this.supplyChance.Value = maxSupplyChance;
    }

    public static FoodSupplyer SingleSupplyer(int amount) => new FoodSupplyer(new FoodSupplyStrategy_随机指定数量(), amount, amount, 10);
}