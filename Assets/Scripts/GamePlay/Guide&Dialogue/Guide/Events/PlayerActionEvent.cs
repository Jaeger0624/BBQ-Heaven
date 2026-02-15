// 玩家动作事件
using QFramework;

public class PlayerActionEvent : AbstractEvent{
    public PlayerActionType actionType;
    public PlayerActionEvent(PlayerActionType actionType){
        this.actionType = actionType;
    }
}
/// <summary>
/// 玩家动作类型枚举 - 用于教程监听
/// </summary>
public enum PlayerActionType
{
    无,
    
    // ========== 通用动作 ==========
    点击任意处,
    点击按钮,
    悬停UI,
    点击继续,
    
    // ========== 时间系统 ==========
    点击时间区域,
    
    // ========== 食材系统 ==========
    抽卡操作,
    悬停食材,
    点击补充按钮,
    
    // ========== 串制系统 ==========
    选择串签,
    右键旋转,
    左键确认串制,
    点击待售区,
    
    // ========== 顾客系统 ==========
    点击顾客,
    悬停需求,
    点击出售,
    点击确认结算,
    
    // ========== 目标与声望 ==========
    点击目标区域,
    完成交易,
    打开顾客志,
    点击经营日间,
    
    // ========== 教程流程 ==========
    教程开始,
    教程完成,
}