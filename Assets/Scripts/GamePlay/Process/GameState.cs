using System.Collections.Generic;
using QFramework;
using UnityEngine;

/// <summary>
/// 游戏状态接口，支持父子状态嵌套
/// </summary>
public interface IGameState{
    IGameState ParentState { get; }
    List<IGameState> SubStates { get; }
    IGameState CurrentSubState { get; }
    void OnEnter();
    void OnAfterActivate();
    void OnExit();
    void Move();
    void AddSubState(IGameState state);
    void RemoveSubState(IGameState state);
    void SetParentState(IGameState state);
    void ReviseCurrentSubState(IGameState state);
}

/// <summary>
/// 游戏状态抽象类，用于控制游戏流程
/// </summary>
public abstract class AbstractGameState : IGameState, IController, ICanSendEvent
{
    public IGameState ParentState { get; private set; } = null;
    public List<IGameState> SubStates { get; private set; } = new List<IGameState>();
    public IGameState CurrentSubState { get; private set; } = null;
    public void ReviseCurrentSubState(IGameState state) => CurrentSubState = state;
    private bool isMoving = false; // 防止递归调用的标志
    public abstract void OnEnter();
    public virtual void OnAfterActivate(){}
    public abstract void OnExit();

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public void AddSubState(IGameState state)
    {
        if (SubStates.Contains(state)){Debug.LogError("【GameState】子状态已存在，不进行添加"); return;}
        if (state.ParentState != null){Debug.LogError("【GameState】子状态已存在父状态，不进行添加"); return;}
        // 双向绑定
        SubStates.Add(state);
        state.SetParentState(this);
    }

    public void RemoveSubState(IGameState state)
    {
        if (!SubStates.Contains(state)){Debug.LogError("【GameState】子状态不存在，不进行删除"); return;}
        if (state.ParentState != this){Debug.LogError("【GameState】子状态父状态不匹配，不进行删除"); return;}
        SubStates.Remove(state);
        state.SetParentState(null);
    }

    public void SetParentState(IGameState state)
    {
        if (ParentState != null){Debug.LogError("【GameState】父状态已存在，不进行设置"); return;}
        if (state == null){Debug.LogError("【GameState】父状态为空，不进行设置"); return;}
        ParentState = state;
    }
    
    /// <summary>
    /// 如果当前子状态匹配给定的状态，则清空它（用于子状态退出时通知父状态）
    /// </summary>
    public void ClearCurrentSubStateIfMatches(IGameState state)
    {
        if (CurrentSubState == state)
        {
            CurrentSubState = null;
        }
    }

    public void Move()
    {
        // 防止递归调用
        if (isMoving)
        {
            Debug.LogWarning($"【GameState】状态 {GetType().Name} 正在执行 Move()，忽略递归调用");
            return;
        }
        
        isMoving = true;
        try
        {
            MoveInternal();
        }
        finally
        {
            isMoving = false;
        }
    }
    
    /// <summary>
    /// 内部 Move 实现，不检查 isMoving 标志
    /// </summary>
    private void MoveInternal()
    {
        // 1. 如果当前状态没有子状态，则退出当前状态，并通知父状态继续
        if (SubStates.Count == 0) 
        {
            OnExit(); 

            // OnExit之后，如果游戏结束了，则退出流程
            if (!this.GetSystem<IProcessSystem>().GameStarted) return;

            // 通知父状态清空当前子状态引用并继续推进
            if (ParentState != null)
            {
                ClearCurrentSubStateInParent();
                // 父状态需要继续推进，但需要避免递归
                if (ParentState is AbstractGameState parentAbstract && !parentAbstract.isMoving)
                {
                    // 通知父状态处理子状态退出后的逻辑
                    parentAbstract.HandleSubStateExited(this);
                }
            }
            return;
        }
        // 2. 如果当前状态有子状态但还没有进入任何子状态，则进入第一个子状态
        if (CurrentSubState == null && SubStates.Count > 0){
            CurrentSubState = SubStates[0];
            CurrentSubState.OnEnter();
            return;
        }
        // 3. 如果当前状态有子状态且已经进入了一个子状态，则推进子状态
        if (CurrentSubState != null){
            // 保存当前子状态的引用和索引，因为子状态可能会退出并清空父状态的引用
            IGameState currentSubStateRef = CurrentSubState;
            int currentIndex = SubStates.IndexOf(currentSubStateRef);
            
            if (currentIndex < 0)
            {
                // 当前子状态不在列表中，清空引用
                CurrentSubState = null;
                return;
            }
            
            // 先让子状态自己推进（如果子状态还有子状态，会递归处理）
            currentSubStateRef.Move();
            
            // 检查子状态是否已经退出（CurrentSubState被清空说明子状态已退出）
            // 注意：这里需要检查CurrentSubState是否为null或与保存的引用不同
            // 因为子状态退出时会调用ClearCurrentSubStateInParent()清空CurrentSubState
            if (CurrentSubState == null || CurrentSubState != currentSubStateRef)
            {
                // 子状态已退出，尝试进入下一个兄弟状态
                if (currentIndex + 1 < SubStates.Count)
                {
                    // 有下一个兄弟状态
                    CurrentSubState = SubStates[currentIndex + 1];
                    CurrentSubState.OnEnter();
                }
                else
                {
                    // 没有下一个兄弟状态，当前状态完成，退出并通知父状态
                    CurrentSubState = null;
                    OnExit();

                    if (!this.GetSystem<IProcessSystem>().GameStarted) return;

                    if (ParentState != null)
                    {
                        ClearCurrentSubStateInParent();
                        // 父状态需要继续推进，但需要避免递归
                        if (ParentState is AbstractGameState parentAbstract && !parentAbstract.isMoving)
                        {
                            // 通知父状态处理子状态退出后的逻辑
                            parentAbstract.HandleSubStateExited(this);
                        }
                    }
                }
            }
            // 如果CurrentSubState不为null且等于currentSubStateRef，说明子状态还在运行，不需要处理
        }
    }
    
    /// <summary>
    /// 处理子状态退出后的逻辑（由子状态调用，用于通知父状态继续推进）
    /// </summary>
    private void HandleSubStateExited(IGameState exitedSubState)
    {
        // 如果当前正在执行Move()，说明是子状态在Move()过程中退出的
        // 父状态在Move()中会检查CurrentSubState是否被清空，然后处理下一个兄弟状态
        // 所以这里不需要处理，直接返回
        if (isMoving)
        {
            // 父状态正在执行Move()，会在Move()中处理子状态退出
            return;
        }
        
        // 如果不在Move()中，说明是子状态退出后主动调用的（这种情况应该很少见）
        // 需要找到退出的子状态，然后进入下一个兄弟状态
        int exitedIndex = SubStates.IndexOf(exitedSubState);
        if (exitedIndex < 0)
        {
            // 退出的子状态不在列表中，可能是已经被处理过了
            return;
        }
        
        // 如果CurrentSubState已经被清空（说明子状态已退出），需要进入下一个兄弟状态
        if (CurrentSubState == null || CurrentSubState == exitedSubState)
        {
            if (exitedIndex + 1 < SubStates.Count)
            {
                // 有下一个兄弟状态
                CurrentSubState = SubStates[exitedIndex + 1];
                CurrentSubState.OnEnter();
            }
            else
            {
                // 没有下一个兄弟状态，当前状态完成，退出并通知父状态
                CurrentSubState = null;
                OnExit();
                
                if (!this.GetSystem<IProcessSystem>().GameStarted) return;
                
                if (ParentState != null)
                {
                    ClearCurrentSubStateInParent();
                    if (ParentState is AbstractGameState parentAbstract && !parentAbstract.isMoving)
                    {
                        parentAbstract.HandleSubStateExited(this);
                    }
                }
            }
        }
        // 如果CurrentSubState不为null且不等于exitedSubState，说明已经进入了下一个兄弟状态，不需要处理
    }
    
    /// <summary>
    /// 清空父状态中对当前状态的引用
    /// </summary>
    private void ClearCurrentSubStateInParent()
    {
        if (ParentState != null && ParentState is AbstractGameState parentAbstract)
        {
            parentAbstract.ClearCurrentSubStateIfMatches(this);
        }
    }
}

