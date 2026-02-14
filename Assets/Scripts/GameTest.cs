using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using cfg;
using QFramework;
using Reflex.Attributes;
using UnityEngine;

public class GameTest : MonoBehaviour, IController, ICanSendEvent{
    
    [Inject]
    IAudioService audioService;
    private int selected = 0;
    private bool showLayout = false;
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    IEnumerator Start()
    {
        yield return null;
#if UNITY_EDITOR
        // 等0.2秒后开始新游戏
        yield return new WaitForSecondsRealtime(0.25f);
        
        if (!this.GetSystem<IProcessSystem>().GameStarted){
            LogKit.I("<color=yellow>【GameTest】开始新游戏</color>");
            DifficultyData difficultyData = this.GetSystem<IDataSystem>().GetDifficultyData(0);
            LevelData levelData = this.GetSystem<IDataSystem>().GetLevelData("1");
            this.GetSystem<IProcessSystem>().StartNewGame(new NewGameInfo("1", Random.Range(0, 1000000), difficultyData, levelData));
        }
        
#endif
        // 播放背景音乐
        AudioManager.Instance.AudioService.PlayBGM("1");

        this.GetSystem<IAnimationSystem>().Play(AnimQueue.Default);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.M)){
            audioService.Play("Score 2", 0.5f, 0.1f);
        }

        // 按 N 键推进流程状态
        if (Input.GetKeyDown(KeyCode.N))
        {
            this.SendEvent(new ProcessMoveNextEvent());
            LogKit.I("【GameTest】手动推进流程状态");
        }
    }
#if UNITY_EDITOR
    void OnGUI()
    {
        // 在屏幕上方正中间显示GameState
        GUILayout.BeginArea(new Rect(Screen.width/2 - 150, 0, 300, 40), GUI.skin.box);
        ShowGameState();
        GUILayout.EndArea();

        // 字体稍微大一些
        // GUI.skin.label.fontSize = 20;
        selected = GUILayout.SelectionGrid(selected, new string[]
        {
            "基础信息",
            "吉祥物测试",
            "顾客测试",
            "烤串和食材测试",
            "时间测试",
            "PC信息测试",
            "种子测试",
            "GA测试",
            "卡牌测试",
            "教程测试"
        }, 8);
        // GUI.skin.label.fontSize = 16;

        showLayout = GUILayout.Toggle(showLayout, "显示布局");
        GUILayout.Space(10);
        if (!showLayout) return;
        GUILayout.BeginArea(new Rect(0, 100, 150, 1000), GUI.skin.box);
        // 背景色
        
        // 根据选择显示不同的测试内容
        switch (selected)
        {
            case 0:
                ShowBaseInfo();
                break;
            case 1:
                MascotTest();
                break;
            case 2:
                CustomerTest();
                break;
            case 3:
                StickAndFoodTest();
                break;
            case 4:
                TimeTest();
                break;
            case 5:
                PCInfoTest();
                break;
            case 6:
                SeedTest();
                break;
            case 7:
                GameActionTest();
                break;
            case 8:
                CardTest();
                break;
            case 9:
                GuideTest();
                break;
        }
        GUILayout.EndArea();

    }
#endif

# region 测试按钮
    private void MascotTest()
    {
        if (GUILayout.Button("添加一个吉祥物1(不可叠层)")){
            this.GetSystem<IMascotSystem>().AddMascot("1");
        }
        if (GUILayout.Button("添加一个吉祥物2(可叠层)")){
            this.GetSystem<IMascotSystem>().AddMascot("2");
        }
        if (GUILayout.Button("移除一个吉祥物")){
            this.GetSystem<IMascotSystem>().RemoveMascot("1");
        }
        if (GUILayout.Button("移除所有吉祥物")){
            this.GetSystem<IMascotSystem>().RemoveAllMascots();
        }
    }

    private void CustomerTest(){
        GUILayout.Label($"正在点餐的顾客数量: {this.GetSystem<ICustomerSystem>().GetAmount(CustomerState.Ordering)}");
        GUILayout.Label($"已离开的顾客数量: {this.GetSystem<ICustomerSystem>().GetAmount(CustomerState.Leaved)}");
        if (GUILayout.Button("添加一个立刻会到的顾客")){
            this.GetSystem<ICustomerSystem>().CreateCustomer(1);
        }
        if (GUILayout.Button("刷新顾客")){
            // 让当前顾客离开
            List<Customer> customers = this.GetSystem<ICustomerSystem>().OrderingCustomers.ToList();
            this.GetSystem<ICustomerSystem>().LeaveCustomer(customers);
            this.GetSystem<ICustomerSystem>().CreateCustomer(3);
        }
    }

    private void StickAndFoodTest(){
        GUILayout.Label($"空格子数量: {this.GetSystem<IBoardSystem>().GetEmptyCells().Count}");
        if (GUILayout.Button("分数+100")){
            this.GetSystem<IScoreSystem>().ChangeScore(100);
        }
        if (GUILayout.Button("分数翻倍")){
            this.GetSystem<IScoreSystem>().ChangeScore(this.GetSystem<IScoreSystem>().Score);
        }
        if (GUILayout.Button("添加一根烤串")){
            this.GetSystem<IStickSystem>().AddStickToRepository("1");
        }
        if (GUILayout.Button("重置烤串")){
            this.GetSystem<IStickSystem>().ResetCurrentSticks();
        }
        if (GUILayout.Button("设置烧烤计算器为食材基础值逐个加")){
            this.GetSystem<IBBQSystem>().SetCalculator(new BBQCalculator_食材基础值逐个加());
        }
        if (GUILayout.Button("随机移动1个食材实例")){
            List<FoodInstance> foodInstances = this.GetSystem<IFoodSystem>()
                .GetFoodInstances().Values
                .Where(x => x.state == FoodInstanceState.棋盘上)
                .ToList();
            List<BoardCell> emptyCells = this.GetSystem<IBoardSystem>().GetEmptyCells();
            Vector2Int randomPosition = this.GetSystem<IRngSystem>().GetSubRng<IBoardSystem>().PickOne(emptyCells).position;
            FoodInstance foodInstance = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>().PickOne(foodInstances);
            this.GetSystem<IBoardEntitySystem>().Mover.PlaceEntity(foodInstance, this.GetSystem<IBoardSystem>().GetCell(randomPosition));
        }
        if (GUILayout.Button("随机移动所有食材实例")){
            List<BoardEntity> entities = this.GetSystem<IFoodSystem>()
                .GetFoodInstances().Values
                .Where(x => x.state == FoodInstanceState.棋盘上)
                .Select(x => x as BoardEntity)
                .ToList();

            this.GetSystem<IBoardEntitySystem>().Mover.RandomPlaceEntities(entities);
        }
    }

    private void ShowBaseInfo(){

        GUILayout.Label("当前是否在测试模式: " + this.GetSystem<IProxySystem>().isTesting);

        GUILayout.Label("=== 动画系统播放状态 ===");
        foreach (var key in this.GetSystem<IAnimationSystem>().IsPlaying.Keys)
        {
            GUILayout.Label($"动画系统播放状态: {key} - {this.GetSystem<IAnimationSystem>().IsPlaying[key]}");
            GUILayout.Label($"动画系统队列长度: {key} - {this.GetSystem<IAnimationSystem>().Queue[key].Count}");
        }
        GUILayout.Label($"当前时间缩放: {Time.timeScale}");
        
        // 流程测试按钮
        GUILayout.Space(10);
        GUILayout.Label("=== 流程测试 ===");

        if (GUILayout.Button("推进流程 (N键)"))
        {
            this.SendEvent(new ProcessMoveNextEvent());
            LogKit.I("【GameTest】手动推进流程状态");
        }

        if (GUILayout.Button("添加一个新状态到根状态")){
            this.GetSystem<IProcessSystem>().AddNewStateToRoot(new GameState_经营日());
        }

        if (this.GetSystem<IProcessSystem>().RootState == null) return;
        GUILayout.Label($"根状态子状态数量: {this.GetSystem<IProcessSystem>().RootState.SubStates.Count}");
        StringBuilder sb = new StringBuilder();
        sb.Append("根状态子状态: ");
        foreach (var state in this.GetSystem<IProcessSystem>().RootState.SubStates) sb.Append($"{state.GetType().Name}, ");
        GUILayout.Label(sb.ToString());

        if (GUILayout.Button("阻塞动画系统")){
            this.GetSystem<IAnimationSystem>().Append(new EventTriggerAnimTask());
        }

        if (GUILayout.Button("推进动画系统")){
            this.SendEvent(new TriggerAnimEvent(0f));
        }
    }

    private void ShowGameState(){
        IProcessSystem processSystem = this.GetSystem<IProcessSystem>();
        if (processSystem == null) return;
        IGameState state = processSystem.CurrentActiveState;
        if (state == null){
            GUILayout.Label("当前尚未进入任何状态", GUILayout.Width(300), GUILayout.Height(50));
            return;
        }
        List<IGameState> stateList = new List<IGameState>();
        while (state != null){
            stateList.Add(state);
            state = state.ParentState;
        }
        stateList.Reverse();
        string stateName = string.Join(" -> ", stateList.Select(x => x.GetType().Name));

        GUILayout.Label($"当前状态: {stateName}", GUILayout.Width(300), GUILayout.Height(50));
    }

    private void TimeTest(){
        if (GUILayout.Button("推动1分钟")){
            this.GetSystem<ITimeSystem>().PushTimePoint(1);
        }
        if (GUILayout.Button("推动5分钟")){
            this.GetSystem<ITimeSystem>().PushTimePoint(5);
        }
        if (GUILayout.Button("推动1小时")){
            this.GetSystem<ITimeSystem>().PushTimePoint(60);
        }
        TimeInfo currentTime = this.GetSystem<ITimeSystem>().CurrentTime;
        TimeInfo targetTime = this.GetSystem<ITimeSystem>().TargetTime;

        if (currentTime == null || targetTime == null){
            GUILayout.Label("当前时间或目标时间未设置");
            return;
        }
        GUILayout.Label($"当前时间: {currentTime.hour:D2}:{currentTime.minute:D2}");
        GUILayout.Label($"目标时间: {targetTime.hour:D2}:{targetTime.minute:D2}");
    }
    private string buffId = "softness";
    private void PCInfoTest(){

        if (GUILayout.Button("增加1点声望")){
            this.GetSystem<IPCSystem>().AddReputation(1);
        }
        if (GUILayout.Button("减少1点声望")){
            this.GetSystem<IPCSystem>().AddReputation(-1);
        }
        if (GUILayout.Button("添加100金币")){
            this.GetSystem<IEconomySystem>().AddCoin(100);
        }
        if (GUILayout.Button("扣除100金币")){
            this.GetSystem<IEconomySystem>().CostCoin(100);
        }
        GUILayout.Label($"金币: {this.GetSystem<IEconomySystem>().coin.Value}");
        GUILayout.Label($"声望: {this.GetSystem<IPCSystem>().Reputation.Value}");
        GUILayout.Label($"等级: {this.GetSystem<IPCSystem>().Level.Value}");
        GUILayout.Label($"下一级声望: {this.GetSystem<IPCSystem>().NextLevelReputation.Value}");
        GUILayout.Label($"解锁的等级: {string.Join("\n", this.GetSystem<IPCSystem>().UnlockedLevels.Select(x => $"{x.Key}: {x.Value}"))}");



        GUILayout.Space(10);
        buffId = GUILayout.TextField(buffId, GUILayout.Width(100));
        if (GUILayout.Button("添加Buff")){
            this.GetSystem<IBuffSystem>().AddBuff(buffId, 1);
        }
        if (GUILayout.Button("移除Buff")){
            this.GetSystem<IBuffSystem>().RemoveBuff(buffId, 1);
        }
        GUILayout.Label("=== Buff测试 ===");
        StringBuilder sb = new StringBuilder();
        foreach (var buff in this.GetSystem<IBuffSystem>().GetBuffs()){
            sb.Append($"{buff.Value.name} - {buff.Value.GetStackNumber()}\n");
        }
        GUILayout.Label(sb.ToString());

    }
# endregion
    private Rng rng = new Rng(0);
    private int currentSeed = 0;
    private void SeedTest(){
        GUILayout.Label($"当前种子: {currentSeed}");
        string seedString = GUILayout.TextField(currentSeed.ToString());
        if (int.TryParse(seedString, out int seed)){
            currentSeed = seed;
        }
        if (GUILayout.Button("设置种子")){
            rng = new Rng(currentSeed);
            LogKit.I($"【GameTest】设置种子: {currentSeed}");
        }
        if (GUILayout.Button("获取随机Float")){
            float randomNumber = rng.NextFloat();
            LogKit.I($"【GameTest】随机Float: {randomNumber}");
        }
        if (GUILayout.Button("获取随机Bool")){
            bool randomNumber = rng.NextBool();
            LogKit.I($"【GameTest】随机Bool: {randomNumber}");
        }
    }

    private void GameActionTest(){
        // 当前状态
        GUILayout.Label($"当前状态: {this.GetSystem<IGASystem>().IsRunning}");

        if (GUILayout.Button("测试食材位移")){

            GameAction gameAction = new GA_食材位移(new GetFoodInstancesInfo(GetFoodInstanceStrategy.随机, new DV_值(1), new List<FoodInstanceState>{FoodInstanceState.棋盘上}), new GetCellInfo(GetCellStrategy.周围空位, true));
            // 1. 随机选一个食材实例
            List<FoodInstance> foodInstances = this.GetSystem<IFoodSystem>()
                .GetFoodInstances().Values
                .Where(x => x.state == FoodInstanceState.棋盘上)
                .ToList();

            FoodInstance foodInstance = this.GetSystem<IRngSystem>().GetSubRng<IFoodSystem>().PickOne(foodInstances);

            this.GetSystem<IGASystem>().TriggerReaction(gameAction, this, new List<object>{foodInstance});
        }    
    }

    private string cardId = "1";
    private void CardTest(){
        CardPile cardPile = this.GetSystem<ICardSystem>().CardPile;
        if (cardPile != null){
            GUILayout.Label($"抽牌堆数量: {this.GetSystem<ICardSystem>().CardPile.drawPile.Count}");
            GUILayout.Label($"弃牌堆数量: {this.GetSystem<ICardSystem>().CardPile.discardPile.Count}");
            GUILayout.Label($"手牌堆数量: {this.GetSystem<ICardSystem>().CardPile.handPile.Count}");
        }
        if (GUILayout.Button("初始化测试卡组")){
            for (int i = 0; i < 10; i++){
                this.GetSystem<ICardSystem>().AddCardToRepository($"1");
            }
            // 初始化CardPile
            this.GetSystem<ICardSystem>().InitCardPile();
        }
        if (GUILayout.Button("抽取n张卡片")){
            int cardCount = int.Parse(GUILayout.TextField("1", GUILayout.Width(100)));
            List<Card> cards = this.GetSystem<ICardSystem>().DrawCard(cardCount);
            LogKit.I($"【GameTest】抽取了{cardCount}张卡片: {string.Join(", ", cards.Select(x => x.name))}");
        }
        // 输入框
        cardId = GUILayout.TextField(cardId, GUILayout.Width(100)   );

        // 获取一张指定id的卡牌
        if (GUILayout.Button("获取一张指定id的卡牌")){
            this.GetSystem<ICardSystem>().AddCardToRepository(cardId);
            LogKit.I($"【GameTest】获取了一张指定id的卡牌: {cardId}");
        }
    }

    private void GuideTest(){
        if (GUILayout.Button("启动教程")){
            GuideFlow flow = SettingManager.Instance.GuideSettings.GetDefaultGuideFlow();
            this.GetSystem<IGuideSystem>().StartGuide(flow);
        }
        if (GUILayout.Button("停止教程")){
            this.GetSystem<IGuideSystem>().StopGuide();
        }
    }
}