using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GameState_开始新游戏 : AbstractGameState
{
    private NewGameInfo newGameInfo;
    private GameArchive archive;
    private bool isNewGame = true;
    public GameState_开始新游戏(NewGameInfo newGameInfo){
        this.newGameInfo = newGameInfo;
        this.isNewGame = true;
    }
    public GameState_开始新游戏(GameArchive archive){
        this.archive = archive;
        this.isNewGame = false;
    }
    public override void OnEnter()
    {
        if (isNewGame)
        {
            Debug.Log("<color=green>【GameState】进入开始新游戏状态</color>");
            StartNewGame(newGameInfo);
        }
        else
        {
            Debug.Log("<color=yellow>【GameState】加载游戏状态</color>");
            this.GetSystem<ISaveSystem>().LoadGame(archive);
        }
    }
    public override void OnExit()
    {
        if (this.GetSystem<IProcessSystem>().GameStarted)
        {
            this.GetSystem<IProcessSystem>().GameOver("游戏结束");
            Debug.Log("【GameState】游戏结束！");
        }
    }
    private void StartNewGame(NewGameInfo newGameInfo){
        // 0. 重置随机种子
        this.GetSystem<IRngSystem>().SetMainSeed(newGameInfo.seed);
        
        // 1. 创建角色
        PlayerCharacter pc = this.GetSystem<IPCSystem>().ChoosePC(newGameInfo.pcID);

        // 2. 初始化角色（添加基本串、初始吉祥物、初始被动技能、创建主动技能）
        this.GetSystem<IPCSystem>().InitPC(pc);

        // 3. 注册所有配方
        this.GetSystem<IRecipeSystem>().RegisterAllRecipes();

        // 4. 添加初始食材
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
    }
}