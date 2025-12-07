using cfg;

/// <summary>
/// 只需要解析静态值和动态值即可（甚至可以不需要解析）动静态值可以延迟到顾客生成时动态解析
/// </summary>
public class CustomerTagFactory{
    public static CustomerTag CreateCustomerTag(CustomerTagData customerTagData){
        return new CustomerTag(customerTagData);
    }
}