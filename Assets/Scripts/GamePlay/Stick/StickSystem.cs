using QFramework;
using cfg;
using System.Collections.Generic;
using System.Linq;
public interface IStickSystem : ISystem, ISavable
{
    Stick selectedStick { get; }
    Stick AddStickToRepository(string id);
    void AddCurrentStick(Stick stick);
    void UseCurrentStick(Stick stick);
    void RemoveCurrentStick(Stick stick);
    void ResetCurrentSticks();
    List<Stick> StickRepositorys();
    void SetSelectedStick(Stick stick);
    void UnselectStick();
}

/// <summary>
/// 烤串系统 - 系统层
/// </summary>
public class StickSystem : AbstractSystem, IStickSystem
{
    // 烤串仓库
    private Dictionary<string, Stick> stickRepository;
    // 当前烤串列表
    private List<Stick> currentSticks;
    public Stick selectedStick { get; private set; }
    protected override void OnInit()
    {
        stickRepository = new Dictionary<string, Stick>();
        currentSticks = new List<Stick>();
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDayEvent);
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.stickRepositorys = stickRepository;
    }
    public void Load(GameArchive archive)
    {
        stickRepository = archive.playerInfoData.stickRepositorys.ToDictionary(stick => stick.Key, stick => stick.Value);
    }
    private void OnStartNewDayEvent(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;
        ResetCurrentSticks();
    }
    public Stick AddStickToRepository(string id){
        // 获取烤串配置
        StickData stickData = this.GetSystem<IDataSystem>().GetStickData(id);
        // Debug.Log($"【StickSystem】添加烤串到烤串仓库: {stickData.Name}");
        // 创建烤串
        Stick stick = CreateStick(stickData);
        // 添加到仓库
        stickRepository.Add(stick.guid, stick);
        return stick;
    }
    private Stick CreateStick(StickData stickData) => new Stick(stickData);
    public void ResetCurrentSticks(){
        List<Stick> tempCurrentSticks = new List<Stick>();
        // 复制当前烤串列表
        foreach (var stick in currentSticks) tempCurrentSticks.Add(stick);
        // 移除当前烤串列表中的所有烤串
        foreach (var stick in tempCurrentSticks) RemoveCurrentStick(stick);
        // 添加仓库中的所有烤串到当前烤串列表
        stickRepository.Values.ToList().ForEach(stick => AddCurrentStick(new Stick(stick.stickData)));
    }

    // 添加一个烤串到当前烤串列表（涉及UI更新）
    public void AddCurrentStick(Stick stick){
        currentSticks.Add(stick);
        this.SendEvent(new AddStickEvent(stick));
    }
    // 从当前烤串列表中移除一个烤串（涉及UI更新）
    public void UseCurrentStick(Stick stick){
        currentSticks.Remove(stick);
        // 删除后肯定要取消选中
        UnselectStick();
        this.SendEvent(new RemoveStickEvent(stick));
    }
    public void RemoveCurrentStick(Stick stick){
        currentSticks.Remove(stick);
        UnselectStick();
        this.SendEvent(new RemoveStickEvent(stick));
    }
    public void SetSelectedStick(Stick stick){
        if (selectedStick != null) UnselectStick();
        selectedStick = stick;
        // Debug.Log($"【StickSystem】选中烤串: {stick.name}");
    }
    public void UnselectStick(){
        this.SendEvent(new UnselectStickEvent());
        if (selectedStick == null) return;
        selectedStick = null;
    }
    public List<Stick> GetCurrentSticks(){
        List<Stick> tempCurrentSticks = new List<Stick>();
        // 复制当前烤串列表
        foreach (var stick in currentSticks) tempCurrentSticks.Add(stick);
        return tempCurrentSticks;
    }
    // 返回只读列表（只读列表不能修改）
    public List<Stick> StickRepositorys() => stickRepository.Values.ToList();

}



# region 烤串系统事件
// 移除一个当日烤串
public class RemoveStickEvent : AbstractEvent{
    public Stick stick;
    public RemoveStickEvent(Stick stick){
        this.stick = stick;
    }
}
// 添加一个当日烤串
public class AddStickEvent : AbstractEvent{
    public Stick stick;
    public AddStickEvent(Stick stick){
        this.stick = stick;
    }
}

// 选中一个烤串
public class SelectStickEvent : AbstractEvent{
    public Stick stick;
    public SelectStickEvent(Stick stick){
        this.stick = stick;
    }
}

// 取消选中烤串
public class UnselectStickEvent : AbstractEvent{
}
#endregion