using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using UnityEngine;

/// <summary>
/// 食材仓库类
/// </summary>
public class Food {
    public string guid { get; private set; }
    public string name => foodData.Name;
    public bool isTemporary = false;
    public readonly FoodData foodData;
    public FoodType foodType;
    public FoodTag foodTag;
    public Dictionary<FoodGAType, List<CGA>> foodGAs;
    public List<SustainEffect> sustainEffects => foodData.SEs;
    public Food(FoodData foodData, bool isTemporary = false){
        this.foodData = foodData;
        this.guid = Guid.NewGuid().ToString();
        this.isTemporary = isTemporary;
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
}

public class FoodPack{
    public string foodId;
    public int quantity;
    public FoodPack(string foodId, int quantity){
        this.foodId = foodId;
        this.quantity = quantity;
    }
}