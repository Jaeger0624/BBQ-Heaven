
using System.Collections.Generic;
using QFramework;

public class DealContext{
    public readonly BBQ BBQ;
    public readonly Customer Customer;
	public readonly CustomerSatisfaction Satisfaction;

    public readonly Dictionary<string, float> OtherMultipliers;

    public DealContext(BBQ targetBBQ, Customer targetCustomer, CustomerSatisfaction targetCustomerSatisfaction){
        this.BBQ = targetBBQ;
        this.Customer = targetCustomer;
        this.Satisfaction = targetCustomerSatisfaction;
        this.OtherMultipliers = new Dictionary<string, float>();
    }
}

public class BBQProcessContext{
    public readonly BBQ targetBBQ;

    public BBQProcessContext(BBQ targetBBQ){
        this.targetBBQ = targetBBQ;
    }
}
