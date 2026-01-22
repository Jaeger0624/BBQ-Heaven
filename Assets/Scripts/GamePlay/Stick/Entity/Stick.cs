using System;
using cfg;
using QFramework;
using UnityEngine;
/// <summary>
/// 烤串系统 - 实例层
/// </summary>
[Serializable]
public class Stick : ICanGetSystem{
    public string guid { get; private set; }
    public string name => stickData.Name;
    public StickData stickData;
    public IStickStrategy strategy;
    public int maxFoodCount = 0;  // 最大食材数
    public int extraTimeCost = 0;
    public int originalTimeCost = 0;
    public Stick(StickData stickData){
        this.guid = Guid.NewGuid().ToString();
        this.stickData = stickData;
        this.strategy = GetStrategy(stickData.Strategy);
        this.extraTimeCost = 0;
        this.maxFoodCount = stickData.MaxFoodCount;
        this.originalTimeCost = stickData.OriginCost;
    }

    public static IStickStrategy GetStrategy(StickStrategy stickStrategy){
        switch (stickStrategy){
            case StickStrategy.默认:
                return new StickStrategy_默认();
            default:
                return new StickStrategy_默认();
        }
    }

    public void TryReturnStick(){
        //TODO: 如果是临时的那就不返回
        extraTimeCost += SettingManager.Instance.GameplaySettings.烤串额外时间成本_默认;
        this.GetSystem<IStickSystem>().AddCurrentStick(this);
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}