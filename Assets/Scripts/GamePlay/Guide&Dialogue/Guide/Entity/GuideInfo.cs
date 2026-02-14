using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideInfo", menuName = "GuideInfo")]
public class GuideInfo : ScriptableObject{
    [TextArea(3, 10)]
    public string guideText = "这是教程文本";
    // 所监听的玩家动作类型（当触发时，会推进教程）
    public PlayerActionType playerActionType = PlayerActionType.无;
}


// 气泡向左边显示还是向右边显示
public enum GuideTextDirection
{
    左,
    右,
}