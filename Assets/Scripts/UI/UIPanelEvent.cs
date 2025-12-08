using QFramework;

public class UIPanelEvent : AbstractEvent{
    public UIPanelType panelType;
    public UIPanelAction action;
    public UIPanelEvent(UIPanelType panelType, UIPanelAction action){
        this.panelType = panelType;
        this.action = action;
    }
}