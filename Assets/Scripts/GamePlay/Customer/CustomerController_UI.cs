using System.Collections.Generic;
using QFramework;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using TMPro;
using MoreMountains.Feedbacks;

public class CustomerController_UI : MonoBehaviour, IController
{
    public AnimQueue customerAnimQueue = AnimQueue.Default; // 顾客动画队列(先设置为默认队列)
    ICustomerSystem customerSystem => this.GetSystem<ICustomerSystem>();
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [LabelText("迷你视图")]
    [SerializeField] private GameObject customerMiniViewPrefab;
    [LabelText("迷你视图容器")]
    [SerializeField] private Transform customerMiniViewContainer;
    [LabelText("顾客视图")]
    [SerializeField] public CustomerView customerView;
    [SerializeField] private TextMeshProUGUI waitingCustomerText;
    // 顾客视图字典
    private Dictionary<string, CustomerMiniView> customerMiniViews = new Dictionary<string, CustomerMiniView>();

    [Header("Feedback")]
    [SerializeField] private MMF_Player showFeedback;
    [SerializeField] private MMF_Player hideFeedback;
    void Start()
    {
        this.RegisterEvent<ChangePanelEvent>(OnChangePanel).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    void OnEnable(){
        // 1. 注册添加顾客事件 -> 添加顾客视图
        this.RegisterEvent<AddCustomerEvent>(OnAddCustomer);
        // 2. 注册移除顾客事件 -> 移除顾客视图
        this.RegisterEvent<RemoveCustomerEvent>(OnRemoveCustomer);
        // 结束结算
        this.RegisterEvent<退出日结算_Event>(OnExitDaySettleEvent);
    }
    void OnDisable(){
        this.UnRegisterEvent<AddCustomerEvent>(OnAddCustomer);
        this.UnRegisterEvent<RemoveCustomerEvent>(OnRemoveCustomer);
        this.UnRegisterEvent<退出日结算_Event>(OnExitDaySettleEvent);
    }
    private void OnExitDaySettleEvent(退出日结算_Event e){
        ClearView();
        // 销毁所有迷你视图
        foreach (var customerMiniView in customerMiniViews.Values){
            GameObject.Destroy(customerMiniView.gameObject);
        }
        customerMiniViews.Clear();
    }

    void Update()
    {
        UpdateWaitingCustomerText();
    }

    private void OnAddCustomer(AddCustomerEvent e){
        if (customerView.currentCustomer == null){
            ChooseCurrentCustomer(e.customers.FirstOrDefault());
        }
        CreateCustomerView(e.customers);
    }
    private void OnRemoveCustomer(RemoveCustomerEvent e){
        // 如果当前有顾客，并且即将离开的顾客在移除列表中，则选择下一个正在点餐的顾客
        if (customerView.currentCustomer != null && e.customers.Contains(customerView.currentCustomer)){
            Customer cus = customerMiniViews.Values.FirstOrDefault(view => view.customer.state == CustomerState.Ordering)?.customer;
            if (cus != null){
                ChooseCurrentCustomer(cus);
            }
            else{
                ClearView();
            }
        }
        // 播放移除动画
        RemoveCustomerView(e.customers);
        // 尝试选择当前第一位顾客作为当前顾客
        Customer customer = customerMiniViews.Values.FirstOrDefault()?.customer;
        if (customer != null){
            ChooseCurrentCustomer(customer);
        }
    }
    // 传入顾客实例，创建顾客视图
    public void CreateCustomerView(List<Customer> customers){
        if (customerMiniViewContainer == null || customerMiniViewPrefab == null){
            Debug.LogError("迷你视图容器或迷你视图预制体未设置");
            return;
        }

        this.GetSystem<IAnimationSystem>().Append(ShowMini(customers), customerAnimQueue);
        this.GetSystem<IAnimationSystem>().Play(customerAnimQueue);
    }

    public void RemoveCustomerView(List<Customer> customers){
        // 销毁迷你视图
        List<CustomerMiniView> customerMiniViewsToRemove = new List<CustomerMiniView>();
        foreach (var customer in customers){
            if (!customerMiniViews.TryGetValue(customer.guid, out CustomerMiniView customerMiniView)) return;
            customerMiniViewsToRemove.Add(customerMiniView);
            customerMiniViews.Remove(customer.guid);
        }

        List<IAnimTask> animTasks = new List<IAnimTask>();
        foreach (var customerMiniView in customerMiniViewsToRemove){
            animTasks.Add(RemoveCustomerAnim(customerMiniView.transform));
        }

        // 延迟销毁，创建并行动画任务
        IAnimTask parallelAnimTask = new ParallelAnimTask(animTasks);
        this.GetSystem<IAnimationSystem>().Append(parallelAnimTask, customerAnimQueue);
        this.GetSystem<IAnimationSystem>().Play(customerAnimQueue);
    }

    private IAnimTask RemoveCustomerAnim(Transform transform){
        // 缩放动画（不使用TimeScale）
        SequenceAnimTask sequenceAnimTask = new SequenceAnimTask(new List<IAnimTask>{
            new ActionAnimTask(UpdateView),
            new TweenAnimTask(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutSine).SetUpdate(true)),
            new ActionAnimTask(() => {
                GameObject.Destroy(transform.gameObject);
            }),
            new DelayAnimTask(0.15f, true)
        });
        return sequenceAnimTask;
    }

    private IAnimTask ShowMini(List<Customer> customers){
        ActionAnimTask actionAnimTask = new ActionAnimTask(() => {
            if (customerMiniViewContainer == null || customerMiniViewPrefab == null){
                Debug.LogError("迷你视图容器或迷你视图预制体未设置");
                return;
            }
            List<IAnimTask> animTasks = new List<IAnimTask>();
            foreach (var customer in customers){
                // 创建迷你视图
                CustomerMiniView customerMiniView = GameObject.Instantiate(customerMiniViewPrefab).GetComponent<CustomerMiniView>();
                customerMiniView.gameObject.SetActive(false);
                customerMiniView.transform.SetParent(customerMiniViewContainer, true);
                customerMiniView.Init(customer);
                customerMiniView.OnClick += () => {
                    ChooseCurrentCustomer(customer);
                };
                // 获取原始缩放比例
                float originScale = customerMiniView.transform.lossyScale.x;
                customerMiniView.transform.localScale = Vector3.zero;
                customerMiniViews.Add(customer.guid, customerMiniView);


                // 延迟缩放动画的创建
                IAnimTask sequenceAnimTask = new SequenceAnimTask(new List<IAnimTask>{
                    new ActionAnimTask(() => {
                        customerMiniView.gameObject.SetActive(true);
                    }),
                    new TweenAnimTask(customerMiniView.transform.DOScale(new Vector3(originScale, originScale, originScale), 0.5f).SetEase(Ease.OutSine).SetUpdate(true)),
                    new ActionAnimTask(UpdateView),
                    new DelayAnimTask(0.2f, true),
                });
                animTasks.Add(sequenceAnimTask);
            }
            IAnimTask parallelAnimTask = new ParallelAnimTask(animTasks);
            this.GetSystem<IAnimationSystem>().AddFirst(parallelAnimTask, customerAnimQueue);
        });

        return actionAnimTask;
    }
    private void ChooseCurrentCustomer(Customer customer){
        if (customer == null){
            customerView.Bind(null);
            return;
        }
        if (customer.state == CustomerState.Leaved){
            Debug.LogError("选择当前顾客时，顾客已经离开");
            customerView.Bind(null);
            return;
        }
        // Debug.Log("设置当前顾客: " + customer.name);
        customerView.Bind(customer);
    }
    private void UpdateWaitingCustomerText(){
        int waitingCustomerCount = customerSystem.GetAmount(CustomerState.Waiting);
        waitingCustomerText.text = $"Waiting: {waitingCustomerCount}";
    }

    private void ClearView() => customerView.Bind(null);
    private void UpdateView() => customerView.UpdateVisual();

    private void OnChangePanel(ChangePanelEvent evt){
        if (evt.newPanel == ProcessPanel.Customer){
            Show();
        }else{
            Hide();
        }
    }
    private void Hide(){
        hideFeedback.PlayFeedbacks();
    }

    private void Show(){
        showFeedback.PlayFeedbacks();
    }
}