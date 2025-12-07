using cfg;

/// <summary>
/// 挂这个接口的类是可以被随机
/// </summary>
public interface IRandomProperty{
    /// <summary>
    /// 稀有度
    /// </summary>
    Rank rank {get;}

    /// <summary>
    /// 随机池
    /// </summary>
    RandomPoolType poolType {get;}
}

public enum RandomPoolType{
    默认,
    节日,
}