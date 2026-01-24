using UnityEngine;

[CreateAssetMenu(fileName = "PrefabSettings", menuName = "Settings/PrefabSettings")]
public class PrefabSettings : ScriptableObject
{
    [Header("食材展示")]
    public DisplayFoodView foodItemPrefab;
    public DisplayFoodView foodCardItemPrefab;

    [Header("吉祥物展示")]
    public DisplayMascotView mascotItemPrefab;

    // [Header("装饰品展示")]

    [Header("卡牌展示")]
    public DisplayCardView cardItemPrefab;
}
