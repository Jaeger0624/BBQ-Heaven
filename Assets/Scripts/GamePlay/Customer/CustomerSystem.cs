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
    #region field
    List<Customer> OrderingCustomers { get; }
    CustomerSatisfaction Satisfaction { get; set; }
    CustomerLookMaker CustomerLookMaker { get; set; }
    CustomerActionHandler CustomerActionHandler { get; }
    #endregion
    #region logic
    // 生成每日顾客序列后即时添加顾客，如果isPreScheduled为true，则是在每日生成前
    public void CreateCustomer(List<Customer> customers);
    public void PreScheduleCustomer(ScheduleInfo scheduleInfo);
    // 排队
    void EnqueueCustomer(Customer customer);
    // 补位
    Customer DequeueCustomer();
    // 移除顾客
    void LeaveCustomer(List<Customer> customers);
    #endregion

    #region query
    bool IsMaxOrderAmount();
    int GetAmount(CustomerState state);
    // 获取预定的顾客
    List<(Customer, float)> GetPreScheduledCustomers();
    #endregion
}
public class CustomerSystem_新 : AbstractCustomerSystem
{
    // 每分钟来一个顾客的可能性（不能大于等于1）
    private float naturalArriveChance => SettingManager.GetSetting<GameplaySettings>().每分钟来一个顾客的可能性;
    private ICustomerFactory customerFactory;
    private Rng rng => this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
    private CustomerActionHandler customerActionHandler;
    public override CustomerActionHandler CustomerActionHandler => customerActionHandler;
    protected override void OnInit()
    {
        base.OnInit();  
        customerFactory = new CustomerFactory_默认影响权重();

        customerActionHandler = new CustomerActionHandler();
        customerActionHandler.Init();
    }

    protected override void OnDeinit()
    {
        base.OnDeinit();
        customerFactory = null;

        customerActionHandler.Release();
    }
    protected override void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;

        // 设定总初始顾客数量
        int amount = SettingManager.GetSetting<GameplaySettings>().默认每日开始客户数;

        // 处理预定顾客
        List<Customer> customers = preScheduledCustomers.Where(info => info.Item2 == 0).Select(info => info.Item1).ToList();
        int finalAmount = Math.Min(customers.Count, amount);
        List<Customer> customersToCreate = new();
        customersToCreate.AddRange(customers.Take(finalAmount));

        amount -= finalAmount;

        // 补充顾客（如果还有剩余）
        customersToCreate.AddRange(Enumerable.Repeat(0, amount).Select(_ => customerFactory.GenerateCustomer()));
        CreateCustomer(customersToCreate);
    }
    protected override void OnEndDay(EndDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.System)) return;


        // 1. 清除预定队列
        preScheduledCustomers.Clear();

        // 2. 清除等待队列
        while (waitingCustomers.Count > 0){
            Customer customer = waitingCustomers.Dequeue();
            customer.SetState(CustomerState.Leaved);
            leavedCustomers.Add(customer);
        }

        // 3. 移除所有正在点餐的顾客
        List<Customer> customers = OrderingCustomers.ToList();
        customers.ForEach(customer => {
            this.LeaveOnDayEnd(customer);
        });

        // 4. 移除所有顾客
        this.SendEvent(new RemoveCustomerEvent(customers));
        
        // 5. 移除所有已离开的顾客
        leavedCustomers.Clear();


    }
    protected override void OnTimeTick(TimeTickEvent evt)
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
            customer.PatienceNow.Value += evt.timePoint;

            if (customer.PatienceNow.Value >= customer.PatienceMax.Value){
                customer.isAboutToLeave = true;
                this.SendEvent(new CustomerAboutToLeaveEvent(customer.guid));
            }
        });
    }

    private void OnTimeTick_迎来新顾客(TimeTickEvent evt){
        int timePoint = evt.timePoint;
        List<Customer> customersToCreate = new();
        for (int i = 0; i < timePoint; i++){

            float currentProcess = this.GetSystem<ITimeSystem>().CurrentProcess;

            Customer scheduledCustomer = HandlePreScheduledCustomers(currentProcess);
            if (scheduledCustomer != null){
                customersToCreate.Add(scheduledCustomer);
                continue;
            }

            if (rng.NextFloat() <= naturalArriveChance){
                Customer customer = customerFactory.GenerateCustomer();
                customersToCreate.Add(customer);
            }
        }

        // 只创建一次，可能含有多个顾客
        this.CreateCustomer(customersToCreate);
    }

    private Customer HandlePreScheduledCustomers(float currentProcess){

        // 1. 将预定顾客按到达时间排序
        preScheduledCustomers.OrderBy(info => info.Item2).ToList();

        // 2. 如果没有预定顾客，则返回空
        if (preScheduledCustomers.Count == 0) return null;

        // 3. 检查是否有在当前进度之前的（理论上不该有）

        // 获取最早到达的预定顾客
        (Customer customer, float arriveTime) = preScheduledCustomers.First();

        // (例如)当前进程为0.5，到达时间为0.75，则比例为0.6666666666666666
        float ratio = currentProcess / arriveTime;
        
        // 处理后的概率 -> 0.6666 * 0.6666 = 0.4444
        float newChance = (float)Math.Pow(ratio, 2);

        if (rng.NextFloat() <= newChance){
            preScheduledCustomers.RemoveAll(info => info.Item1 == customer);
            return customer;
        }
        return null;
    }
    public override void CreateCustomer(List<Customer> customers)
    {
        if (OrderingCustomers.Any(customer => customers.Contains(customer))){ Debug.LogWarning("创建顾客时，顾客已经在正在点餐的顾客列表中"); return; }

        List<Customer> customersToCreate = new();

        customers.ForEach(customer => {
            if (IsMaxOrderAmount()){
                customer.SetState(CustomerState.Waiting);
                EnqueueCustomer(customer);
            }
            else{
                customer.SetState(CustomerState.Ordering);
                OrderingCustomers.Add(customer);
                customersToCreate.Add(customer);
            }
        });

        this.SendEvent<AddCustomerEvent>(new AddCustomerEvent(customersToCreate));
    }

    public override void LeaveCustomer(List<Customer> customers)
    {

        if (customers.Count == 0){return;}

        customers.ForEach(customer => {

            customerActionHandler.HandleCustomerAction(new List<Customer>{customer}, CustomerActionType.离开时, new List<object>());

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


        List<Customer> customersToCreate = new();
        foreach (var customer in customers){
            // 5. 因为已经腾出位置，所以可以尝试从等待队列中补位
            Customer dequeuedCustomer = DequeueCustomer();
            if (dequeuedCustomer != null){
                customersToCreate.Add(dequeuedCustomer);
            }
        }
        this.CreateCustomer(customersToCreate);
    }

    private void LeaveOnDayEnd(Customer customer){
        OrderingCustomers.Remove(customer);
        leavedCustomers.Add(customer);
        customer.SetState(CustomerState.Leaved);
    }

    public override void PreScheduleCustomer(ScheduleInfo scheduleInfo)
    {
        // 1. 获取预定时间
        float arriveTime = scheduleInfo.arriveTime;

        // 2. 合法性检测
        if (arriveTime > 1){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是到达时间大于1");
            return;
        }
        if (arriveTime < 0){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是到达时间小于0");
            return;
        }
        if (scheduleInfo.customer == null){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是顾客为空");
            return;
        }

        preScheduledCustomers.Add((scheduleInfo.customer, arriveTime));
    }


}
#region CustomerSystem抽象层
public abstract class AbstractCustomerSystem : AbstractSystem, ICustomerSystem
{
    private int maxOrderAmount = 3; // 最大同时点餐顾客数量
    public List<Customer> OrderingCustomers => orderingCustomers;
    public CustomerSatisfaction Satisfaction { get; set; }
    public CustomerLookMaker CustomerLookMaker { get; set; } = new CustomerLookMaker();
    public abstract CustomerActionHandler CustomerActionHandler { get;}
    /// <summary> 等待顾客队列，用于处理排队和填补空缺 </summary>
    protected Queue<Customer> waitingCustomers = new Queue<Customer>();
    /// <summary> 当日顾客实例字典，用于存储顾客实例 </summary>
    protected List<Customer> orderingCustomers = new List<Customer>();
    /// <summary> 已离开的顾客，用于存储已离开的顾客实例 </summary>
    protected List<Customer> leavedCustomers = new List<Customer>();

    /// <summary> 预定的顾客，用于供外部处理预定顾客，并保留顾客和到达时间
    /// float 用来表示大致的进程时间（比如预定顾客的到达时间）
    /// </summary>
    protected List<(Customer, float)> preScheduledCustomers = new List<(Customer, float)>();

    public abstract void CreateCustomer(List<Customer> customers);
    public abstract void LeaveCustomer(List<Customer> customers);
    public abstract void PreScheduleCustomer(ScheduleInfo scheduleInfo);
    public Customer DequeueCustomer(){
        if (waitingCustomers.Count == 0) return null;
        Customer customer = waitingCustomers.Dequeue();
        return customer;
    }
    public void EnqueueCustomer(Customer customer){
        if (waitingCustomers.Contains(customer)) return;
        waitingCustomers.Enqueue(customer);
    }

    public List<(Customer, float)> GetPreScheduledCustomers()
    {
        List<(Customer, float)> res = new();
        foreach (var (customer, arriveTime) in preScheduledCustomers){
            res.Add((customer, arriveTime));
        }
        preScheduledCustomers.Clear();
        return res;
    }

    public int GetAmount(CustomerState state)
    {
        switch(state){
            case CustomerState.Ordering:
                return OrderingCustomers.Count;
            case CustomerState.Waiting:
                return waitingCustomers.Count;
            case CustomerState.Leaved:
                return leavedCustomers.Count;
            default:
                Debug.LogWarning("获取顾客数量时，传入的顾客状态不合法");
                return 0;
        }
    }

    public bool IsMaxOrderAmount()
    {
        int totalCustomerCount = OrderingCustomers.Count;
        return totalCustomerCount >= maxOrderAmount;
    }

    public void PreScheduleCustomer(ScheduleInfo scheduleInfo, float arriveProgress)
    {
        Customer customer = scheduleInfo.customer;
        if (customer == null && arriveProgress == -1){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是顾客和到达时间都为空");
            return;
        }
        preScheduledCustomers.Add((customer, arriveProgress));
    }

    protected override void OnInit()
    {
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.RegisterEvent<EndDayEvent>(OnEndDay);
        this.RegisterEvent<TimeTickEvent>(OnTimeTick);
        CustomerLookMaker.Reset();
    }
    protected override void OnDeinit()
    {
        this.UnRegisterEvent<StartNewDayEvent>(OnStartNewDay);
        this.UnRegisterEvent<EndDayEvent>(OnEndDay);
        this.UnRegisterEvent<TimeTickEvent>(OnTimeTick);
    }
    protected abstract void OnStartNewDay(StartNewDayEvent evt);
    protected abstract void OnEndDay(EndDayEvent evt);
    protected abstract void OnTimeTick(TimeTickEvent evt);
}
#endregion