using System;
using System.Collections.Generic;
using Sirenix.Serialization;

// 玩家信息数据
[Serializable]
public class PlayerInfoData{
    [OdinSerialize]
    public PlayerCharacter playerCharacter;
    public int coin;
    public int reputation;
    public int nextLevelReputation;
    public int level;
    // 收入
    public int income;
    [OdinSerialize]
    public List<Mascot> mascots;
    [OdinSerialize]
    public Dictionary<string, FoodCard> foodRepositorys;
    [OdinSerialize]
    public Dictionary<string, Stick> stickRepositorys;
    [OdinSerialize]
    public Dictionary<string, Card> cardRepositorys;
    [OdinSerialize]
    public List<Recipe> recipes;
}

// 游戏进程数据
[Serializable]
public class GameProcessData{
    // 日月信息
    public int month;
    public int day;
    public List<int> stateIndexes;
}