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
    选择串签,
    点击任意处,
    点击按钮,
}