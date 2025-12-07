using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public static class UtilityExtentions{
    public static List<T> RandomSelect<T>(this List<T> list, int amount){
        if (list.Count == 0) return null;
        if (amount > list.Count) amount = list.Count;
        return list.OrderBy(x => Random.Range(0, list.Count)).Take(amount).ToList();
    }

    public static List<T> RandomSelect_Rng<T, TSystem>(this List<T> list, int amount) where TSystem : ISystem{
        IRngSystem rngSystem = GameArchitecture.Interface.GetSystem<IRngSystem>();
        Rng subRng = rngSystem.GetSubRng<TSystem>();
        if (subRng == null) return null;
        return subRng.PickMany(list, amount);
    }

    public static List<T> RandomSelectWithWeight<T>(this List<(T item, float weight)> list, int amount){
        if (list.Count == 0) return null;
        if (amount > list.Count) amount = list.Count;
        // 需要根据权重，计算出每个元素的权重范围，然后随机一个权重，然后根据权重范围，选择对应的元素
        List<T> result = new List<T>();
        for (int i = 0; i < amount; i++)
        {
            // 随机获取一个元素
            T item = RandomSelectWithWeightSingle(list);

            // 如果获取到的元素为空，则退出循环
            if (item == null) break;
            // 添加到结果列表
            result.Add(item);
            // 移除已经获取的元素
            list.RemoveAll(x => x.item.Equals(item));
        }
        return result.ToList();
    }

    public static T RandomSelectWithWeightSingle<T>(this List<(T item, float weight)> list){
        if (list.Count == 0) return default(T);
    
        float totalWeight = 0;
        for (int i = 0; i < list.Count; i++)
        {
            totalWeight += list[i].weight;
            list[i] = (list[i].item, totalWeight);
        }
        float randomWeight = UnityEngine.Random.value * totalWeight;
        return list.FirstOrDefault(x => x.weight >= randomWeight).item;
    }



    public static float Remap(this float value, float from1, float to1, float from2, float to2){
        return Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, value));
    }
}