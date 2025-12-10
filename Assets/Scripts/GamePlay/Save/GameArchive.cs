using System;
using UnityEngine;
public interface ISavable{
    void Save(GameArchive archive);
    void Load(GameArchive archive);
}

[Serializable]
public class GameArchive : MonoBehaviour
{
    public GameArchive()
    {
        saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    // metadata
    public string saveTime;
    // 玩家信息
    public PlayerInfoData playerInfoData;
    // 游戏进程
    public GameProcessData gameProcessData;
    
}
