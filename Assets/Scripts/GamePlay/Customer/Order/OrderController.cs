using System.Collections.Generic;
using System.Linq;
using QFramework;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class OrderController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public Transform orderContainer;
    public GameObject orderViewPrefab;
    [SerializeField] private TextMeshProUGUI orderCountText;
    private Dictionary<string, OrderView> orderViews = new Dictionary<string, OrderView>();
    void OnEnable(){
        // 添加顾客时，创建订单
        this.RegisterEvent<AddCustomerEvent>(OnAddCustomerEvent).UnRegisterWhenDisabled(this);
        // 移除顾客时，移除订单
        this.RegisterEvent<RemoveCustomerEvent>(OnRemoveCustomerEvent).UnRegisterWhenDisabled(this);
        // 切换当前顾客时，切换选中的订单
        this.RegisterEvent<CurrentCustomerUpdateEvent>(OnCurrentCustomerUpdate).UnRegisterWhenDisabled(this);
        // 高亮顾客时，高亮选中的订单
        this.RegisterEvent<HighlightCustomersEvent>(OnHighlightCustomersEvent).UnRegisterWhenDisabled(this);
        // 取消高亮顾客时，取消高亮选中的订单
        this.RegisterEvent<UnhighlightCustomersEvent>(OnUnhighlightCustomersEvent).UnRegisterWhenDisabled(this);
        // 改变顾客等待值时，改变选中的订单的等待值
        this.RegisterEvent<ChangeCustomerPatienceEvent>(OnChangeCustomerPatienceEvent).UnRegisterWhenDisabled(this);
    }
    private void OnChangeCustomerPatienceEvent(ChangeCustomerPatienceEvent e){
        // 跳字
        string text = null;
        if (e.value > 0){
            text = $"等待值增加了 {e.value}";
        }
        else{
            if (e.value == 0){
                text = $"等待值保持不变";
            }
            else{
                text = $"等待值减少了 {e.value}";
            }
        }
        OrderView orderView = orderViews[e.customer.guid];
        this.SendEvent(new ChangeCustomerPatienceEvent_飘字(e.customer, e.value, orderView.transform));
    }
    private void OnAddCustomerEvent(AddCustomerEvent e){
        e.customers.ForEach(customer => {
            CreateOrderView(customer);
        });

        UpdateOrderCountText();
    }
    private void OnRemoveCustomerEvent(RemoveCustomerEvent e){
        e.customers.ForEach(customer => {
            RemoveOrderView(customer);
        });

        UpdateOrderCountText();
    }
    private void OnCurrentCustomerUpdate(CurrentCustomerUpdateEvent e){
        foreach (var orderView in orderViews.Values)
        {
            if (orderView.customer == e.customer)
            {
                orderView.SetViewSelected(true);
            }
            else
            {
                orderView.SetViewSelected(false);
            }
        }
    }
    public void CreateOrderView(Customer customer){
        OrderView orderView = Instantiate(orderViewPrefab, orderContainer, false).GetComponent<OrderView>();
        orderView.Bind(customer);
        orderViews.Add(customer.guid, orderView);

        UpdateOrderCountText();

        orderView.BackgroundImage.gameObject.transform.localPosition = new Vector3(-1, 0, 0);
        orderView.BackgroundImage.gameObject.transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutBack).SetLink(orderView.BackgroundImage.gameObject);
    }
    private void UpdateOrderCountText(){
        orderCountText.text = $"订单数: {orderViews.Count} / {SettingManager.GetSetting<GameplaySettings>().最大同时点餐顾客数量}";
    }
    public void RemoveOrderView(Customer customer){
        if (orderViews.TryGetValue(customer.guid, out OrderView orderView))
        {
            Destroy(orderView.gameObject);
            orderViews.Remove(customer.guid);
        }
        else{
            Debug.LogWarning($"订单视图 {customer.guid} 不存在,但尝试移除");
        }
    }


    public void UpdateAll(){
        orderViews.Values.ToList().ForEach(orderView => {
            orderView.UpdateVisual();
        });
    }
    private void OnHighlightCustomersEvent(HighlightCustomersEvent e){
        // Debug.Log($"OnHighlightCustomersEvent: {e.customers.Count}");
        e.customers.ForEach(customer => {
            orderViews[customer.guid].Highlight();
        });
    }
    private void OnUnhighlightCustomersEvent(UnhighlightCustomersEvent e){
        // Debug.Log($"OnUnhighlightCustomersEvent: {orderViews.Count}");
        orderViews.Values.ToList().ForEach(orderView => {
            orderView.Unhighlight();
        });
    }
}
