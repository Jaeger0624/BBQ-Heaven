using System.Collections.Generic;

public abstract class CustomerRequirement{
    public abstract bool IsMet(Customer customer, List<object> context);
    public abstract void OnMet(Customer customer, List<object> context);
}


