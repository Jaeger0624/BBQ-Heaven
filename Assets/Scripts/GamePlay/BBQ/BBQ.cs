using System;
using System.Collections.Generic;
using cfg;
using UniRx;

/// <summary>
/// 烧烤实例类 - 实例层
/// </summary>
public class BBQ{
    public string guid { get; private set; }
    public ReactiveProperty<int> totalRarity { get; private set; }
    public ReactiveProperty<int> totalTaste { get; private set; }
    public Stick stick;
    public List<FoodInstance> foodInstances;
    // 激活的配方ID列表
    public List<string> recipeIds;
    public BBQ(Stick stick, List<FoodInstance> foodInstances){
        this.guid = Guid.NewGuid().ToString();
        this.stick = stick;
        this.foodInstances = foodInstances;
        this.totalRarity = new ReactiveProperty<int>(0);
        this.totalTaste = new ReactiveProperty<int>(0);
    }

    /// <summary>
    /// 设置珍稀度
    /// </summary>
    /// <param name="totalRarity">珍稀度</param>
    public void SetTotalRarity(int totalRarity){
        this.totalRarity.Value = totalRarity;
    }
    /// <summary>
    /// 设置美味度
    /// </summary>
    /// <param name="totalTaste">美味度</param>
    public void SetTotalTaste(int totalTaste){
        this.totalTaste.Value = totalTaste;
    }

    public void AddExtraTimeCost(int extraTimeCost){
        this.stick.extraTimeCost += extraTimeCost;
    }


}