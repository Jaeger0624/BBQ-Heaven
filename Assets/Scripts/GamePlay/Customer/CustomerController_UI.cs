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
    [LabelText("顾客视图")]
    [SerializeField] public CustomerView customerView;
    [SerializeField] private TextMeshProUGUI waitingCustomerText;
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
    }

    void Update()
    {
        UpdateWaitingCustomerText();
    }

    private void OnAddCustomer(AddCustomerEvent e){
        if (customerView.currentCustomer == null){
            ChooseCurrentCustomer(e.customers.FirstOrDefault());
        }
    }
    private void OnRemoveCustomer(RemoveCustomerEvent e){
        // 如果当前有顾客，并且即将离开的顾客在移除列表中，则选择下一个正在点餐的顾客
        if (customerView.currentCustomer != null && e.customers.Contains(customerView.currentCustomer)){
            Customer cus = customerSystem.OrderingCustomers.FirstOrDefault();
            if (cus != null){
                ChooseCurrentCustomer(cus);
            }
            else{
                ClearView();
            }
        }
        // 播放移除动画


        // 尝试选择当前第一位顾客作为当前顾客
        Customer customer = customerSystem.OrderingCustomers.FirstOrDefault();
        if (customer != null){
            ChooseCurrentCustomer(customer);
        }
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