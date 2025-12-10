using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface IMascotSystem : ISystem, ISavable{
    Dictionary<string, Mascot> Mascots { get; }
    public void AddMascot(string id);
    public void RemoveMascot(string id);
    public void SoldMascot(string id);
    public void RemoveAllMascots();
}
public class MascotSystem : AbstractSystem, IMascotSystem
{
    // ID -> 吉祥物
    private Dictionary<string, Mascot> mascots;
    public Dictionary<string, Mascot> Mascots => mascots;

    protected override void OnInit()
    {
        mascots = new Dictionary<string, Mascot>();
    }
    protected override void OnDeinit()
    {
        mascots.Clear();
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.mascots = mascots.Values.ToList();
    }
    public void Load(GameArchive archive)
    {
        List<Mascot> mascotsToLoad = archive.playerInfoData.mascots;
        mascotsToLoad.ForEach(mascot => LoadMascot(mascot));
    }
    public void AddMascot(string mascotID)
    {

        // 检查是否已存在（是否堆叠）
        if (mascots.TryGetValue(mascotID, out Mascot mascot))
        {
            if (!mascot.data.Stackable) return; 
            mascot.OnStack();
            this.SendEvent(new MascotAddedEvent(mascot));
            return;
        }
        
        // 创建吉祥物实例
        MascotData mascotData = this.GetSystem<IDataSystem>().GetMascotData(mascotID);
        if (mascotData == null)
        {
            Debug.LogWarning($"吉祥物 {mascotID} 不存在");
            return;
        }
        mascot = new Mascot(mascotData);
        mascots.Add(mascotID, mascot);
        mascot.OnAdd();

        this.SendEvent(new MascotAddedEvent(mascot));
    }

    private void LoadMascot(Mascot mascot)
    {
        // TODO: 设置堆叠数量
        mascots.Add(mascot.ID, mascot);
        mascot.OnAdd();

        this.SendEvent(new MascotAddedEvent(mascot));
    }

    public void RemoveAllMascots()
    {
        List<Mascot> mascotsToRemove = mascots.Values.ToList();
        mascotsToRemove.ForEach(mascot => RemoveMascot(mascot.data.ID));
        mascots.Clear();
        Debug.Log("移除所有吉祥物");
    }

    public void RemoveMascot(string mascotID)
    {
        if (mascots.TryGetValue(mascotID, out Mascot mascot))
        {
            mascot.OnRemove();
            mascots.Remove(mascotID);
            this.SendEvent(new MascotRemovedEvent(mascot));
            return;
        }
        Debug.LogError($"吉祥物 {mascotID} 不存在,但尝试移除");
    }

    public void SoldMascot(string mascotID)
    {
        if (mascots.TryGetValue(mascotID, out Mascot mascot))
        {
            mascot.OnRemove();
            mascots.Remove(mascotID);
            this.SendEvent(new MascotSoldEvent(mascot));
            return;
        }
        Debug.LogError($"吉祥物 {mascotID} 不存在,但尝试出售");
    }
}

