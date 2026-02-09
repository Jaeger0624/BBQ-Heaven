using cfg;
using QFramework;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

public class HeadlessTest : MonoBehaviour, IController, ICanGetSystem
{
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    void Start()
    {
        this.GetSystem<IProxySystem>().SetTesting(true);
    }

    [Button]
    private void ExecuteSingleTest(){
        // 必须是先重置游戏
        GameArchitecture.ResetGame();
        
        this.GetSystem<IProxySystem>().SetTesting(true);
        Debug.Log("【HeadlessTest】执行单个测试");

        if (!this.GetSystem<IProcessSystem>().GameStarted){
            LogKit.I("<color=yellow>【HeadlessTest】开始新游戏</color>");
            DifficultyData difficultyData = this.GetSystem<IDataSystem>().GetDifficultyData(0);
            LevelData levelData = this.GetSystem<IDataSystem>().GetLevelData("1");
            this.GetSystem<IProcessSystem>().StartNewGame(new NewGameInfo("1", Random.Range(0, 1000000), difficultyData, levelData));
        }
        else{
            Debug.LogError("【HeadlessTest】游戏已启动，无法开始新游戏");
        }
    }

    [Button]
    private void TryAction(){
        this.GetSystem<IProxySystem>().TryAction();
    }
}
