using QFramework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Runtime.CompilerServices;
using TMPro;

// 进度条接口
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
    [SerializeField] private TextMeshProUGUI timeText;

    private int currentTotalTimePoint = 0;

    void OnEnable()
    {
        // 1. 注册时间Tick事件 -> 更新进度条
        this.RegisterEvent<TimeTickEvent>(OnTimeTickEvent).UnRegisterWhenDisabled(this.gameObject);
        this.RegisterEvent<TimePreviewEvent>(OnTimePreviewEvent).UnRegisterWhenDisabled(this.gameObject);
        // 2. 注册开始新一天事件 -> 重置进度条
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent).UnRegisterWhenDisabled(this.gameObject);
    }
    private void UpdateTimeText()
    {
        int orginTotalTimePoint = this.GetSystem<ITimeSystem>().CurrentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        int totalTimePoint = this.GetSystem<ITimeSystem>().CurrentTime.GetTotalTimePoint() - orginTotalTimePoint;
        int targetTimePoint = this.GetSystem<ITimeSystem>().TargetTime.GetTotalTimePoint() - orginTotalTimePoint;
        timeText.text = $"<color=white>{totalTimePoint}</color>/{targetTimePoint}";
        
        currentTotalTimePoint = totalTimePoint;
    }
    private void UpdateTimeTextPreview(TimePreviewEvent evt){
        int originTotalTimePoint = this.GetSystem<ITimeSystem>().CurrentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        int totalTimePoint = evt.currentTime.GetTotalTimePoint() - originTotalTimePoint;
        int targetTimePoint = this.GetSystem<ITimeSystem>().TargetTime.GetTotalTimePoint() - originTotalTimePoint;

        if (totalTimePoint == currentTotalTimePoint){
            UpdateTimeText();
        }
        else{
            timeText.text = $"<color=yellow>{totalTimePoint}</color>/{targetTimePoint}";
        }
    }
    private void OnStartNewDayEvent(StartNewDayEvent evt){
        ResetProcessBar();

        UpdateTimeText();
        currentTotalTimePoint = 0;
    }
    private void OnTimePreviewEvent(TimePreviewEvent evt){
        if (sliderUI == null) return;
        // 计算进度
        float process = CalculateProcess(evt.currentTime, evt.targetTime);
        sliderUI.SetPreview(process);
        // Debug.Log($"【TimeController】时间预览：{process}");

        UpdateTimeTextPreview(evt);
    }
    private void OnTimeTickEvent(TimeTickEvent evt){
        if (sliderUI == null) return;
        // 计算进度
        float process = CalculateProcess(evt.currentTime, evt.targetTime);
        sliderUI.SetValue(process);

        UpdateTimeText();
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
    public int timeCost;
    public TimePreviewEvent(TimeInfo currentTime, TimeInfo targetTime, int timeCost){
        this.currentTime = currentTime;
        this.targetTime = targetTime;
        this.timeCost = timeCost;
    }


    public TimePreviewEvent(int timeCost){
        if (this.GetSystem<ITimeSystem>() == null){
            Debug.LogError("TimeSystem is null");
            return;
        }
        if (this.GetSystem<ITimeSystem>().CurrentTime == null || this.GetSystem<ITimeSystem>().TargetTime == null){
            Debug.LogError("TimeInfo is null");
            return;
        }
        TimeInfo currentTime = this.GetSystem<ITimeSystem>().CurrentTime.Clone(timeCost);
        TimeInfo targetTime = this.GetSystem<ITimeSystem>().TargetTime.Clone(0);
        this.currentTime = currentTime;
        this.targetTime = targetTime;
        this.timeCost = timeCost;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}


#endregion