using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public interface IGameSystem : ISystem{
    public int CurrentDay { get; }
    public int MaxDay { get; set; }
    public void StartMonth();
    public void StartDay();
    public void EndDay();
    public void EndMonth();
}

/// <summary>
/// 单局游戏进程管理器 - 系统层
/// </summary>
public class GameSystem : AbstractSystem, IGameSystem
{
    private int maxDay; // 当月最大天数
    public int CurrentDay => currentDay;
    public int MaxDay { get => maxDay; set => maxDay = value; }
    private int currentDay = 1; // 当前天数
    private int currentDayTime = 0; // 当前天数时间（只是记录用）
    private int maxMonth; // 当年最大月份
    private int currentMonth = 1; // 当前月份
    
    protected override void OnInit()
    {
        currentDay = 0;
        currentDayTime = 0;
        currentMonth = 0;
        maxDay = 3;
        maxMonth = 3;
    }

    public void StartMonth(){
        currentMonth++;
        currentDay = 0;
        // TODO: 月度初始化（主题、目标等）
        Debug.Log($"【GameSystem】开始新月份:{currentMonth}月");
    }

    public void StartDay(){
        currentDay++;
        currentDayTime = 0;

        Debug.Log($"【GameSystem】开始新一天:{currentMonth}月{currentDay}日");

        // 一天开始 - 系统初始化前
        this.SendEvent(new StartNewDayEvent(currentMonth, currentDay, EventStage.Before));

        // 一天开始 - 系统初始化
        this.SendEvent(new StartNewDayEvent(currentMonth, currentDay, EventStage.System));

        // 处理预定顾客补充（若有）
        // TODO: 这应该是Bug，这个在StartNewDayEvent中处理了
        // var preSchedules = this.GetSystem<ICustomerSystem>().GetPreScheduledCustomers();
        // this.GetSystem<ICustomerSystem>().CreateCustomer(preSchedules.Select(info => info.Item1).ToList());

        // 一天开始 - 系统初始化后
        this.SendEvent(new StartNewDayEvent(currentMonth, currentDay, EventStage.After));

        this.GetSystem<IScoreSystem>().ChangeTargetScore();
    }

    // 一天结束的系统层逻辑是清空实例、重置仓库等
    public void EndDay(){

        // 播报一天结束 - 前（例如，一天结束时检测场上剩余食材效果）
        this.SendEvent(new EndDayEvent(currentMonth, currentDay, EventStage.Before));

        // 播报一天结束 - 系统（例如，一天结束时，FoodSystem清除剩余食材实例）
        this.SendEvent(new EndDayEvent(currentMonth, currentDay, EventStage.System));

        // 播报一天结束 - 后
        this.SendEvent(new EndDayEvent(currentMonth, currentDay, EventStage.After));

        // 一天最后的最后，是结算分数
        if (!this.GetSystem<IScoreSystem>().CheckTargetScore()) return;
    }
    public void EndMonth(){
        Debug.Log("【GameSystem】结束月份");
    }
}
/// <summary>
/// 事件阶段
/// </summary>
public enum EventStage{
    Before,
    System,
    After
}


#region 事件
public class StartNewDayEvent : IEvent{
    public int month;
    public int day;
    public EventStage stage;
    public StartNewDayEvent(int month, int day, EventStage stage){
        this.month = month;
        this.day = day;
        this.stage = stage;
    }
    public bool StageMeet(EventStage stage) => this.stage == stage;
}



public class EndDayEvent : IEvent{
    public int month;
    public int day;
    public EventStage stage;
    public EndDayEvent(int month, int day, EventStage stage){
        this.month = month;
        this.day = day;
        this.stage = stage;
    }
    public bool StageMeet(EventStage stage) => this.stage == stage;
}
#endregion
