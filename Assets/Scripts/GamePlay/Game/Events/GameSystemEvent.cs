

#region 事件
using QFramework;

public class StartNewDayEvent : AbstractEvent{
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
