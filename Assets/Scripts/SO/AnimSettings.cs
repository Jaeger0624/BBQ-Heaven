using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(fileName = "AnimSettings", menuName = "Settings/AnimSettings")]
public class AnimSettings : ScriptableObject
{
    [LabelText("食材实例上串间隔动画时间")]
    public float foodInstanceOnStickAnimInterval = 0.5f;
    [Header("食材实例移动动画延迟")]
    public float foodInstanceMoveAnimDelay = 0.04f;
    public float foodInstanceMoveDelay_棋盘上移动 = 0.03f;

    [Header("文本弹窗动画时间")]
    public float textSpawnLifetime_默认 = 0.6f;
    public float textSpawnLifetime_配方 = 3f;

    [Header("动画参数")]
    public float foodInstanceCreateDistance = 1f;
    [Header("时间进度条动画时间")]
    [LabelText("时间进度条动画持续时间")]
    public float timeProgressBarAnimDuration = 0.4f;
    [LabelText("时间进度条动画缓动幅度")]
    public float timeProgressBarAnimEaseOvershootOrAmplitude = 1.50f;
    [LabelText("时间进度条动画缓动周期")]
    public float timeProgressBarAnimEasePeriod = 0.4f;
}
