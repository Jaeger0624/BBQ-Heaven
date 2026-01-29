using System;
using System.Collections.Generic;
using Sirenix.Serialization;

[Serializable]
public abstract class FoodCardEnhancement
{
    [OdinSerialize]
    public abstract string Name { get; }
    public abstract string SpriteName { get; }
    
    [OdinSerialize]
    public abstract string Description { get; }

    // 【卡牌级强化】影响生成的数量 (例如：双重)
    public virtual int ModifySpawnCount(int currentCount) => currentCount;

    // 【实例级强化】影响生成实例后的属性 (例如：初始美味度+1)
    public virtual void OnInstanceCreated(FoodInstance instance) { }

    public static FoodCardEnhancement GetRandomEnhancement(Rng rng){
        List<FoodCardEnhancement> enhancements = new List<FoodCardEnhancement>
        {
            new Enhancement_双重(),
            new Enhancement_多重(),
            new Enhancement_省时(),
        };
        return rng.PickOne(enhancements);
    }
}

[Serializable]
public class Enhancement_双重 : FoodCardEnhancement
{
    public override string Name => "双重";
    public override string SpriteName => "双倍";
    public override string Description => "额外放置一个食材";
    public override int ModifySpawnCount(int currentCount) => currentCount + 1;
}

[Serializable]
public class Enhancement_多重 : FoodCardEnhancement
{
    public override string Name => "多重";
    public override string SpriteName => "三倍";
    public override string Description => "额外放置两个食材，食材时间成本+2。";
    public override int ModifySpawnCount(int currentCount) => currentCount + 2;
    public override void OnInstanceCreated(FoodInstance instance) 
    {
        instance.timeCost += 2;
    }
}

public class Enhancement_省时 : FoodCardEnhancement
{
    public override string Name => "省时";
    public override string SpriteName => "省时";
    public override string Description => "食材时间成本-1。";
    public override int ModifySpawnCount(int currentCount) => currentCount;
    public override void OnInstanceCreated(FoodInstance instance) 
    {
        if (instance.timeCost <= 0) return;
        instance.timeCost -= 1;
    }
}