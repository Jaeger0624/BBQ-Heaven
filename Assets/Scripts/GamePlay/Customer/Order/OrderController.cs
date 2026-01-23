using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public class OrderController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public Transform orderContainer;
    public GameObject orderViewPrefab;
    private Dictionary<string, OrderView> orderViews = new Dictionary<string, OrderView>();
    void OnEnable(){
        this.RegisterEvent<AddCustomerEvent>(OnAddCustomerEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<RemoveCustomerEvent>(OnRemoveCustomerEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<CurrentCustomerUpdateEvent>(OnCurrentCustomerUpdate).UnRegisterWhenDisabled(this);


        this.RegisterEvent<HighlightCustomersEvent>(OnHighlightCustomersEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<UnhighlightCustomersEvent>(OnUnhighlightCustomersEvent).UnRegisterWhenDisabled(this);
    }
    private void OnAddCustomerEvent(AddCustomerEvent e){
        e.customers.ForEach(customer => {
            CreateOrderView(customer);
        });
    }
    private void OnRemoveCustomerEvent(RemoveCustomerEvent e){
        e.customers.ForEach(customer => {
            RemoveOrderView(customer);
        });
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
