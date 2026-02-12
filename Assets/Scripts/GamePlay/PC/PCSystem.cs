using System.Collections.Generic;
using System.Linq;
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
    Dictionary<int, bool> UnlockedLevels { get; }
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
    public Dictionary<int, bool> UnlockedLevels => unlockedLevels;
    private Dictionary<int, bool> unlockedLevels = new Dictionary<int, bool>();

    protected override void OnInit()
    {
        // 初始只有第一级是解锁的
        List<ReputationData> reputationDatas = this.GetSystem<IDataSystem>().GetAllReputationData();
        foreach (var repu in reputationDatas)
        {
            unlockedLevels[repu.Rank] = false;
        }
        if (unlockedLevels.Count > 0)
        {
            unlockedLevels[1] = true;
        }
        ReputationData reputationData = reputationDatas.FirstOrDefault(x => x.Rank == 1);
        if (reputationData != null)
        {
            NextLevelReputation.Value = reputationData.TotalReputation;
        }
    }
    protected override void OnDeinit()
    {
        ClearPC();
        unlockedLevels.Clear();
    }
    public void Save(GameArchive archive)
    {
        archive.playerInfoData.playerCharacter = currentPC;
        archive.playerInfoData.reputation = Reputation.Value;
        archive.playerInfoData.nextLevelReputation = NextLevelReputation.Value;
        archive.playerInfoData.level = Level.Value;

        // 导入解锁的等级信息
        archive.playerInfoData.unlockedLevels = unlockedLevels;
    }
    public void Load(GameArchive archive)
    {
        ReloadPC(archive.playerInfoData.playerCharacter);

        Reputation.Value = archive.playerInfoData.reputation;
        NextLevelReputation.Value = archive.playerInfoData.nextLevelReputation;
        Level.Value = archive.playerInfoData.level;

        // 导入解锁的等级信息
        unlockedLevels = archive.playerInfoData.unlockedLevels;
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

        if (Reputation.Value < 0){
            Reputation.Value = 0;
            Debug.Log($"声望不能为负数，已重置为0");
        }
        this.SendEvent(new ReputationChangedEvent(amount));
    }
    
    private void Upgrade(){
        if (Level.Value >= this.GetSystem<IDataSystem>().GetAllReputationData().Count)
        {
            Debug.Log("声望等级已达到最大");
            return;
        }

        Level.Value++;

        // 计算下一级声望
        ReputationData reputationData = this.GetSystem<IDataSystem>().GetAllReputationData().FirstOrDefault(x => x.Rank == Level.Value);
        if (reputationData != null)
        {
            NextLevelReputation.Value = reputationData.TotalReputation;
        }
        else{
            Debug.LogError($"声望等级 {Level.Value} 不存在");
            return;
        }

        Debug.Log($"升级到等级: {Level.Value}, 下一级声望: {NextLevelReputation.Value}");

        if (unlockedLevels[Level.Value]) return;

        // 解锁下一级（从1->2，解锁的是2)
        unlockedLevels[Level.Value] = true;
        
        if (reputationData != null)
        {
            foreach (var se in reputationData.Actions)
            {
                this.GetSystem<IGASystem>().TriggerReaction(new CGA(se), currentPC, null);
            }
        }
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
        List<string> cardNames = new List<string>(){
            "方向移动",
            "补充",
            "交换",
            "定点爆破",
            "请离",
            "抽牌",
        };
        foreach (var cardName in cardNames)
        {
            CardData cardData = this.GetSystem<IDataSystem>().GetAllCardData().FirstOrDefault(x => x.Name == cardName);
            if (cardData == null)
            {
                Debug.LogError($"卡牌数据不存在: {cardName}");
                continue;
            }
            this.GetSystem<ICardSystem>().AddCardToRepository(cardData.ID);
        }

        // 5. 添加初始被动技能
        foreach (var se in currentPC.data.SEs)
        {
            Debug.Log($"添加角色被动技能: {se.GetType().Name}");
            this.GetSystem<IGASystem>().ApplySE(currentPC, se);
        }

        // 5. 创建主动技能
    }

    private void ReloadPC(PlayerCharacter playerCharacter)
    {
        currentPC = playerCharacter;
        Debug.Log($"加载玩家角色: {currentPC.data.Name}");

        // 1. 注册被动技能：
        foreach (var se in currentPC.data.SEs)
        {
            DebugSE(se, true);
            this.GetSystem<IGASystem>().ApplySE(currentPC, se);
        }
    }

    private void ClearPC(){
        if (currentPC == null) return;
        // 1. 注销所有被动技能
        foreach (var se in currentPC.data.SEs)
        {
            DebugSE(se, false);
            this.GetSystem<IGASystem>().RemoveSE(currentPC, se);
        }
        Debug.Log($"注销玩家角色{currentPC.data.Name}");
        currentPC = null;
    }

    private void DebugSE(SustainEffect se, bool isAdd){
        if (se is SE_监听事件 se_监听事件)
        {
            if (isAdd)
            {
                Debug.Log($"注册角色被动技能: {se.GetType().Name} - {se_监听事件.Evt}");
            }
            else
            {
                Debug.Log($"注销角色被动技能: {se.GetType().Name} - {se_监听事件.Evt}");
            }
        }
        else if (se is SE_基于线性GA se_基于线性GA)
        {
            if (isAdd)
            {
                Debug.Log($"注册角色被动技能: {se.GetType().Name} - {se_基于线性GA.OnAddAction.Count} 个GA");
            }
            else
            {
                Debug.Log($"注销角色被动技能: {se.GetType().Name} - {se_基于线性GA.OnAddAction.Count} 个GA");
            }
        }
        else
        {
            Debug.LogError($"注册角色被动技能: {se.GetType().Name} - {se.guid} 不是 SE_监听事件或 SE_基于线性GA");
        }
    }

    #endregion
}