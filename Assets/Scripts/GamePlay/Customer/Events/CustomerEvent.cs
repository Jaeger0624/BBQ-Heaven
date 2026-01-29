using System.Collections.Generic;
using QFramework;
using UnityEngine;
#region 事件
public class AddCustomerEvent : AbstractEvent{
    public List<Customer> customers = new List<Customer>();
    public AddCustomerEvent(List<Customer> customers){
        this.customers = customers;
    }
}

/// <summary>
/// 移除单一顾客事件
/// </summary>
public class RemoveCustomerEvent : AbstractEvent{
    public List<Customer> customers = new List<Customer>();
    public RemoveCustomerEvent(List<Customer> customers){
        this.customers = customers;
    }
}

public class CustomerAboutToLeaveEvent : AbstractEvent{
    public string guid;
    public CustomerAboutToLeaveEvent(string guid){
        this.guid = guid;
    }
}


public class ShowTagViewEvent : AbstractEvent{}
public class HideTagViewEvent : AbstractEvent{}
public class TagExecuteEvent : AbstractEvent{
	public CustomerTag customerTag;
	public List<bool> results;
	public TagExecuteEvent(CustomerTag customerTag, List<bool> results){
		this.customerTag = customerTag;
		this.results = results;
	}
}


// 选择当前顾客事件（由Order触发）
public class OrderClickedEvent : AbstractEvent{
    public Customer customer;
    public OrderClickedEvent(Customer customer){
        this.customer = customer;
    }
}

// 由CustomerController_UI触发
public class CurrentCustomerUpdateEvent : AbstractEvent{
    public Customer customer;
    public CurrentCustomerUpdateEvent(Customer customer){
        this.customer = customer;
    }
}
#endregion



#region 高光

public class HighlightCustomersEvent : AbstractEvent{
    public List<Customer> customers = new List<Customer>();
    public HighlightCustomersEvent(List<Customer> customers){
        this.customers = customers;
    }
}

public class UnhighlightCustomersEvent : AbstractEvent{
}

#endregion


public class ChangeCustomerPatienceEvent : AbstractEvent{
    public Customer customer;
    public int value;
    public ChangeCustomerPatienceEvent(Customer customer, int value){
        this.customer = customer;
        this.value = value;
    }
}

public class ChangeCustomerPatienceEvent_飘字 : AbstractEvent, IFloatingTextEvent{
    public Customer customer;
    public int value;
    public Transform targetTransform;
    public ChangeCustomerPatienceEvent_飘字(Customer customer, int value, Transform targetTransform){
        this.customer = customer;
        this.value = value;
        this.targetTransform = targetTransform;
    }
    public string GetDescription(){
        return $"{customer.name} 的等待值增加了 {value}";
    }
    public Vector3 GetPosition(){
        return targetTransform.position;
    }
}
