using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public interface IBuffSystem : ISystem, ISavable{
    Dictionary<string, Buff> GetBuffs();
    int GetBuffStackNumber(string buffID);
    void AddBuff(string buffID, int amount = 1);
    int RemoveBuff(string buffID, int amount = 1);
    void UpdateBuff(string buffID, int stackNumber);
    void ClearBuff(string buffID);
    void ClearAllBuff();
}


public class BuffSystem : AbstractSystem, IBuffSystem{
    private Dictionary<string, Buff> buffs = new Dictionary<string, Buff>();
    protected override void OnInit()
    {
        buffs = new Dictionary<string, Buff>();
    }
    protected override void OnDeinit()
    {
        buffs.Clear();
    }
    #region 存档
    public void Save(GameArchive archive)
    {
        // archive.buffs = buffs;
    }

    public void Load(GameArchive archive)
    {
        // buffs = archive.buffs;
    }
    #endregion
    public Dictionary<string, Buff> GetBuffs() => buffs;
    public int GetBuffStackNumber(string buffID){
        if (buffs.TryGetValue(buffID, out Buff buff)){
            if (buff.isStackable) return buff.GetStackNumber();
            Debug.LogWarning($"Buff {buffID} 不能堆叠，但调用了获取堆叠数量方法");
            return 1;
        }
        return 0;
    }
    public void AddBuff(string buffID, int amount){
        if (buffs.TryGetValue(buffID, out Buff buff)){
            buff.AddStackNumber(amount);
        }
        else{
            buff = new Buff(buffID, () => ClearBuff(buffID), amount);
            buffs.Add(buffID, buff);
        }
        this.SendEvent(new BuffAddedEvent(buff, amount));
    }
    public void ClearAllBuff(){
        List<string> buffIDs = buffs.Keys.ToList();
        foreach (string buffID in buffIDs){
            ClearBuff(buffID);
        }
        buffs.Clear();
    }
    // 减少Buff
    public int RemoveBuff(string buffID, int amount = 1){
        if (buffs.TryGetValue(buffID, out Buff buff)){
            int stackNumber = buff.RemoveStackNumber(amount);
            this.SendEvent(new BuffRemovedEvent(buff, amount));
            return stackNumber;
        }
        return 0;
    }
    // 清除Buff
    public void ClearBuff(string buffID){
        if (buffs.TryGetValue(buffID, out Buff buff)){
            buffs.Remove(buffID);
            this.SendEvent(new BuffClearedEvent(buffID));
            return;
        }
    }
    // 更新Buff
    public void UpdateBuff(string buffID, int stackNumber){
    }

}