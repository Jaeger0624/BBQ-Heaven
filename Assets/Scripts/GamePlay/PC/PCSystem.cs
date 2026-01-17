using System.Collections.Generic;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 玩家角色系统 - 系统层
/// 用于处理玩家角色（Player Character）相关的逻辑
/// </summary>
public interface IPCSystem : ISystem, ISavable{
    ReactiveProperty<int> Level { get; }
    ReactiveProperty<int> Reputation { get; }
    ReactiveProperty<int> NextLevelReputation { get; }
    PlayerCharacter ChoosePC(string id);
    void InitPC(PlayerCharacter playerCharacter);
    // 增加口碑
    void AddReputation(int amount);
}


public class PCSystem : AbstractSystem, IPCSystem
{
    // 角色声誉与等级
    public ReactiveProperty<int> Level { get; private set; } = new ReactiveProperty<int>(1);
    public ReactiveProperty<int> Reputation { get; private set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> NextLevelReputation { get; private set; } = new ReactiveProperty<int>(10);
    // 角色信息
    private PlayerCharacter currentPC;

    protected override void OnInit()
    {
    }
    protected override void OnDeinit()
    {
        ClearPC();
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.playerCharacter = currentPC;
        archive.playerInfoData.reputation = Reputation.Value;
        archive.playerInfoData.nextLevelReputation = NextLevelReputation.Value;
        archive.playerInfoData.level = Level.Value;
    }
    public void Load(GameArchive archive)
    {
        LoadPC(archive.playerInfoData.playerCharacter);
    }
    #region 声望部分
    public void AddReputation(int amount)
    {
        int newReputation = Reputation.Value + amount;
        Reputation.Value = newReputation;
        Debug.Log($"增加声望: {amount}, 当前声望: {newReputation}");

        while (Reputation.Value >= NextLevelReputation.Value){
            Upgrade();
        }
        while (Reputation.Value < 0){
            if (Level.Value > 1){
                Downgrade();
            }
            else{
                Reputation.Value = 0;
                Debug.Log($"声望不能为负数，已重置为0");
                break;
            }
        }

        this.SendEvent(new ReputationChangedEvent(amount));
    }
    
    private void Upgrade(){
        Level.Value++;
        Reputation.Value = Reputation.Value - NextLevelReputation.Value;
        NextLevelReputation.Value = Level.Value * 2 + 8;
        Debug.Log($"升级到等级: {Level.Value}, 下一级声望: {NextLevelReputation.Value}");
    } 
    private void Downgrade(){
        Level.Value--;
        Reputation.Value = Reputation.Value + NextLevelReputation.Value;
        NextLevelReputation.Value = Level.Value * 2 + 8;
        Debug.Log($"降级到等级: {Level.Value}, 下一级声望: {NextLevelReputation.Value}");
    }
    #endregion
    #region 角色部分
    public PlayerCharacter ChoosePC(string id)
    {
        PCData data = this.GetSystem<IDataSystem>().GetPCData(id);
        if (data == null)
        {
            Debug.LogError($"玩家角色 {id} 不存在");
            return null;
        }
        PlayerCharacter playerCharacter = new PlayerCharacter(data);
        // this.SendEvent(new PCChosenEvent(playerCharacter));
        return playerCharacter;
    }
    /// <summary>
    /// 初始化玩家角色
    /// </summary>
    /// <param name="playerCharacter"></param>
    public void InitPC(PlayerCharacter playerCharacter)
    {
        Debug.Log($"初始化玩家角色: {playerCharacter.data.Name}");
        currentPC = playerCharacter;
        // 1. 添加基本串
        foreach (var stickID in currentPC.data.StickList)
        {
            this.GetSystem<IStickSystem>().AddStickToRepository(stickID);
        }
        // 2. 添加初始吉祥物
        foreach (var mascotID in currentPC.data.MascotList)
        {
            this.GetSystem<MascotSystem>().AddMascot(mascotID);
        }

        // 3. 添加基础卡牌
        // 获取第一张卡牌的ID
        string cardID = this.GetSystem<IDataSystem>().GetAllCardData().Count.ToString();
        for (int i = 0; i < 10; i++)
        {
            this.GetSystem<ICardSystem>().AddCardToRepository(cardID);
        }

        // 5. 添加初始被动技能
        foreach (var se in currentPC.data.SEs)
        {
            Debug.Log($"添加角色被动技能: {se.GetType().Name}");
            this.GetSystem<IGASystem>().ApplySE(currentPC, se);
        }

        // 5. 创建主动技能
    }

    private void LoadPC(PlayerCharacter playerCharacter)
    {
        currentPC = playerCharacter;
        Debug.Log($"加载玩家角色: {currentPC.data.Name}");

        // 1. 注册被动技能：
        foreach (var se in currentPC.data.SEs)
        {
            Debug.Log($"注册角色被动技能: {se.GetType().Name}");
            this.GetSystem<IGASystem>().ApplySE(currentPC, se);
        }
    }

    private void ClearPC(){
        if (currentPC == null) return;
        // 1. 注销所有被动技能
        foreach (var se in currentPC.data.SEs)
        {
            Debug.Log($"注销角色被动技能: {se.GetType().Name}");
            this.GetSystem<IGASystem>().RemoveSE(currentPC, se);
        }
        Debug.Log($"注销玩家角色{currentPC.data.Name}");
        currentPC = null;

    }

    #endregion
}