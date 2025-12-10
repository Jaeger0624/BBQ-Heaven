/// <summary>
/// 返回单局游戏流程
/// </summary>
public interface IProcessBuilder{
    IGameState GetProcess();
}



public class ProcessMaker_单月 : IProcessBuilder{
    private readonly int dayAmount = 3;
    public IGameState GetProcess(){
        // 根状态：开始新游戏
        IGameState process = new GameState_经营月();

        for (int i = 0; i < dayAmount; i++){
            process.AddSubState(new ProcessMaker_单日().GetProcess());
        }
        return process;
    }

    public ProcessMaker_单月(int dayAmount){
        this.dayAmount = dayAmount;
    }
}

public class ProcessMaker_单日 : IProcessBuilder{
    public IGameState GetProcess(){
        IGameState process = new GameState_日();
        process.AddSubState(new GameState_日前提升());
        process.AddSubState(new GameState_经营日());
        process.AddSubState(new GameState_日结算());
        process.AddSubState(new GameState_日间活动());
        return process;
    }
}