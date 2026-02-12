public class GALogContext
{
    public string Log { get; private set; } = "执行逻辑";
    public void SetLog(string log)
    {
        this.Log = log;
    }
    // 还可以加其他数据，比如造成的伤害值、获得的金币等
}