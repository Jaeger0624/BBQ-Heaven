
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



public class ContextUtility{
    public static List<object> GetContexts(){
        List<object> contexts = new List<object>();

        // 1. 添加交易上下文（若当前有交易）

        Deal currentDeal = GameArchitecture.Interface.GetSystem<IDealSystem>().GetCurrentDeal();
        CustomerSatisfaction satisfaction = GameArchitecture.Interface.GetSystem<ICustomerSystem>().Satisfaction;
        if (currentDeal != null && satisfaction != null){
            contexts.Add(new DealContext(currentDeal.bbq, currentDeal.customer, satisfaction));
        }

        // 2. 添加烧烤上下文（若当前有烧烤）
        BBQ currentBBQ = GameArchitecture.Interface.GetSystem<IBBQSystem>().GetCurrentBBQ();

        if (currentBBQ != null){
            contexts.Add(new BBQProcessContext(currentBBQ));
        }
        return contexts;
    }

}