using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 流程系统接口
/// </summary>
public interface IProcessSystem : ISystem, ISavable{
    bool GameStarted { get; }
    IGameState RootState { get; }
    IGameState CurrentActiveState { get; }
    void StartNewGame(NewGameInfo newGameInfo);
    void LoadGame(GameArchive archive);
    void GameOver(string reason);
    void AddNewStateToRoot(IGameState state);
}

/// <summary>
/// 流程系统 - 系统层
/// </summary>
public class ProcessSystem : AbstractSystem, IProcessSystem
{
    public bool GameStarted { get; private set; } = false;
    /// <summary>
    /// 根状态（整个流程的入口）
    /// </summary>
    public IGameState RootState { get; private set; }
    
    /// <summary>
    /// 当前活跃的叶子状态（最深层正在运行的状态）
    /// </summary>
    public IGameState CurrentActiveState { get; private set; }
    
    private IProcessBuilder processBuilder;

    protected override void OnInit()
    {
        RootState = null;
        CurrentActiveState = null;
        processBuilder = null;
        GameStarted = false;
        // 注册状态切换事件
        this.RegisterEvent<ProcessMoveNextEvent>(OnProcessMoveNext);
    }

    protected override void OnDeinit()
    {
        this.UnRegisterEvent<ProcessMoveNextEvent>(OnProcessMoveNext);
        RootState = null;
        CurrentActiveState = null;
        processBuilder = null;
        GameStarted = false;
    }

    public void Save(GameArchive archive)
    {
        List<int> stateIndexes = new List<int>();
        if (RootState == null) return;
        // 获取所有子状态的索引
        IGameState currentState = CurrentActiveState;
        while (currentState != null)
        {
            IGameState parentState = currentState.ParentState;
            if (parentState == null) break;
            stateIndexes.Insert(0, parentState.SubStates.IndexOf(currentState));
            currentState = parentState;
        }
        archive.gameProcessData.stateIndexes = stateIndexes;

        Debug.Log("【ProcessSystem】保存游戏进程: " + string.Join(", ", stateIndexes));
    }
    public void Load(GameArchive archive)
    {
        RestoreProcess(archive.gameProcessData.stateIndexes);
    }

    private void RestoreProcess(List<int> stateIndexes)
    {
        if (RootState == null){
            Debug.LogError("【ProcessSystem】根状态为空，无法恢复游戏进程");
            return;
        }
        IGameState currentState = RootState;
        for (int i = 0; i < stateIndexes.Count; i++)
        {
            if (currentState.SubStates.Count <= stateIndexes[i])
            {
                // TODO:做错误处理逻辑
                Debug.LogError("【ProcessSystem】子状态索引超出范围，无法恢复游戏进程");
                return;
            }
            IGameState subState = currentState.SubStates[stateIndexes[i]];
            currentState.ReviseCurrentSubState(subState);

            // 切换
            currentState = currentState.SubStates[stateIndexes[i]];
        }
        currentState.OnEnter();
        UpdateCurrentActiveState();
    }
    public void LoadGame(GameArchive archive)
    {
        if (GameStarted) return;


        //TODO: 1. 需要能根据存档信息调整，先写死一个
        processBuilder = new ProcessMaker_默认(archive);
        RootState = processBuilder.GetProcess();
        GameStarted = true;


        // 2. 推动RootState
        if (RootState != null)
        {
            RootState.OnEnter();
        }
    }

    /// <summary>
    /// 开始新游戏，使用 ProcessMaker 构建流程
    /// </summary>
    public void StartNewGame(NewGameInfo newGameInfo)
    {
        if (GameStarted) return;

        // 使用默认的 ProcessMaker 构建流程
        processBuilder = new ProcessMaker_默认(newGameInfo);
        RootState = processBuilder.GetProcess();
        GameStarted = true;
        
        
        // 初始化流程：进入根状态
        if (RootState != null)
        {
            RootState.OnEnter();
            // 根状态进入后，自动进入第一个子状态（如果有）
            if (RootState.SubStates.Count > 0)
            {
                RootState.Move(); // 这会自动进入第一个子状态
            }
            // 更新当前活跃状态
            UpdateCurrentActiveState();
        }
        
        Debug.Log("【ProcessSystem】新游戏流程已构建并启动");
    }

    /// <summary>
    /// 推进流程到下一个状态（由事件触发）
    /// </summary>
    private void MoveNext()
    {
        if (RootState == null)
        {
            Debug.LogWarning("【ProcessSystem】根状态为空，无法推进流程");
            return;
        }

        // 找到当前活跃的叶子状态
        IGameState activeState = GetActiveLeafState(RootState);
        
        if (activeState == null)
        {
            Debug.LogWarning("【ProcessSystem】无法找到当前活跃状态，流程可能已结束");
            return;
        }

        // 推进当前活跃状态
        activeState.Move();
        
        // 更新当前活跃状态
        UpdateCurrentActiveState();
    }

    /// <summary>
    /// 处理状态切换事件
    /// </summary>
    private void OnProcessMoveNext(ProcessMoveNextEvent evt)
    {
        MoveNext();
    }

    /// <summary>
    /// 更新当前活跃的叶子状态
    /// </summary>
    private void UpdateCurrentActiveState()
    {
        CurrentActiveState = GetActiveLeafState(RootState);
    }

    /// <summary>
    /// 递归查找当前活跃的叶子状态（最深层正在运行的状态）
    /// </summary>
    private IGameState GetActiveLeafState(IGameState state)
    {
        if (state == null) return null;

        // 如果当前状态有子状态且已经进入了某个子状态，继续向下查找
        if (state.CurrentSubState != null)
        {
            return GetActiveLeafState(state.CurrentSubState);
        }

        // 如果当前状态没有子状态，或者还没有进入任何子状态，返回当前状态
        return state;
    }

    public void GameOver(string reason){
        if (!GameStarted) {Debug.LogWarning("【ProcessSystem】游戏未开始，无法结束"); return;}
        GameStarted = false;
        
        // if (RootState == null) return;
        // RootState.OnExit();

        // 游戏结束时，可以切换到游戏结束状态
        // 这里可以根据需要实现
        Debug.Log("【ProcessSystem】游戏结束：" + reason);

        RootState = null;
        CurrentActiveState = null;
        processBuilder = null;
        this.SendEvent(new GameOverEvent(reason));
    }

    public void AddNewStateToRoot(IGameState state)
    {
        RootState.AddSubState(state);
    }
}

#region ProcessSystem 事件

/// <summary>
/// 流程推进事件：外部系统发送此事件来通知流程系统推进到下一个状态
/// 这个事件只能逻辑层监听，不能被系统层监听
/// </summary>
public class ProcessMoveNextEvent : AbstractEvent
{
    public ProcessMoveNextEvent(){}
}

public class GameOverEvent : AbstractEvent
{
    public string reason;
    public GameOverEvent(string reason){
        this.reason = reason;
    }
}

#endregion