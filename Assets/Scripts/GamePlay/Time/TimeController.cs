using QFramework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TimeController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    // [SerializeField] private VividProcessBar processBar;

    [SerializeField] private Slider progressBar;
    [SerializeField] private TimeUseView timeUseView;
    private float duration => SettingManager.Instance.AnimSettings.timeProgressBarAnimDuration;
    private float easeTime => SettingManager.Instance.AnimSettings.timeProgressBarAnimEasePeriod;
    private float amplitude => SettingManager.Instance.AnimSettings.timeProgressBarAnimEaseOvershootOrAmplitude;

    void OnEnable()
    {
        // 1. 注册时间Tick事件 -> 更新进度条
        this.RegisterEvent<TimeTickEvent>(OnTimeTickEvent).UnRegisterWhenDisabled(this.gameObject);
        // 2. 注册开始新一天事件 -> 重置进度条
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDayEvent).UnRegisterWhenDisabled(this.gameObject);
        // 3. 注册显示烤串计时器事件 -> 显示烤串计时器
        this.RegisterEvent<ShowStickTimerEvent>(OnShowStickTimerEvent).UnRegisterWhenDisabled(this.gameObject);
        // 4. 注册隐藏烤串计时器事件 -> 隐藏烤串计时器
        this.RegisterEvent<HideStickTimerEvent>(OnHideStickTimerEvent).UnRegisterWhenDisabled(this.gameObject);
    }
    void Update()
    {
        
    }
    private void OnShowStickTimerEvent(ShowStickTimerEvent evt){
        timeUseView.Show();
        timeUseView.UpdateVisual(evt.time);
    }
    private void OnHideStickTimerEvent(HideStickTimerEvent evt){
        timeUseView.Hide();
    }
    private void OnStartNewDayEvent(StartNewDayEvent evt){
        ResetProcessBar();
    }

    private void OnTimeTickEvent(TimeTickEvent evt){
        // 计算进度
        SetProcessBar(evt.currentTime, evt.targetTime);
    }

    private void ResetProcessBar(){
        progressBar.DOValue(0f, duration).SetEase(Ease.OutBack, amplitude, easeTime);
    }
    private void SetProcessBar(TimeInfo currentTime, TimeInfo targetTime){
        // 1. 获取三种时间点的总时间点
        int originCurrentTimePoint = currentTime.GetOriginalTimeInfo().GetTotalTimePoint();
        int currentTimePoint = currentTime.GetTotalTimePoint();
        int targetTimePoint = targetTime.GetOriginalTimeInfo().GetTotalTimePoint();

        // 2. 计算进度
        float process = (float)(currentTimePoint - originCurrentTimePoint) / (targetTimePoint - originCurrentTimePoint);
        // processBar.SetNormalizedProgress(process);
        progressBar.DOValue(process, 0.4f).SetEase(Ease.OutBack, 1.50f, 0.4f);
    }
}
