using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IRngSystem : ISystem, ISavable{
    void SetMainSeed(int seed);
    Rng GetSubRng<T>() where T : ISystem;
}


public class RngSystem : AbstractSystem, IRngSystem{
    private int _globalSeed;
    public Rng MainRng { get; private set; }
    private Dictionary<string, Rng> _subRngs = new Dictionary<string, Rng>();
    protected override void OnInit()
    {   
        _globalSeed = (int)DateTime.Now.Ticks;
    }

    protected override void OnDeinit()
    {
        _globalSeed = (int)DateTime.Now.Ticks;
    }

    public void Save(GameArchive archive)
    {
        RngSaveData rngSaveData = new RngSaveData();
        rngSaveData.MainSeed = _globalSeed;
        rngSaveData.SubRngCallCounts = new Dictionary<string, int>();

        foreach (var subRng in _subRngs)
        {
            rngSaveData.SubRngCallCounts.Add(subRng.Key, subRng.Value.CallCount);
        }
        archive.rngSaveData = rngSaveData;
    }
    public void Load(GameArchive archive)
    {
        SetMainSeed(archive.rngSaveData.MainSeed);

        foreach (var subRng in archive.rngSaveData.SubRngCallCounts)
        {
            string systemName = subRng.Key;
            int savedCallCount = subRng.Value;

            // 重新计算子种子
            int subSeed = GenerateDeterministicSeed(_globalSeed, systemName);
            Rng rng = new Rng(subSeed);

            // 快进到存档时的调用次数
            rng.RestoreState(savedCallCount);
            _subRngs[systemName] = rng;
        }
    }
    public void SetMainSeed(int seed)
    {
        Debug.Log($"【RngSystem】全局重置，新种子: {seed}");
        _globalSeed = seed;
        _subRngs.Clear(); // 清除旧的实例，下次获取时重新生成
    }
    public Rng GetSubRng<T>() where T : ISystem{
        string key = typeof(T).Name;


        if (!_subRngs.ContainsKey(key)){
            // 【核心改进】使用 哈希算法 替代 MainRng 生成子种子。
            // 这样做的好处是：T系统的种子只取决于 GlobalSeed 和 T的名字。
            // 无论系统初始化的顺序如何，只要 GlobalSeed 没变，IGASystem 拿到的种子永远是一样的。
            int subSeed = GenerateDeterministicSeed(_globalSeed, key);
            
            _subRngs[key] = new Rng(subSeed);
            // Debug.Log($"Created Rng for {key} with seed {subSeed}");
        }
        return _subRngs[key];
    }

    // 简单定向的哈希算法
    private int GenerateDeterministicSeed(int baseSeed, string key){
        unchecked // 允许溢出
        {
            int hash = baseSeed;
            foreach (char c in key)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }
}