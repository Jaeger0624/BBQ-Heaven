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
}
