using QFramework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Runtime.CompilerServices;
public interface ISliderUI
{
    void SetPreview(float value);
    void SetValue(float value);
}

public class TimeController : SerializedMonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    // [SerializeField] private VividProcessBar processBar;
    [OdinSerialize, ShowInInspector]
    private ISliderUI sliderUI;

    void OnEnable()
    {
        // 1. 注册时间Tick事件 -> 更新进度条
        this.RegisterEvent<TimeTickEvent>(OnTimeTickEvent).UnRegisterWhenDisabled(this.gameObject);
        this.RegisterEvent<TimePreviewEvent>(OnTimePreviewEvent).UnRegisterWhenDisabled(this.gameObject);
        // 2. 注册开始新一天事件 -> 重置进度条
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent).UnRegisterWhenDisabled(this.gameObject);
    }
    void Update()
    {
        
    }
    private void OnStartNewDayEvent(StartNewDayEvent evt){
        ResetProcessBar();
    }
    private void OnTimePreviewEvent(TimePreviewEvent evt){
        if (sliderUI == null) return;
        // 计算进度
        float process = CalculateProcess(evt.currentTime, evt.targetTime);
        sliderUI.SetPreview(process);
        // Debug.Log($"【TimeController】时间预览：{process}");
    }
    private void OnTimeTickEvent(TimeTickEvent evt){
        if (sliderUI == null) return;
        // 计算进度
        float process = CalculateProcess(evt.currentTime, evt.targetTime);
        sliderUI.SetValue(process);
    }

    private void ResetProcessBar(){
        if (sliderUI == null) return;
        sliderUI.SetValue(0f);
        sliderUI.SetPreview(0f);
    }
    private float CalculateProcess(TimeInfo currentTime, TimeInfo targetTime){
        int originCurrentTimePoint = currentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        int currentTimePoint = currentTime.GetTotalTimePoint();
        int targetTimePoint = targetTime.GetOriginalTimeInfo().GetTotalTimePoint();
        return (float)(currentTimePoint - originCurrentTimePoint) / (targetTimePoint - originCurrentTimePoint);
    }
}


#region 事件
public class TimePreviewEvent : AbstractEvent, ICanGetSystem{
    public TimeInfo currentTime;
    public TimeInfo targetTime;
    public TimePreviewEvent(TimeInfo currentTime, TimeInfo targetTime){
        this.currentTime = currentTime;
        this.targetTime = targetTime;
    }


    public TimePreviewEvent(int timeCost){
        TimeInfo currentTime = this.GetSystem<ITimeSystem>().CurrentTime.Clone(timeCost);
        TimeInfo targetTime = this.GetSystem<ITimeSystem>().TargetTime.Clone(0);
        this.currentTime = currentTime;
        this.targetTime = targetTime;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}


#endregion