using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 教程配置生成器 - 自动生成猫猫星新手教程的 ScriptableObject 配置文件
/// 使用方法：在 Unity 菜单栏选择 Tools > 教程 > 生成猫猫星教程配置
/// </summary>
public static class GuideConfigGenerator
{
    // 配置文件保存路径
    private const string CONFIG_PATH = "Assets/Resources/SO/Tutorial";

    /// <summary>
    /// 生成猫猫星教程配置（主入口）
    /// </summary>
    [MenuItem("Tools/教程/生成猫猫星教程配置")]
    public static void GenerateCatPlanetTutorial()
    {
        // 确保目录存在
        if (!Directory.Exists(CONFIG_PATH))
        {
            Directory.CreateDirectory(CONFIG_PATH);
            Debug.Log($"[GuideConfigGenerator] 创建目录: {CONFIG_PATH}");
        }

        // 生成所有步骤配置
        var steps = GenerateAllSteps();

        // 生成流程配置
        var flow = GenerateFlow(steps);

        // 保存资源
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[GuideConfigGenerator] 猫猫星教程配置生成完成！共 {steps.Count} 个步骤");
        Selection.activeObject = flow;
    }

    /// <summary>
    /// 生成所有教程步骤
    /// </summary>
    private static List<GuideStepInfo> GenerateAllSteps()
    {
        var steps = new List<GuideStepInfo>();

        // ========== 第一阶段：开场引入 ==========
        steps.Add(CreateStep("教程_1_1_首次见面",
            "哎呀，你就是那个新来的星际旅商？我是莫奇，这家店的房东~ 店铺我都给你准备好了，租金嘛...看你经营情况再说咯！",
            PlayerActionType.点击按钮,
            true));

        steps.Add(CreateStep("教程_1_2_店铺介绍",
            "这就是你的店铺了！别看它现在简陋，只要你会经营，迟早变成星际连锁！...那样我的租金也能涨涨~",
            PlayerActionType.点击任意处,
            true));

        // ========== 第二阶段：时间系统 ==========
        steps.Add(CreateStep("教程_认识时间点",
            "看到这个了吗？这是今天的'营业时间'，一共40个时间点。你做的每件事都要消耗时间，时间用完今天就结束啦！",
            PlayerActionType.点击时间区域,
            true));

        steps.Add(CreateStep("教程_时间消耗预览",
            "别担心，每次操作前都会告诉你需要多少时间。合理分配时间，是经营的第一课哦~",
            PlayerActionType.点击任意处,
            true));

        // ========== 第三阶段：食材系统 ==========
        steps.Add(CreateStep("教程_食材卡牌库",
            "这些是你的食材卡！每张卡代表一种食材。点击抽取，食材就会出现在棋盘上~",
            PlayerActionType.抽卡操作,
            false));

        steps.Add(CreateStep("教程_棋盘与食材",
            "看，食材被随机放在棋盘上了！不同食材有不同的属性——美味度和珍稀度，这两个值越高，串串越值钱！",
            PlayerActionType.悬停食材,
            false));

        steps.Add(CreateStep("教程_补充牌库",
            "食材抽完了怎么办？点击这里补充！当然，这也要消耗时间~",
            PlayerActionType.点击补充按钮,
            false));

        // ========== 第四阶段：制作串串 ==========
        steps.Add(CreateStep("教程_选择串签",
            "要做串串，先选一根签子！不同签子属性不同——木串便宜但容量小，铁串耐用但费时间...选哪个看你策略咯~",
            PlayerActionType.选择串签,
            false));

        steps.Add(CreateStep("教程_串制方向",
            "把鼠标移到棋盘上，看到预览范围了吗？左键确认，右键旋转方向！上下左右，选好角度再下手~",
            PlayerActionType.右键旋转,
            false));

        steps.Add(CreateStep("教程_确认串制",
            "范围选好了？点击确认！记住，串上食材的数量不能超过签子的容量哦~",
            PlayerActionType.左键确认串制,
            false));

        steps.Add(CreateStep("教程_串串完成",
            "看！一串美味的串串做好了！它现在在待售区，等着卖给顾客~",
            PlayerActionType.点击待售区,
            false));

        // ========== 第五阶段：顾客系统 ==========
        steps.Add(CreateStep("教程_顾客到来",
            "有顾客来了！每位顾客都有自己的喜好和要求，满足他们才能赚大钱~",
            PlayerActionType.点击顾客,
            false));

        steps.Add(CreateStep("教程_顾客需求",
            "看看这位客人想要什么...美味度至少20？珍稀度15以上？你的串串达标了吗？",
            PlayerActionType.悬停需求,
            false));

        steps.Add(CreateStep("教程_出售串串",
            "选中你的串串，点击出售！满足要求的话，会有额外奖励哦~",
            PlayerActionType.点击出售,
            false));

        steps.Add(CreateStep("教程_结算界面",
            "看！基础得分 = 美味度 × 珍稀度，再加上各种加成...这一单干得不错嘛！",
            PlayerActionType.点击确认结算,
            false));

        // ========== 第六阶段：目标与声望 ==========
        steps.Add(CreateStep("教程_每日目标",
            "每天都要达到这个目标分数，不然...嘿嘿，你的租金可就要涨了哦！",
            PlayerActionType.点击目标区域,
            true));

        steps.Add(CreateStep("教程_声望系统",
            "服务顾客可以积累声望！声望高了，会有更多客人慕名而来~",
            PlayerActionType.完成交易,
            false));

        steps.Add(CreateStep("教程_顾客志",
            "服务过的客人会记录在这里。老顾客会经常光顾，熟客还有特殊奖励呢！",
            PlayerActionType.打开顾客志,
            false));

        // ========== 第七阶段：收尾与引导 ==========
        steps.Add(CreateStep("教程_第一天结束",
            "第一天就这样啦！让我看看你的成绩...嗯，还凑合！租金嘛...今天先不涨，继续努力~",
            PlayerActionType.点击继续,
            true));

        steps.Add(CreateStep("教程_经营日间预告",
            "明天开业前，你可以在这里升级食材、买新签子、或者招募吉祥物！合理规划才能赚更多~",
            PlayerActionType.点击经营日间,
            true));

        steps.Add(CreateStep("教程_传奇食材预告",
            "对了，猫猫星传说有一种'传奇食材'，只有最厉害的老板才能找到...你感兴趣吗？嘿嘿，那就好好经营吧！",
            PlayerActionType.点击任意处,
            true));

        return steps;
    }

    /// <summary>
    /// 创建单个教程步骤
    /// </summary>
    /// <param name="stepName">步骤名称</param>
    /// <param name="guideText">教程文本（莫奇的台词）</param>
    /// <param name="triggerAction">触发动作</param>
    /// <param name="waitForClick">是否等待点击</param>
    private static GuideStepInfo CreateStep(string stepName, string guideText, PlayerActionType triggerAction, bool waitForClick)
    {
        // 检查是否已存在
        var path = $"{CONFIG_PATH}/{stepName}.asset";
        var step = AssetDatabase.LoadAssetAtPath<GuideStepInfo>(path);

        if (step == null)
        {
            // 创建新资源
            step = ScriptableObject.CreateInstance<GuideStepInfo>();
            AssetDatabase.CreateAsset(step, path);
        }

        // 设置属性
        step.name = stepName;
        step.guideText = guideText;
        step.triggerAction = triggerAction;
        step.waitForClick = waitForClick;

        EditorUtility.SetDirty(step);
        Debug.Log($"[GuideConfigGenerator] 创建步骤: {stepName}");

        return step;
    }

    /// <summary>
    /// 生成教程流程配置
    /// </summary>
    private static GuideFlow GenerateFlow(List<GuideStepInfo> steps)
    {
        var flowName = "GuideFlow_CatPlanet";
        var path = $"{CONFIG_PATH}/{flowName}.asset";

        var flow = AssetDatabase.LoadAssetAtPath<GuideFlow>(path);

        if (flow == null)
        {
            flow = ScriptableObject.CreateInstance<GuideFlow>();
            AssetDatabase.CreateAsset(flow, path);
        }

        // 设置流程属性
        flow.name = flowName;
        flow.flowID = "tutorial_cat_planet";
        flow.steps = steps;
        flow.canSkip = true;
        flow.pauseGame = false;
        flow.rewardID = "";

        EditorUtility.SetDirty(flow);
        Debug.Log($"[GuideConfigGenerator] 创建流程: {flowName}");

        return flow;
    }

    /// <summary>
    /// 清空所有教程配置（用于重新生成）
    /// </summary>
    [MenuItem("Tools/教程/清空教程配置")]
    public static void ClearAllConfigs()
    {
        if (Directory.Exists(CONFIG_PATH))
        {
            var files = Directory.GetFiles(CONFIG_PATH, "*.asset");
            foreach (var file in files)
            {
                AssetDatabase.DeleteAsset(file);
            }
            Debug.Log($"[GuideConfigGenerator] 已清空 {files.Length} 个配置文件");
        }
    }
}
