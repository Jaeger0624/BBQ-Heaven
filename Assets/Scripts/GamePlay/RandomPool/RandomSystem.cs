using System.Collections.Generic;
using System.Linq;
using QFramework;

public interface IRandomSystem : ISystem{
    PlayerBuildContext playerBuildContext { get; set; }
    List<T> RandomGet<T>(int amount, IRandomStrategy randomStrategy) where T : class;
}

public class RandomSystem : AbstractSystem, IRandomSystem
{
    // 玩家构筑上下文
    public PlayerBuildContext playerBuildContext { get; set; } = new PlayerBuildContext();
    /// <summary>
    /// 随机获取指定数量的元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="amount">要获取的元素数量</param>
    /// <param name="randomStrategy">随机策略</param>
    /// <returns>随机获取的元素列表</returns>
    public List<T> RandomGet<T>(int amount, IRandomStrategy randomStrategy) where T : class{
        return randomStrategy.Random<T>(amount);
    }
    protected override void OnInit()
    {
    }
}

/// <summary>
/// 收束玩家构筑上下文
/// </summary>
[System.Serializable]
public class PlayerBuildContext : ICanGetSystem{
    public List<Mascot> mascots => this.GetSystem<MascotSystem>().Mascots.Values.ToList();
    public List<Food> foodRepositorys => this.GetSystem<FoodSystem>().FoodRepositorys().Values.ToList();
    public List<Stick> sticks => this.GetSystem<IStickSystem>().StickRepositorys();
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}
