using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GameState_开始新游戏 : AbstractGameState
{
    private NewGameInfo newGameInfo;
    public override void OnEnter()
    {
        Debug.Log("【GameState】进入开始新游戏状态");
        StartNewGame(newGameInfo);
    }
    public override void OnExit()
    {
        if (this.GetSystem<IProcessSystem>().GameStarted)
        {
            this.GetSystem<IProcessSystem>().GameOver("游戏结束");
            Debug.Log("【GameState】游戏结束！");
        }
    }
    public GameState_开始新游戏(NewGameInfo newGameInfo){
        this.newGameInfo = newGameInfo;
    }
    private void StartNewGame(NewGameInfo newGameInfo){
        // 1. 创建角色
        PlayerCharacter pc = this.GetSystem<IPCSystem>().ChoosePC(newGameInfo.pcID);

        // 2. 初始化角色（添加基本串、初始吉祥物、初始被动技能、创建主动技能）
        this.GetSystem<IPCSystem>().InitPC(pc);

        // 3. 添加初始食材
        List<FoodPack> foodPacks = new List<FoodPack>
        {
            new FoodPack("apple", 20),
            new FoodPack("beer", 20),
            new FoodPack("chicken", 20),
            new FoodPack("shrimp", 20),
            new FoodPack("cherry", 20),
        };
        this.GetSystem<IFoodSystem>().AddFoodToRepository(foodPacks);

        Debug.Log("【GameSystem】新游戏初始化完成");
        
        // 4. 初始化完成后，推进流程到下一个状态
        // 流程会自动进入第一个子状态（经营月），然后进入经营日的第一个子状态
        // this.SendEvent(new ProcessMoveNextEvent());
    }
}