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
    移动串签,
    确认制作,
    抽取食材卡,
    出售串串,
    使用卡牌,
    点击UI,
    // 后续根据游戏需求扩展...
}