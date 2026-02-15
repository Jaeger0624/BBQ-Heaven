// 玩家动作事件
using QFramework;
using cfg;
public class PlayerActionEvent : AbstractEvent{
    public PlayerActionType actionType;
    public PlayerActionEvent(PlayerActionType actionType){
        this.actionType = actionType;
    }
}