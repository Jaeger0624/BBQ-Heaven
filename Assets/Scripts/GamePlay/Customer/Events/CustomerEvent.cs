using System.Collections.Generic;
using QFramework;
#region 事件
public class AddCustomerEvent : IEvent{
    public List<Customer> customers = new List<Customer>();
    public AddCustomerEvent(List<Customer> customers){
        this.customers = customers;
    }
}

/// <summary>
/// 移除单一顾客事件
/// </summary>
public class RemoveCustomerEvent : IEvent{
    public List<Customer> customers = new List<Customer>();
    public RemoveCustomerEvent(List<Customer> customers){
        this.customers = customers;
    }
}

public class CustomerAboutToLeaveEvent : IEvent{
    public string guid;
    public CustomerAboutToLeaveEvent(string guid){
        this.guid = guid;
    }
}


public class ShowTagViewEvent : IEvent{}
public class HideTagViewEvent : IEvent{}
public class TagExecuteEvent : IEvent{
	public CustomerTag customerTag;
	public List<bool> results;
	public TagExecuteEvent(CustomerTag customerTag, List<bool> results){
		this.customerTag = customerTag;
		this.results = results;
	}
}
#endregion