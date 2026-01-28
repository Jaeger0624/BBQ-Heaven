using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IRecordSystem : ISystem{
    void RecordEvent(Record record);
}
// 记录系统 - 系统层
// 职责：负责记录玩家的操作、事件的结果等，让玩家可以回看游戏过程
// 难点不在于记录，而是在于搞清楚层次，如何展示连锁反应
public class RecordSystem : AbstractSystem, IRecordSystem
{
    private List<Record> recordEvents = new List<Record>();



    protected override void OnInit()
    {
    }

    public void RecordEvent(Record record)
    {
        recordEvents.Add(record);
    }
}





public class Record{
    public string content;

    public Record(string content){
        this.content = content;
    }
}