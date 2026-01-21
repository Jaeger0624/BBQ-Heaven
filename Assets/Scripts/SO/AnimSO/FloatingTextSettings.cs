using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public enum ScatterMode{
    Both,
    Left,
    Right,
}

[CreateAssetMenu(fileName = "NewFloatingTextSettings", menuName = "Settings/FloatingTextSettings")]
public class FloatingTextSettings : ScriptableObject
{
    [Header("--- 测试 ---")]
    public ScatterMode testScatterMode = ScatterMode.Both;
    public bool testFloatingText = false;
    [Header("--- 基础外观 ---")]
    public float Duration = 1.0f;       // 动画总基础时长
    public Color Color = Color.white;
    public float FontSize = 36f;
    public bool UseUnscaledTime = false; // 是否忽略 TimeScale (暂停时是否继续播放)

    [Header("--- 缩放与打击感 ---")]
    public float InitialScale = 0.5f;   // 初始大小
    public float PeakScale = 1.5f;      // 峰值大小 (打击感)
    public float EndScale = 1.0f;       // 结束时大小
    public float PunchDuration = 0.3f;  // 缩放动画时长
    [Header("--- 收集模式 ---")]
    public float GatherDelay = 0.5f;       // 散布后停留多久开始飞
    public float FlightDuration = 1f;    // 飞行时长
    public Ease FlightMotionEase = Ease.InBack;  // 飞行缓动
    public Ease FlightScaleEase = Ease.OutSine; // 飞行缩放缓动
    public float CurveDeviation = 0.5f;  
    public float FlightScale = 0.4f;   // 飞行缩放

    [Header("--- 扇形散射 ---")]
    public float SpreadAngle = 90f;
    public float ScatterDistance = 1f;
    public float ScatterTime = 0.3f;
    [LabelText("散射距离倍率最小值")]
    public float ScatterMultiplierMin = 0.8f;
    [LabelText("散射距离倍率最大值")]
    public float ScatterMultiplierMax = 1.2f;
}