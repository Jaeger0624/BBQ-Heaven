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
    private void OnEnable()
    {
        this.RegisterEvent<CreateSelectionEvent>(HandleSelection);
    }
    private void OnDisable()
    {
        this.UnRegisterEvent<CreateSelectionEvent>(HandleSelection);
    }

    [Button]
    private void ExecuteSingleTest(){

        Debug.Log("【HeadlessTest】执行单个测试");

        // 重置游戏
        GameArchitecture.ResetGame();
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

    private void HandleSelection(CreateSelectionEvent evt){
        Debug.Log("<color=purple>【HeadlessTest】代理处理选择事件: " + evt.SelectionRequest.Title + "</color>");
        // 从选项中随机选择一个
        ISelectionRequest selectionRequest = evt.SelectionRequest;
        SelectRequest request = selectionRequest.Create();
        int randomIndex = Random.Range(0, request.Contexts.Count);
        SelectionBuildContext context = request.Contexts[randomIndex];
        selectionRequest.OnSelect?.Invoke(context);

        evt.GetSubject().OnNext(Unit.Default);
        evt.GetSubject().OnCompleted();
    }
}
