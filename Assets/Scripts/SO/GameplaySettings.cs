using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplaySettings", menuName = "Settings/GameplaySettings")]
public class GameplaySettings : ScriptableObject{
    public int supplyFoodTime_默认 = 1;
    public int 烤串额外时间成本_默认 = 1;
    [LabelText("时间系统")]
    public Vector2Int targetTime_默认 = new Vector2Int(0, 22);
    public Vector2Int currentTime_默认 = new Vector2Int(0, 20);


    [Header("顾客系统")]
    [LabelText("默认每日开始客户数")]
    public int 默认每日开始客户数 = 2;
    [LabelText("每分钟来一个顾客的可能性（不能大于等于1）")]
    public float 每分钟来一个顾客的可能性 = 0.2f;

    [LabelText("默认顾客满意度区间")]
    public List<float> 默认顾客满意度区间 = new List<float>{1.5f,2.5f,3f};


    [LabelText("每日目标分数")]
    public List<int> 每日目标分数列表 = new List<int>();
}