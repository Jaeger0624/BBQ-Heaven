/// <summary>
/// 流程不涉及具体的随机逻辑，只负责组装流程，具体的随机逻辑靠状态中调用系统来实现
/// </summary>
public class ProcessMaker_默认 : IProcessBuilder{
    private NewGameInfo newGameInfo;
    private GameArchive archive;
    private bool isNewGame = true;
    public IGameState GetProcess(){
        // 根状态：开始新游戏
        IGameState process = null;
        if (isNewGame)
        {
            process = new GameState_开始新游戏(newGameInfo);
        }
        else
        {
            process = new GameState_开始新游戏(archive);
        }
        
        // 第一个经营月状态及其子状态
        // 经营月包含：经营日 -> 日结算 -> 日间活动 的循环
        IGameState monthState1 = new ProcessMaker_单月(3).GetProcess();
        IGameState monthState2 = new ProcessMaker_单月(3).GetProcess();
        IGameState monthState3 = new ProcessMaker_单月(3).GetProcess();
        process.AddSubState(monthState1);
        process.AddSubState(monthState2);
        process.AddSubState(monthState3);

        // 主界面状态（流程终点）
        IGameState mainState = new GameState_单局回顾();
        process.AddSubState(mainState);
        
        return process;
    }

    public ProcessMaker_默认(NewGameInfo newGameInfo){
        this.newGameInfo = newGameInfo;
        this.isNewGame = true;
    }

    public ProcessMaker_默认(GameArchive archive){
        this.archive = archive;
        this.isNewGame = false;
    }
}