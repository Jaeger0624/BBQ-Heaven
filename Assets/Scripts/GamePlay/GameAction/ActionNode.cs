using System.Collections.Generic;
using cfg;
using UniRx;

/// <summary>
/// 动作执行节点：记录了一次具体的动作执行实例（包含 配置 + 运行时参数）
/// </summary>
public class ActionNode
{
    private static int _globalIdCounter = 0;
    public int RuntimeID { get; private set; }

    // --- 静态数据 (配置) ---
    public GameAction ActionData { get; private set; }

    // --- 运行时数据 (现场) ---
    public object Sender { get; private set; }
    public List<object> Params { get; private set; } // 这里面装着Context

    // --- 树形关系 ---
    public ActionNode Parent { get; private set; }
    public List<ActionNode> Children { get; private set; } = new List<ActionNode>();

    // --- 状态 ---
    public bool IsFinished { get; set; } = false;

    public ActionNode(GameAction action, object sender, List<object> parameters, ActionNode parent = null)
    {
        RuntimeID = _globalIdCounter++;
        ActionData = action;
        Sender = sender;
        Params = parameters;
        Parent = parent;

        if (parent != null)
        {
            parent.Children.Add(this);
        }
    }
}