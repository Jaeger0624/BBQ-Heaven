using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideInfo", menuName = "GuideInfo")]
public class GuideInfo : ScriptableObject{
    [TextArea(3, 10)]
    public string guideText = "这是教程文本";
    public GuideAnimInfo guideAnimInfo;

    // 所监听的玩家动作类型（当触发时，会推进教程）
    public PlayerActionType playerActionType = PlayerActionType.无;
}



// 记录教程动画的位置/方向等参数
[Serializable]
public class GuideAnimInfo
{

}