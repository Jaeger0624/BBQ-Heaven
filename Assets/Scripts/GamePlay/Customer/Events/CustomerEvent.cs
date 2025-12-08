using System.Collections.Generic;
using QFramework;
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
#endregion