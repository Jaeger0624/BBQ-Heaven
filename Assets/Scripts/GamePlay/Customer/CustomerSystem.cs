using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;


/// <summary>
/// 区分交互接口和查询接口是很重要的，因为查询接口都是幂等操作，不影响系统稳定性
/// </summary>
public interface ICustomerSystem : ISystem, ICanSendQuery{
    CustomerTime CustomerTime { get;}
    #region field
    List<MetaCustomer> ArrivedCustomers { get; }
    List<Customer> OrderingCustomers { get; }
    CustomerActionHandler CustomerActionHandler { get; }
    // 顾客日志
    CustomerRecorder Recorder { get; }
    #endregion
    #region logic
    // 生成每日顾客序列后即时添加顾客，如果isPreScheduled为true，则是在每日生成前
    public void CreateCustomer(int amount);
    // 移除顾客
    void LeaveCustomer(List<Customer> customers);

    // 设置顾客时间
    void SetCustomerTime(string customerTime);
    #endregion

    #region query
    bool IsMaxOrderAmount();
    int GetAmount(CustomerState state);
    
    /// <summary>
    /// 获取当前时段的老客概率（0-1之间）
    /// </summary>
    float GetOldCustomerChance();
    
    /// <summary>
    /// 获取当前时段的显示名称（如 "☀️ 午市"）
    /// </summary>
    string GetCustomerTimeDisplayName();
    
    /// <summary>
    /// 获取当前时段的描述文本（用于Tooltip）
    /// </summary>
    string GetCustomerTimeDescription();
    #endregion
}

public enum CustomerTime{
    午间,
    夜间,
}
public class CustomerSystem : AbstractSystem, ICustomerSystem
{
    // 每分钟来一个顾客的可能性（不能大于等于1）
    private float naturalArriveChance => SettingManager.GetSetting<GameplaySettings>().每分钟来一个顾客的可能性;
    private CustomerActionHandler customerActionHandler;
    public CustomerActionHandler CustomerActionHandler => customerActionHandler;
    public List<Customer> OrderingCustomers { get; protected set; } = new();
    public List<MetaCustomer> ArrivedCustomers { get; protected set; } = new();
    private List<Customer> leavedCustomers = new();
    public CustomerRecorder Recorder { get; protected set; } = new();
    
    private float oldCustomerChance = 0.2f; // 老顾客来店的可能性
    public CustomerTime CustomerTime { get; set; } = CustomerTime.午间;
    public void SetCustomerTime(string customerTime){
        this.CustomerTime = Enum.Parse<CustomerTime>(customerTime);
        switch (this.CustomerTime){
            case CustomerTime.午间:
                oldCustomerChance = 0.5f;
                break;
            case CustomerTime.夜间:
                oldCustomerChance = 0.2f;   
                break;
            default:
                Debug.LogError($"【CustomerSystem】不支持的顾客时间：{customerTime}");
                break;
        }
        Debug.Log($"【CustomerSystem】设置顾客时间：{this.CustomerTime}");
    }
    protected override void OnInit()
    {
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<EndDayEvent>(OnEndDay);
        this.RegisterEvent<TimeTickEvent>(OnTimeTick);

        this.SendEvent(new ClearCustomerRecordViewsEvent());
        Recorder = new CustomerRecorder();
        customerActionHandler = new CustomerActionHandler();
        customerActionHandler.Init();
    }

    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<EndDayEvent>(OnEndDay);
        this.UnRegisterEvent<TimeTickEvent>(OnTimeTick);

        customerActionHandler.Release();
    }
    public void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;

        // 设定总初始顾客数量
        int amount = SettingManager.GetSetting<GameplaySettings>().默认每日开始客户数;

        CreateCustomer(amount);
    }

    private Customer CreateSingleCustomer(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        // 老顾客来店的可能性
        if (rng.NextFloat() <= oldCustomerChance){
            return Recorder.CreateCustomer(isOld: true);
        }
        else{
            return Recorder.CreateCustomer(isOld: false);
        }
    }
    public void OnEndDay(EndDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;

        // 3. 移除所有正在点餐的顾客
        List<Customer> customers = OrderingCustomers.ToList();
        customers.ForEach(customer => {
            this.LeaveOnDayEnd(customer);
        });

        // 4. 移除所有顾客
        this.SendEvent(new RemoveCustomerEvent(customers));
        
        // 5. 移除所有已离开的顾客
        leavedCustomers.Clear();

        // 6. 移除所有已到达的顾客
        ArrivedCustomers.Clear();

    }
    public void OnTimeTick(TimeTickEvent evt)
    {
        // 先把当前正在点餐的顾客等待逻辑处理完
        OnTimeTick_处理正在点餐的顾客(evt);

        // 创建新顾客
        OnTimeTick_迎来新顾客(evt);
    }

    private void OnTimeTick_处理正在点餐的顾客(TimeTickEvent evt){
        List<Customer> customers = OrderingCustomers.ToList();
        
        // 1. 把即将走的一次性全部处理
        List<Customer> customersToLeave = customers.Where(customer => customer.isAboutToLeave).ToList();
        LeaveCustomer(customersToLeave);

        // 2. 处理正在点餐的顾客
        customers.ForEach(customer => {
            customer.ChangePatience(evt.timePoint);

            if (customer.PatienceNow.Value >= customer.PatienceMax.Value){
                customer.isAboutToLeave = true;
                this.SendEvent(new CustomerAboutToLeaveEvent(customer.guid));
            }
        });
    }

    private void OnTimeTick_迎来新顾客(TimeTickEvent evt){
        int timePoint = evt.timePoint;

        // 如果当前已经在超时状态
        if (this.GetSystem<ITimeSystem>().isTimeUp){
            Debug.Log("【CustomerSystem】当前已经在超时状态，不创建新顾客");
            return;
        }
        int amount = 0;
        for (int i = 0; i < timePoint; i++){

            if (this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>().NextFloat() <= naturalArriveChance){
                Customer customer = CreateSingleCustomer();
                amount++;
            }
        }
        if (amount == 0){
            return;
        }
        this.CreateCustomer(amount);
    }
    
    public bool IsMaxOrderAmount(){
        return OrderingCustomers.Count >= SettingManager.GetSetting<GameplaySettings>().最大同时点餐顾客数量;
    }

    public void CreateCustomer(int amount)
    {
        List<Customer> customersToCreate = new();
        for (int i = 0; i < amount; i++){
            if (!IsMaxOrderAmount()){
                Customer customer = CreateSingleCustomer();
                customer.SetState(CustomerState.Ordering);
                OrderingCustomers.Add(customer);
                customersToCreate.Add(customer);
            }
        }
        
        // 添加到已到达的顾客列表
        ArrivedCustomers.AddRange(customersToCreate.Select(customer => customer.meta).ToList());

        this.SendEvent(new AddCustomerEvent(customersToCreate));
    }

    public void LeaveCustomer(List<Customer> customers)
    {

        if (customers.Count == 0){return;}

        customers.ForEach(customer => {
            customerActionHandler.HandleCustomerAction(new List<Customer>{customer}, CustomerActionType.离开时, new List<object>());
            // 若未服务，扣除声望
            if (!customer.isServed){
                this.GetSystem<IPCSystem>().AddReputation(-customer.reputationPenalty);
            }
            // 1. 移除顾客
            OrderingCustomers.Remove(customer);
            // 2. 添加到已离开的顾客列表
            leavedCustomers.Add(customer);
            // 3. 设置顾客状态
            customer.SetState(CustomerState.Leaved);
        });
        // 5. 发送移除顾客事件，播放离开动画等
        this.SendEvent<RemoveCustomerEvent>(new RemoveCustomerEvent(customers));

        // 6. 对所有未离开的顾客，触发其他顾客离开时动作
        List<Customer> customersNotLeaved = OrderingCustomers.Where(customer => !customers.Contains(customer)).ToList();
        customerActionHandler.HandleCustomerAction(customersNotLeaved, CustomerActionType.其他顾客离开时, new List<object>());

    }

    private void LeaveOnDayEnd(Customer customer){
        OrderingCustomers.Remove(customer);
        leavedCustomers.Add(customer);
        customer.SetState(CustomerState.Leaved);
    }
    public int GetAmount(CustomerState state)
    {
        return OrderingCustomers.Count(customer => customer.state == state);
    }
    
    /// <summary>
    /// 获取当前时段的老客概率（0-1之间）
    /// </summary>
    public float GetOldCustomerChance()
    {
        return oldCustomerChance;
    }
    
    /// <summary>
    /// 获取当前时段的显示名称（如 "☀️ 午市"）
    /// </summary>
    public string GetCustomerTimeDisplayName()
    {
        return CustomerTime switch
        {
            CustomerTime.午间 => "午市",
            CustomerTime.夜间 => "夜市",
            _ => "未知时段"
        };
    }
    
    /// <summary>
    /// 获取当前时段的描述文本（用于Tooltip）
    /// </summary>
    public string GetCustomerTimeDescription()
    {
        return CustomerTime switch
        {
            CustomerTime.午间 => "老客概率: 50%\n适合积累熟客关系",
            CustomerTime.夜间 => "老客概率: 20%\n人流量大更热闹",
            _ => ""
        };
    }
}