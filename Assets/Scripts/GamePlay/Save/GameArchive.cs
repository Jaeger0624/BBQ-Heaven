using System;
using Sirenix.Serialization;
using UnityEngine;
public interface ISavable{
    void Save(GameArchive archive);
    void Load(GameArchive archive);
}

[Serializable]
public class GameArchive
{
    public GameArchive()
    {
        saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    // metadata
    public string saveTime;
    [OdinSerialize]
    // 玩家信息
    public PlayerInfoData playerInfoData = new PlayerInfoData();
    [OdinSerialize]
    // 游戏进程
    public GameProcessData gameProcessData = new GameProcessData();
    [OdinSerialize]
    // 随机数系统
    public RngSaveData rngSaveData = new RngSaveData();

    [OdinSerialize]
    // 随机池系统
    public ProbabilityInfo probabilityInfo;
}
