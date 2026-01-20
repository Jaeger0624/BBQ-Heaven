using UnityEngine;

[CreateAssetMenu(fileName = "PrefabSettings", menuName = "Settings/PrefabSettings")]
public class PrefabSettings : ScriptableObject
{
    [Header("食材展示")]
    public DisplayFoodView foodItemPrefab;
    public DisplayFoodView foodCardItemPrefab;
}
