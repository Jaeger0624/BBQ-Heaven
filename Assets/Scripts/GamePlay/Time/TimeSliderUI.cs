using UnityEngine;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

// 时间进度条UI
public class TimeSliderUI : MonoBehaviour, IController, ISliderUI
{
    [SerializeField] private Slider valueSlider;
    [SerializeField] private Slider previewSlider;
    private float duration => SettingManager.Instance.AnimSettings.timeProgressBarAnimDuration;
    private float easeTime => SettingManager.Instance.AnimSettings.timeProgressBarAnimEasePeriod;
    private float amplitude => SettingManager.Instance.AnimSettings.timeProgressBarAnimEaseOvershootOrAmplitude;
    public void SetPreview(float value)
    {
        previewSlider.DOValue(value, duration).SetEase(Ease.OutBack, amplitude, easeTime);
    }

    public void SetValue(float value)
    {
        valueSlider.DOValue(value, duration).SetEase(Ease.OutBack, amplitude, easeTime);
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}