

#region 事件
using System.Collections.Generic;
using QFramework;

public class StartNewDayEvent : AbstractEvent, IMascotEvent{
    public int month;
    public int day;
    public EventStage stage;
    public StartNewDayEvent(int month, int day, EventStage stage){
        this.month = month;
        this.day = day;
        this.stage = stage;
    }
    public bool StageMeet(EventStage stage) => this.stage == stage;
    public List<object> parameters => new List<object>{};
}



public class EndDayEvent : AbstractEvent{
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
