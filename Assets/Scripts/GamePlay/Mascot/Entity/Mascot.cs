using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class Mascot : ICanGetSystem{
    public string ID => data.ID;
    public string name;
    public string description;
    public string effectDescription;
    public readonly MascotData data;
    // 堆叠数量
    public int StackNumber = 1;
    // 位置信息
    public int index;
    public List<SustainEffect> SEs;
    public Mascot(MascotData data){
        this.data = data;
        this.name = data.Name;
        this.description = data.Description;
        this.SEs = data.MainSE;
        this.effectDescription = data.EffectDescription;
    }

    public void OnAdd()
    {
        foreach (var se in SEs)
        {
            this.GetSystem<IGASystem>().ApplySE(this, se);
        }
    }

    public void OnRemove()
    {
        foreach (var se in SEs)
        {
            this.GetSystem<IGASystem>().RemoveSE(this, se);
        }
    }
    public void OnStack()
    {
        if (!data.Stackable){
            Debug.LogError($"吉祥物 {name} 不能堆叠，但调用了堆叠方法");
            return;
        }
        StackNumber++;
        OnAdd();
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}