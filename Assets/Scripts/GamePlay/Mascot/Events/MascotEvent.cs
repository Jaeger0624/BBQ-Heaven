

using QFramework;

#region 事件
public class MascotAddedEvent : IEvent{
    public readonly Mascot mascot;
    public MascotAddedEvent(Mascot mascot)
    {
        this.mascot = mascot;
    }
}

public class MascotRemovedEvent : IEvent{
    public readonly Mascot mascot;
    public MascotRemovedEvent(Mascot mascot)
    {
        this.mascot = mascot;
    }
}

public class MascotSoldEvent : IEvent{
    public readonly Mascot mascot;
    public MascotSoldEvent(Mascot mascot)
    {
        this.mascot = mascot;
    }
}


#endregion
