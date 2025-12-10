using System;
using System.Collections.Generic;

// 玩家信息数据
[Serializable]
public class PlayerInfoData{
    public PlayerCharacter playerCharacter;
    public int coin;
    public List<Mascot> mascots;
    public Dictionary<string, Food> foodRepositorys;
    public Dictionary<string, Stick> stickRepositorys;
    public Dictionary<string, Card> cardRepositorys;
    public List<Recipe> recipes;
}

// 游戏进程数据
[Serializable]
public class GameProcessData{
    public int month;
    public int day;
}