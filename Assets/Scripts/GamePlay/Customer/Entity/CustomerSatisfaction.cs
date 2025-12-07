using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 只负责做逻辑上下文，不负责做动画
/// </summary>
public class CustomerSatisfaction : ICanGetSystem, ICanSendEvent{
    private List<SatisMultiplier> SatisMultipliers = new List<SatisMultiplier>();
    private float newMultiplier = 1f;
    private float originalMultiplier = 1f;
    public void AddSatisfactionMultiplier(float satisfactionMultiplier, string name, IAnimTask animTask = null){
        this.SatisMultipliers.Add(new SatisMultiplier(satisfactionMultiplier, name, animTask));
        this.newMultiplier = satisfactionMultiplier;
    }
    public CustomerSatisfaction(){
        this.SatisMultipliers = new List<SatisMultiplier>();
        this.originalMultiplier = 1f;
    }
    public CustomerSatisfaction(float multiplier){
        this.SatisMultipliers = new List<SatisMultiplier>();
        this.originalMultiplier = multiplier ;
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    public float GetFinal(){
        float final = originalMultiplier;
        foreach (var multiplier in SatisMultipliers){
            final *= multiplier.multiplier;
        }
        return final;
    }

    public float GetNewMultiplier(){
        return this.newMultiplier;
    }

    public List<SatisMultiplier> GetAllMultipliers(){
        return SatisMultipliers;
    }
}


// 承载描述一个满意度乘区的所有信息
public class SatisMultiplier{
    public float multiplier;
    public string name;
    public IAnimTask clueAnim; // 满意度变化动画(满意度乘区的来源可能不同，但最后要统一集合成序列的话，就不能在各自执行出创建动画，而是传入后最后整合)
    public SatisMultiplier(float multiplier, string name, IAnimTask animTask){
        this.multiplier = multiplier;
        this.name = name;
        this.clueAnim = animTask;
    }
}
