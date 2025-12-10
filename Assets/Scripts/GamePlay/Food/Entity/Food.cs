using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using Sirenix.Serialization;
using UnityEngine;

/// <summary>
/// 食材仓库类
/// </summary>
public class Food : ICanGetSystem{
    [OdinSerialize]
    public string guid { get; private set; }
    public string name => foodData.Name;
    public bool isTemporary = false;
    public FoodData foodData => this.GetSystem<IDataSystem>().GetFoodData(foodDataId);
    [OdinSerialize]
    public string foodDataId { get; private set; }
    public FoodType foodType;
    public FoodTag foodTag;
    public Dictionary<FoodGAType, List<CGA>> foodGAs;
    public List<SustainEffect> sustainEffects => foodData.SEs;
    public Food(FoodData foodData, bool isTemporary = false){
        this.guid = Guid.NewGuid().ToString();
        this.isTemporary = isTemporary;
        this.foodDataId = foodData.ID;
        this.foodType = foodData.Type;
        this.foodTag = foodData.Tag;

        // 深拷贝foodGAs
        this.foodGAs = new Dictionary<FoodGAType, List<CGA>>();
        foreach (var cga in foodData.CGAs){
            if (!foodGAs.ContainsKey(cga.Type)){
                foodGAs.Add(cga.Type, new List<CGA>());
            }
            foodGAs[cga.Type].Add(new CGA(cga.Action));
        }
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

public class FoodPack{
    public string foodId;
    public int quantity;
    public FoodPack(string foodId, int quantity){
        this.foodId = foodId;
        this.quantity = quantity;
    }
}