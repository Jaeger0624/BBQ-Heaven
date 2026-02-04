using QFramework;
public static class LogKit
{
    // 懒加载获取 Utility
    private static ILogger _logger;
    private static ILogger Logger 
    {
        get
        {
            if (_logger == null)
            {
                // 从架构中获取
                _logger = GameArchitecture.Interface.GetUtility<ILogger>();
            }
            return _logger;
        }
    }

    [System.Diagnostics.Conditional("DEBUG")] // 仅在 Debug 模式下编译
    public static void I(string msg) => Logger.LogInfo(msg);
    public static void W(string msg) => Logger.LogWarning(msg);
    public static void E(string msg) => Logger.LogError(msg);
}