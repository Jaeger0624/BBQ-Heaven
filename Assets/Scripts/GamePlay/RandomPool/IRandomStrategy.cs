using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface IRandomStrategy{
    List<T> Random<T>(int amount) where T : class;


}
public abstract class AbstractRandomStrategy : IRandomStrategy, ICanGetSystem{
    public abstract List<T> Random<T>(int amount) where T : class;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

public class Random_纯随机 : AbstractRandomStrategy{
    public override List<T> Random<T>(int amount){
        IDataSystem dataSystem = this.GetSystem<IDataSystem>();
        List<T> result = new List<T>();
        Type type;
        if (typeof(T) == typeof(FoodData)){
            result = dataSystem.GetAllFoodData().Select(item => item as T).ToList();
            type = typeof(IFoodSystem);
        }
        else if (typeof(T) == typeof(MascotData)){
            result = dataSystem.GetAllMascotData().Select(item => item as T).ToList();
            type = typeof(IMascotSystem);
        }
        else if (typeof(T) == typeof(StickData)){
            result = dataSystem.GetAllStickData().Select(item => item as T).ToList();
            type = typeof(IStickSystem);
        }
        if (result.Count == 0) return new List<T>();
        amount = Mathf.Min(amount, result.Count);
        List<T> randomResult = result.RandomSelect(amount);
        return randomResult;
    }
}


public class Random_考虑Build权重 : AbstractRandomStrategy{
    public override List<T> Random<T>(int amount){
        return new List<T>();
    }

}