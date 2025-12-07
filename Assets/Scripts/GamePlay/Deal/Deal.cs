/// <summary>
/// 交易实例类 - 实例层
/// </summary>
public class Deal{
    public BBQ bbq;
    public Customer customer;
    public Deal(BBQ bbq, Customer customer){
        this.bbq = bbq;
        this.customer = customer;
    }
}