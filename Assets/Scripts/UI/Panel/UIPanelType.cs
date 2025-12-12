using System;
using Sirenix.OdinInspector;
[Serializable]
public enum UIPanelAction{
    Show,
    Hide,
}
[Serializable]
public enum UIPanelType{
    无,
    DialogPanel,
    KitchenPanel,
    CustomerPanel,
    EncounterPanel,
    选关界面,
    选择玩家角色面板,
    主菜单交互界面,
}