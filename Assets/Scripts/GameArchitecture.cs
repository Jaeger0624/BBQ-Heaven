using System.Text;
using QFramework;
using UnityEngine;

public class GameArchitecture : Architecture<GameArchitecture>  
{
    /// <summary>
    /// 初始化架构
    /// </summary>
    protected override void Init()
    {
        Debug.Log("【GameArchitecture】初始化");
        // 只注册事件，不在这里初始化游戏系统
        // 游戏系统的初始化应该通过 InitGame() 或 ResetGame() 显式调用
        InitGame();
    }


    public static void ResetGame(){
        // 先反初始化
        if (mArchitecture != null)
        {
            Interface.Deinit();
        }
        // 重新初始化架构（这会创建新的架构实例并调用 Init()）
        InitArchitecture();
        // InitArchitecture() 中的 Init() 会调用 InitGame()，所以这里不需要再次调用
    }

    public static void InitGame(){
        Debug.Log("【GameArchitecture】初始化游戏系统");
        StringBuilder sb = new StringBuilder("系统初始化列表");
        // 注册系统
        Interface.RegisterSystem<BlackboardSystem>(new BlackboardSystem());
        sb.Append("\nBlackboardSystem - 全局黑板系统");
        Interface.RegisterSystem<IDataSystem>(new DataSystem());
        sb.Append("\nDataSystem - 数据系统");
        Interface.RegisterSystem<IFoodSystem>(new FoodSystem());  // 食材系统
        sb.Append("\nFoodSystem - 食材系统");
        Interface.RegisterSystem<IBoardSystem>(new BoardSystem());  // 棋盘系统
        sb.Append("\nBoardSystem - 棋盘系统");
        Interface.RegisterSystem<IStickSystem>(new StickSystem());  // 烤串系统
        sb.Append("\nStickSystem - 烤串系统");
        Interface.RegisterSystem<IBBQSystem>(new BBQSystem());  // 烧烤系统
        sb.Append("\nBBQSystem - 烧烤系统");
        Interface.RegisterSystem<IScoreSystem>(new ScoreSystem());  // 分数系统
        sb.Append("\nScoreSystem - 分数系统");
        Interface.RegisterSystem<ICustomerSystem>(new CustomerSystem_新());  // 顾客系统
        sb.Append("\nCustomerSystem - 顾客系统");
        Interface.RegisterSystem<IDealSystem>(new DealSystem());  // 交易系统
        sb.Append("\nDealSystem - 交易系统");
        Interface.RegisterSystem<IPCSystem>(new PCSystem());  // 玩家角色系统
        sb.Append("\nPCSystem - 玩家角色系统");
        Interface.RegisterSystem<ITimeSystem>(new TimeSystem_默认());  // 时间系统
        sb.Append("\nTimeSystem - 时间系统");
        Interface.RegisterSystem<IGASystem>(new GASystem());  // 游戏效果系统
        sb.Append("\nGASystem - 游戏效果系统");
        Interface.RegisterSystem<IAnimationSystem>(new AnimationSystem());  // 动画系统
        sb.Append("\nAnimationSystem - 动画系统");
        Interface.RegisterSystem<IGameSystem>(new GameSystem());  // 游戏系统
        sb.Append("\nGameSystem - 游戏系统");
        Interface.RegisterSystem<IMascotSystem>(new MascotSystem());  // 吉祥物系统
        sb.Append("\nMascotSystem - 吉祥物系统");
        Interface.RegisterSystem<IRecipeSystem>(new RecipeSystem());  // 配方系统
        sb.Append("\nRecipeSystem - 配方系统");
        Interface.RegisterSystem<IProcessSystem>(new ProcessSystem());  // 流程系统
        sb.Append("\nProcessSystem - 流程系统");
        Interface.RegisterSystem<IShopSystem>(new ShopSystem());  // 商店系统
        sb.Append("\nShopSystem - 商店系统");
        Interface.RegisterSystem<IRandomSystem>(new RandomSystem());  // 随机系统
        sb.Append("\nRandomSystem - 随机系统");
        Interface.RegisterSystem<IRngSystem>(new RngSystem());  // 随机系统
        sb.Append("\nRngSystem - 随机数系统");
        Interface.RegisterSystem<IEconomySystem>(new EconomySystem());  // 经济系统
        sb.Append("\nEconomySystem - 经济系统");
        Interface.RegisterSystem<ICardSystem>(new CardSystem());  // 卡牌系统
        sb.Append("\nCardSystem - 卡牌系统");
        Interface.RegisterSystem<ISelectorSystem>(new SelectorSystem());  // 选择系统
        sb.Append("\nSelectorSystem - 选择系统");
        Interface.RegisterSystem<IDialogueSystem>(new DialogueSystem());  // 对话系统
        sb.Append("\nDialogueSystem - 对话系统");
        // Debug.Log(sb.ToString());
    }

    protected override void OnDeinit()
    {
        base.OnDeinit();
    }

    // 关闭游戏时调用
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ExitOnApplicationQuit()
    {
        Interface.Deinit();
    }
}
