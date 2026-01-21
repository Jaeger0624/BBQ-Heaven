using System;
using Sirenix.Serialization;

[Serializable]
public abstract class FoodCardEnhancement
{
    [OdinSerialize]
    public abstract string Name { get; }
    
    [OdinSerialize]
    public abstract string Description { get; }

    // 【卡牌级强化】影响生成的数量 (例如：双重)
    public virtual int ModifySpawnCount(int currentCount) => currentCount;

    // 【实例级强化】影响生成实例后的属性 (例如：初始美味度+1)
    public virtual void OnInstanceCreated(FoodInstance instance) { }
}

[Serializable]
public class DoubleSpawnEnhancement : FoodCardEnhancement
{
    public override string Name => "双重";
    public override string Description => "生成时额外创建一个实例。";
    public override int ModifySpawnCount(int currentCount) => currentCount + 1;
}

[Serializable]
public class PreservedEnhancement : FoodCardEnhancement
{
    public override string Name => "陈酿";
    public override string Description => "美味度+2，但抽取后的放置冷却时间+1。";
    public override void OnInstanceCreated(FoodInstance instance) 
    {
        instance.taste += 2;
        instance.timeCost += 1;
    }
}