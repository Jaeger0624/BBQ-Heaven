using cfg;
using QFramework;
#region 事件
public class AddEncounterEvent : AbstractEvent{
    public ActiveEncounter activeEncounter;
    public AddEncounterEvent(ActiveEncounter activeEncounter){
        this.activeEncounter = activeEncounter;
    }
}
public class RemoveEncounterEvent : AbstractEvent{
    public ActiveEncounter activeEncounter;
    public RemoveEncounterEvent(ActiveEncounter activeEncounter){
        this.activeEncounter = activeEncounter;
    }
}
public class StartInstantEncounterEvent : AbstractEvent{
    public InstantEncounterData data;
    public StartInstantEncounterEvent(InstantEncounterData data){
        this.data = data;
    }
}
#endregion