
using QFramework;

/// <summary>
/// 实例对象的动画播放器（如食材实例、烧烤实例等）
/// </summary>
public interface IAnimPlayer{
    IAnimTask GetAnimTask(string param);
}

// 食材实例动画播放器
public partial class FoodInstance : IAnimPlayer, ICanSendEvent{
    public IAnimTask GetAnimTask(string param){
        IAnimTask animTask = null;
        switch (param){
            case "common":
                animTask = new ActionAnimTask(() => this.SendEvent(new FoodInstanceExecuteActionEvent(this.guid)));
                break;
            default:
                return new EmptyAnimTask();
        }
        return animTask;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}

public partial class PlayerCharacter : IAnimPlayer, ICanSendEvent{
    public IAnimTask GetAnimTask(string param){
        IAnimTask animTask = null;
        switch (param){
            case "common":
                // animTask = new ActionAnimTask(() => this.SendEvent(new PCExecuteActionEvent(this.ID)));
                return new EmptyAnimTask();

            default:
                return new EmptyAnimTask();
        }
        return animTask;
    }
}