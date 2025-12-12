using System;
using System.Collections.Generic;
using QFramework;
public class UIPanelEventSender : EventSender
{
    public List<PanelEventParam> panelEventParams;
    public override void 发送事件()
    {
        foreach (var param in panelEventParams)
        {
            UIPanelEvent evt = new UIPanelEvent(param.panelType, param.action);
            this.SendEvent(evt);
        }
    }
}
[Serializable]
public class PanelEventParam{
    public UIPanelType panelType;
    public UIPanelAction action;
}