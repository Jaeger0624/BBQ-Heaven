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
    对话界面,
    后厨界面,
    顾客界面,
    遭遇界面,
    选关界面,
    选择玩家角色面板,
    主菜单交互界面,
    暂停界面
}