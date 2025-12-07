using QFramework;

/// <summary>
/// 这个的核心功能是不用知道传进来的是什么类型的对象，都能直接通过IAnimPlayer接口获取到对应的动画任务
/// </summary>
public static class AnimationConverter{
    public static IAnimTask Convert<T>(T sender, string param) where T : class, IAnimPlayer {
        return sender.GetAnimTask(param);
    }
}

