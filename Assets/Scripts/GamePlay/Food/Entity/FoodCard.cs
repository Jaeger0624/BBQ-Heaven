using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using cfg;
using QFramework;
using Sirenix.Serialization;
using UnityEngine;

/// <summary>
/// 食材仓库类
/// </summary>
[Serializable]
public class FoodCard : ICanGetSystem{
    [OdinSerialize]
    public string guid { get; private set; }
    public string name => foodData.Name;
    public bool isTemporary = false;
    [OdinSerialize]
    public string foodDataId { get; private set; }
    [OdinSerialize]
    public FoodType foodType;
    [OdinSerialize]
    public FoodTag foodTag;
    // 不序列化
    public FoodData foodData => this.GetSystem<IDataSystem>().GetFoodData(foodDataId);
    [NonSerialized]
    public Dictionary<FoodGAType, List<CGA>> foodGAs;
    public List<SustainEffect> sustainEffects => foodData.SEs;
    public int MaxSlots {get; private set;} = 2; // 默认最大槽位为2
    [OdinSerialize]
    public List<FoodCardEnhancement> Enhancements = new List<FoodCardEnhancement>();

    public FoodCard(FoodData foodData, bool isTemporary = false){
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

    public bool AddEnhancement(FoodCardEnhancement enhancement){
        if (Enhancements.Count >= MaxSlots) return false;
        Enhancements.Add(enhancement);
        return true;
    }

    public bool RemoveEnhancement(FoodCardEnhancement enhancement){
        if (!Enhancements.Contains(enhancement)) return false;
        Enhancements.Remove(enhancement);
        return true;
    }

    public List<FoodInstance> SpawnInstances()
    {
        List<FoodInstance> result = new List<FoodInstance>();

        
        // 1. 计算生成数量（应用强化效果）
        int spawnCount = 2;  // 初始生成数量

        if (Enhancements != null){
            foreach (var buff in Enhancements)
            {
                spawnCount = buff.ModifySpawnCount(spawnCount);
            }
        }
        else{
            Debug.LogError($"【FoodCard】{name} 强化效果栏为null");
            return result;
        }


        // 2. 循环生成
        for (int i = 0; i < spawnCount; i++)
        {
            FoodInstance foodInstance = new FoodInstance(this, new Vector2Int(-1, -1));

            if (Enhancements != null){
            // 应用实例级强化
                foreach (var buff in Enhancements)
                {
                    buff.OnInstanceCreated(foodInstance);
                }
            }
            else{
                Debug.LogError($"【FoodCard】{name} 强化效果栏为null");
            }

            result.Add(foodInstance);
        }
        return result;
    }

    [OnDeserialized]
    void OnDeserialized(StreamingContext context){
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