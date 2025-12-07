using cfg;
using QFramework;
using UnityEngine;

/// <summary>
/// 玩家角色系统 - 系统层
/// 用于处理玩家角色（Player Character）相关的逻辑
/// </summary>
public interface IPCSystem : ISystem{
    PlayerCharacter ChoosePC(string id);
    void InitPC(PlayerCharacter playerCharacter);
}


public class PCSystem : AbstractSystem, IPCSystem
{
    private PlayerCharacter currentPC;
    protected override void OnInit()
    {
    }
    protected override void OnDeinit()
    {
        ClearPC();
    }
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
        // 3. 添加初始被动技能
        foreach (var se in currentPC.data.SEs)
        {
            Debug.Log($"添加角色被动技能: {se.GetType().Name}");
            this.GetSystem<IGASystem>().ApplySE(currentPC, se);
        }

        // 4. 创建主动技能

        //TODO: 5. 添加基础卡牌
        // 获取第一张卡牌的ID
        string cardID = this.GetSystem<IDataSystem>().GetAllCardData().Count.ToString();
        for (int i = 0; i < 10; i++)
        {
            this.GetSystem<ICardSystem>().AddCardToRepository(cardID);
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
}