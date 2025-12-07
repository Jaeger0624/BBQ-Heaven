using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IRngSystem : ISystem{
    void SetSeed(int seed);
    Rng GetSubRng<T>() where T : ISystem;
}


public class RngSystem : AbstractSystem, IRngSystem{
    public Rng MainRng { get; private set; }
    private Dictionary<Type, Rng> subRngs = new Dictionary<Type, Rng>();
    protected override void OnInit()
    {   
        MainRng = new Rng(0);
    }
    public void SetSeed(int seed)
    {
        Debug.Log($"【RngSystem】设置种子: {seed}");
        MainRng.SetSeed(seed);
        InitAllSubRngs();
    }
    // 基于子系统的类型，创建一个子Rng
    private void InitSubRng<T>() where T : ISystem{
        Rng subRng = new Rng(MainRng.NextInt(0, 1000000));
        subRngs.Add(typeof(T), subRng);
    }
    public Rng GetSubRng<T>() where T : ISystem{
        if (!subRngs.ContainsKey(typeof(T))){
            Debug.LogError($"【RngSystem】子Rng {typeof(T).Name} 不存在");
            return null;
        }
        return subRngs[typeof(T)];
    }

    private void InitAllSubRngs(){
        InitSubRng<IPCSystem>();
        InitSubRng<IStickSystem>();
        InitSubRng<IFoodSystem>();
        InitSubRng<IMascotSystem>();
        InitSubRng<IGASystem>();
        InitSubRng<IProcessSystem>();
        InitSubRng<ITimeSystem>();
        InitSubRng<IBoardSystem>();
        InitSubRng<IRngSystem>();
        InitSubRng<IShopSystem>();
        InitSubRng<IAnimationSystem>();
    }
}