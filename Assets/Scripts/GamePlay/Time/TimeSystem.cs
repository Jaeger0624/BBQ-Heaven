using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface ITimeSystem : ISystem{
    // 推动时间点
    void PushTimePoint(int timePoint);
    void SetNextTimeInfo(TimeInfo currentTime, TimeInfo targetTime);
    void RefreshTimeInfo();
    TimeInfo CurrentTime { get; }
    TimeInfo TargetTime { get; }
    float CurrentProcess { get; }
    bool isTimeUp { get; }
    int GetCostTime(List<FoodInstance> foodInstances, Stick stick);
}



public class TimeSystem_默认 : AbstractSystem, ITimeSystem
{
    private TimeInfo currentTime;
    private TimeInfo targetTime;
    public bool isTimeUp { get; private set; } = false;
    private (TimeInfo currentTime, TimeInfo targetTime) nextTimeInfo;
    public TimeInfo CurrentTime => currentTime;
    public TimeInfo TargetTime => targetTime;
    public float CurrentProcess{
        get{
            float originCurrentTimePoint = currentTime.GetOriginalTimeInfo().GetTotalTimePoint();
            float currentTimePoint = currentTime.GetTotalTimePoint();
            float targetTimePoint = targetTime.GetOriginalTimeInfo().GetTotalTimePoint();
            return (currentTimePoint - originCurrentTimePoint) / (targetTimePoint - originCurrentTimePoint);
        }
    }
    protected override void OnInit()
    {
        targetTime = null;
        currentTime = null;
        nextTimeInfo = (null, null);
        this.isTimeUp = false;
        // 事件注册
        // 1. 注册开始新一天事件 -> 刷新时间信息
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
    }

    private void OnStartNewDayEvent(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.Before)) return;
        RefreshTimeInfo();
    }

    public void RefreshTimeInfo()
    {
        currentTime = nextTimeInfo.Item1;
        targetTime = nextTimeInfo.Item2;
        nextTimeInfo = (null, null);
        this.isTimeUp = false;
    }

    public void PushTimePoint(int timePoint)
    {
        // 0. 若在已经结束时，再尝试推动时间点，则强制推进流程
        //TODO: 改成限制操作，高亮打烊按钮

        // 1. 推动当前时间
        currentTime.OnAddTimePoint(timePoint);
        Debug.Log($"【TimeSystem】推动时间点：{timePoint}，当前时间：{currentTime.hour:D2}:{currentTime.minute:D2}");

        // 3. 发送时间Tick事件
        this.SendEvent(new TimeTickEvent(timePoint, currentTime, targetTime));

        // 4. 检查时间是否到达目标时间
        isTimeUp = currentTime.GetTotalTimePoint() >= targetTime.GetTotalTimePoint();

        if (isTimeUp){
            Debug.Log("【TimeSystem】现在是疲劳状态！");
            this.SendEvent(new TimeUpEvent());
        }
    }
    public void SetNextTimeInfo(TimeInfo currentTime, TimeInfo targetTime)
    {
        nextTimeInfo = (currentTime, targetTime);
    }

    public int GetCostTime(List<FoodInstance> foodInstances, Stick stick)
    {
        return foodInstances.Count + stick.extraTimeCost + SettingManager.Instance.GameplaySettings.makeBBQTime_默认;
    }
}

#region 事件
public class TimeTickEvent : AbstractEvent{
    public int timePoint;
    public TimeInfo currentTime;
    public TimeInfo targetTime;
    public TimeTickEvent(int timePoint, TimeInfo currentTime, TimeInfo targetTime){
        this.currentTime = currentTime;
        this.targetTime = targetTime;
        this.timePoint = timePoint;
    }
}
public class TimeUpEvent : AbstractEvent{
    public TimeUpEvent(){}
}
#endregion