using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(fileName = "DevSettings", menuName = "Settings/DevSettings")]
public class DevSettings : ScriptableObject{
    
    public float AnimSpeed = 1f;
    public bool UseAnimTimeScale = true;
    public float AnimTimeScale = 1f;

    /// <summary>
    /// 增长速度
    /// </summary>
    /// 
    [LabelText("增长速度")]
    public float AnimTimeScaleSpeed = 0.001f;
    /// <summary>
    /// 基础时间缩放
    /// </summary>
    [LabelText("基础时间缩放")]
    public float baseTimeScale = 1.2f;

    [LabelText("最大时间缩放")] public float maxTimeScale = 7f;

    [LabelText("动画默认间隔")] public float defaultAnimInterval = 0.7f;
    
    [Header("性能设置")]
    [LabelText("目标帧率 (0=不限制, 建议0或120+)")] 
    [Tooltip("设置目标帧率。注意：帧率越高，动画越正常；帧率越低，动画越容易出现问题。\n" +
              "建议：设置为0（不限制帧率，可获得最佳动画效果）或120+（高帧率）。\n" +
              "不推荐设置为60以下，可能导致动画提前中断。")]
    public int targetFrameRate = 0;
    
    public Color AddRarityTextColor = new Color(1, 1, 0);
    public Color AddTasteTextColor = new Color(0, 1, 0);

    [Header("食材实例移动动画延迟")]
    public float foodInstanceMoveAnimDelay = 0.04f;

    [Header("Tooltip")]
    public float tooltipMaxWidth = 300f;
    public Vector2 tooltipPadding = new Vector2(10f, 10f);
    
    [Header("Hand Visual")]
    public float archRadius = 2500f;
    public float centerOffset = -2400f;
    public int maxHandSizeForLayout = 10;
    public float maxSpacingAngle = 10f;
    public float minSpacingAngle = 4f;
    public float rate = 2f;
    public float minPushAngle = 2f;
    public float pushMaxAngle = 15f;
    public int pushRange = 2;
    public float pushFalloffPower = 1.5f;
    public float hoverScale = 1.4f;
    public float hoverHeightOffset = 120f;
    public float targetingHeightOffset = 150f;


    [Header("Tooltip")]
    public float tooltipWaitTime = 0.5f;
}