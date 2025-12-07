
using System.Collections.Generic;
using QFramework;

public class DealContext{
    public readonly BBQ targetBBQ;
    public readonly Customer targetCustomer;
	public readonly CustomerSatisfaction targetCustomerSatisfaction;

    public DealContext(BBQ targetBBQ, Customer targetCustomer, CustomerSatisfaction targetCustomerSatisfaction){
        this.targetBBQ = targetBBQ;
        this.targetCustomer = targetCustomer;
        this.targetCustomerSatisfaction = targetCustomerSatisfaction;

    }
}

public class BBQProcessContext{
    public readonly BBQ targetBBQ;
    public readonly List<FoodInstance> sourceFoodInstances;
    public BBQProcessContext(BBQ targetBBQ, List<FoodInstance> sourceFoodInstances){
        this.targetBBQ = targetBBQ;
        this.sourceFoodInstances = sourceFoodInstances;
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
            contexts.Add(new BBQProcessContext(currentBBQ, new List<FoodInstance>()));
        }
        return contexts;
    }

}