using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UniRx;
using UnityEngine;

public interface IProxySystem : ISystem{
    bool isTesting { get;}
    void SetTesting(bool isTesting);
    void TryAction();
}
public class ProxySystem : AbstractSystem, IProxySystem{
    public bool isTesting { get; private set; } = false;
    public void SetTesting(bool isTesting){
        this.isTesting = isTesting;
        Debug.Log($"<color=purple>当前是否在测试：{this.isTesting}</color>");
        this.SendEvent(new ProxySystemEvent(isTesting));
    }
    protected override void OnInit(){
        // 处理选择事件
        this.RegisterEvent<CreateSelectionEvent>(HandleSelection);

        // 处理添加烧烤实例到仓库事件
        this.RegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);

        // 处理瞬间遭遇事件
        this.RegisterEvent<TriggerInstantEncounterEvent>(HandleInstantEncounter);
    }
    protected override void OnDeinit(){
        this.UnRegisterEvent<CreateSelectionEvent>(HandleSelection);
        this.UnRegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.UnRegisterEvent<TriggerInstantEncounterEvent>(HandleInstantEncounter);
    }


    // 处理选择事件
    private void HandleSelection(CreateSelectionEvent evt){
        if (!isTesting){
            return;
        }

        Debug.Log("<color=purple>【HeadlessTest】代理处理选择事件: " + evt.SelectionRequest.Title + "</color>");
        // 从选项中随机选择一个
        ISelectionRequest selectionRequest = evt.SelectionRequest;
        SelectRequest request = selectionRequest.Create();
        int randomIndex = Random.Range(0, request.Contexts.Count);
        SelectionBuildContext context = request.Contexts[randomIndex];
        selectionRequest.OnSelect?.Invoke(context);

        evt.GetSubject().OnNext(Unit.Default);
        evt.GetSubject().OnCompleted();
    }

    private void HandleInstantEncounter(TriggerInstantEncounterEvent evt)
    {
        if (!isTesting){
            return;
        }
        Debug.Log("<color=purple>【ProxySystem】处理瞬间遭遇事件: " + evt.instantEncounter.Name + "</color>");
        // 从选项中随机选择一个
        InstantEncounter instantEncounter = evt.instantEncounter;
        List<OptionData> options = instantEncounter.options;
        int randomIndex = Random.Range(0, options.Count);
        OptionData optionData = options[randomIndex];
        this.SendEvent(new SelectOptionEvent(optionData));
    }


    public void TryAction(){
        // 若当前有顾客
        int customerCount = this.GetSystem<ICustomerSystem>().OrderingCustomers.Count;
        if (customerCount > 0){
            Debug.Log("<color=purple>【ProxySystem】有顾客，执行顾客导向</color>");
            WhenHaveCustomer();
        }
        else{
            Debug.Log("<color=purple>【ProxySystem】没有顾客，执行打卡牌或补充食材或随便串</color>");
            WhenNoCustomer();
        }
    }

    private void WhenNoCustomer(){
        // 打卡牌或补充食材或随便串
        
        bool isSuccess = this.GetSystem<IFoodSystem>().RefreshFoodPile();
        if (!isSuccess){
            Debug.Log("<color=purple>【ProxySystem】补充食材失败，执行随便串</color>");
            // 推进直到有顾客来
            while (this.GetSystem<ICustomerSystem>().OrderingCustomers.Count == 0)
            {
                this.GetSystem<ITimeSystem>().PushTimePoint(1);
            }
        }
    }

    // 顾客导向
    private void WhenHaveCustomer()
    {
        // 1. 选择一个顾客
        Customer targetCustomer = this.GetSystem<ICustomerSystem>().OrderingCustomers.FirstOrDefault();

        // 2. 选择一个串
        SelectStick();

        // 3. 计算所有可能的烧烤结果
        List<BBQResultTemp> resultTemps = BBQResultTemp.GetAllBBQResultTemps();

        BBQResultTemp currentBest = null;
        int currentBestScore = 0;
        foreach (var resultTemp in resultTemps){
            int score = targetCustomer.GetTempScore(resultTemp);
            if (score > currentBestScore){
                currentBestScore = score;
                currentBest = resultTemp;
            }
        }
        dealRequest = new DealRequest(targetCustomer);
        // 2. 创建一个串
        if (currentBest == null){
            Debug.LogError("<color=purple>【ProxySystem】没有找到最佳烧烤结果，执行随便串</color>");
            return;
        }
        if (currentBest.foodInstances == null){
            Debug.LogError("<color=purple>【ProxySystem】没有找到最佳烧烤结果，执行随便串</color>");
            return;
        }
        if (currentBest.foodInstances.Count == 0){
            Debug.LogError("<color=purple>【ProxySystem】没有找到最佳烧烤结果，执行随便串</color>");
            return;
        }
        if (currentBest.stick == null){
            Debug.LogError("<color=purple>【ProxySystem】没有找到最佳烧烤结果，执行随便串</color>");
            return;
        }
        this.GetSystem<IBBQSystem>().FinishBBQ(currentBest.stick, currentBest.foodInstances);
    }

    // 选择串签
    private void SelectStick()
    {
        // 随机选择一个串签
        List<Stick> sticks = this.GetSystem<IStickSystem>().CurrentSticks;
        int randomIndex = Random.Range(0, sticks.Count);
        Stick stick = sticks[randomIndex];
        this.GetSystem<IStickSystem>().SetSelectedStick(stick);
    }

    DealRequest dealRequest = null;
    private void OnAddBBQToRepository(AddBBQToRepositoryEvent evt){
        if (!isTesting){
            return;
        }   
        if (dealRequest != null){
            Debug.Log("<color=purple>【ProxySystem】有交易请求，执行交易</color>");
            // 1. 立刻完成交易
            this.GetSystem<IDealSystem>().ExecuteDeal(evt.bbq, dealRequest.customer);
            dealRequest = null;
        }
        else{
            Debug.Log("<color=purple>【ProxySystem】没有交易请求，将烧烤实例添加到烧烤仓库中</color>");
        }
    }
}


public class BBQResultTemp{
    public Stick stick;
    public List<FoodInstance> foodInstances;
    public BBQResultTemp(Stick stick, List<FoodInstance> foodInstances){
        this.stick = stick;
        this.foodInstances = foodInstances;
    }

    public static List<BBQResultTemp> GetAllBBQResultTemps(){
        List<BBQResultTemp> resultTemps = new List<BBQResultTemp>();
        Stick selectedStick = GameArchitecture.Interface.GetSystem<IStickSystem>().selectedStick;
        IStickStrategy strategy = selectedStick.strategy;

        // 第一列竖
        List<Vector2Int> horizontalPositions = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
            new Vector2Int(0, 3),
            new Vector2Int(0, 4),
            new Vector2Int(0, 5),
            new Vector2Int(0, 6),
            new Vector2Int(0, 7),
        };

        // 第一行横
        List<Vector2Int> verticalPositions = new List<Vector2Int>()
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0),
            new Vector2Int(4, 0),
            new Vector2Int(5, 0),
            new Vector2Int(6, 0),
            new Vector2Int(7, 0),
        };

        IStickStrategy.directionValue = 0; // 向上
        resultTemps.AddRange(horizontalPositions.Select(x => {
            List<FoodInstance> foodInstances = strategy.GetFood(x, selectedStick);
            return new BBQResultTemp(selectedStick, foodInstances);
        }));

        IStickStrategy.directionValue = 2; // 向下
        resultTemps.AddRange(horizontalPositions.Select(x => {
            List<FoodInstance> foodInstances = strategy.GetFood(x, selectedStick);
            return new BBQResultTemp(selectedStick, foodInstances);
        }));

        IStickStrategy.directionValue = 1; // 向右
        resultTemps.AddRange(verticalPositions.Select(x => {
            List<FoodInstance> foodInstances = strategy.GetFood(x, selectedStick);
            return new BBQResultTemp(selectedStick, foodInstances);
            }));

        IStickStrategy.directionValue = 3; // 向左
        resultTemps.AddRange(verticalPositions.Select(x => {
            List<FoodInstance> foodInstances = strategy.GetFood(x, selectedStick);
            return new BBQResultTemp(selectedStick, foodInstances);
        }));


        resultTemps.RemoveAll(x => x.foodInstances.Count == 0);
        return resultTemps;
    }
}

public class DealRequest{
    public Customer customer;
    public DealRequest(Customer customer){
        this.customer = customer;
    }
}

public class ProxySystemEvent : AbstractEvent{
    public bool isTesting;
    public ProxySystemEvent(bool isTesting){
        this.isTesting = isTesting;
    }
}