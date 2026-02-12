// 玩家动作事件
using QFramework;

public class PlayerActionEvent : AbstractEvent{
    public PlayerActionType actionType;
    public PlayerActionEvent(PlayerActionType actionType){
        this.actionType = actionType;
    }
}
public enum PlayerActionType{
    无,
    选择串签
}