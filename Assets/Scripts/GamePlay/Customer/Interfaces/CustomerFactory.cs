using System.Collections.Generic;
using UniRx;
using UnityEngine;
// 工厂的创建逻辑可能考虑很多因素：玩家build、当前节日、已有顾客的类型（如果今日顾客已经生成了某个Tag，那么这个Tag的权重应该降低）
public interface ICustomerFactory{
    // 用于创建一个独立顾客
    Customer GenerateCustomer();
}


/// <summary>
/// 顾客工厂类 - 工具层
/// </summary>
public class CustomerFactory_默认影响权重 : ICustomerFactory{
    // 用于创建一个独立顾客
    public Customer GenerateCustomer(){

        // 生成一个随机名字
        List<string> customerNames = new List<string>(){
            "明","华","玉","杰","强","伟","超","浩","洋","涛","花",
            "果","美","丽","娜","静","芳","婷","娜","丽","娜","丽"
        };
        List<string> customerSurnames = new List<string>(){
            "小"
        };

        Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        string name = rng.PickOne(customerSurnames) + rng.PickOne(customerNames);
        
        int random = rng.NextInt(-1, 2);
        
        // 生成一个随机耐心阈值
        int patienceMax = 30 + 5 * random;

        int reputation = 2;
        
        Customer customer = new Customer(name, patienceMax, reputation);

        return customer;
    }
}
