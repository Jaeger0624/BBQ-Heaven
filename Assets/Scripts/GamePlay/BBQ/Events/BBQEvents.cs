using QFramework;

public class AddBBQPropertyAnimEvent : AbstractEvent{
    public int totalRarity;
    public int totalTaste;
    public int addRarity;
    public int addTaste;
    public string adderName;
    public AddBBQPropertyAnimEvent(int totalRarity, int totalTaste, int addRarity, int addTaste, string adderName){
        this.totalRarity = totalRarity;
        this.totalTaste = totalTaste;
        this.addRarity = addRarity;
        this.addTaste = addTaste;
    }
}




#region BBQSystem事件
public class CombineBBQEvent : AbstractEvent{
    public BBQ bbq;
    public CombineBBQEvent(BBQ bbq){
        this.bbq = bbq;
    }
}
public class FinishCombineBBQEvent : AbstractEvent{
    public BBQ bbq;
    public FinishCombineBBQEvent(BBQ bbq){
        this.bbq = bbq;
    }
}

public class FinishCombineBBQEvent_动画 : AbstractEvent{}
public class CombineBBQEvent_动画 : AbstractEvent{}
// 将烧烤实例添加到烧烤仓库中
public class AddBBQToRepositoryEvent : AbstractEvent{
    public BBQ bbq;
    public AddBBQToRepositoryEvent(BBQ bbq){
        this.bbq = bbq;
    }
}
public class RemoveBBQFromRepositoryEvent : AbstractEvent{
    public BBQ bbq;
    public RemoveBBQFromRepositoryEvent(BBQ bbq){
        this.bbq = bbq;
    }
}
#endregion