using UnityEngine;
using System.Text;

public static class GALogger
{
    /// <summary>
    /// 打印整棵 Action 树到控制台
    /// </summary>
    public static void LogTree(ActionNode rootNode)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<color=green>=== [GA链] 开始 ===</color>");
        
        // 递归构建字符串
        BuildLogString(rootNode, 0, sb);
        
        sb.AppendLine($"<color=green>=== [GA链] 结束 ===</color>");
        Debug.Log(sb.ToString());
    }

    private static void BuildLogString(ActionNode node, int depth, StringBuilder sb)
    {
        // 1. 缩进处理 (梯状呈现的核心)
        string indent = "";
        if (depth > 0)
        {
            // 例如：  └── 
            indent = new string(' ', (depth - 1) * 4) + "└── ";
        }

        // 2. 获取描述
        string desc = node.ActionData.GetDescription(node.Sender, node.Params);
        
        // 3. 拼接
        sb.AppendLine($"{indent}{desc}");

        // 4. 递归打印子节点
        foreach (var child in node.Children)
        {
            BuildLogString(child, depth + 1, sb);
        }
    }
}