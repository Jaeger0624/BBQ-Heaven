using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

namespace cfg{
    public partial class GetCustomerInfo : ICanGetSystem{
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;
        public List<Customer> GetCustomers(object sender, List<object> param){
            List<Customer> customers = new List<Customer>();
            // 根据策略获取顾客
            switch (Strategy){
                case GetCustomerStrategy.选取:
                    Customer customer = param.First(x => x is Customer) as Customer;
                    if (customer == null){
                        Debug.LogError("获取顾客时，顾客为空");
                        return null;
                    }
                    customers.Add(customer);
                    break;
                case GetCustomerStrategy.随机:
                    customers.AddRange(this.GetSystem<ICustomerSystem>().OrderingCustomers);
                    break;
                case GetCustomerStrategy.随机除自己:
                    Customer self = sender as Customer;
                    if (self == null){
                        Debug.LogError("获取顾客时，自己为空");
                        return new List<Customer>();
                    }
                    customers.AddRange(this.GetSystem<ICustomerSystem>().OrderingCustomers);
                    customers.RemoveAll(c => c.guid == self.guid);
                    break;
                default:
                    Debug.LogError("获取顾客时，未实现的策略: " + Strategy);
                    return new List<Customer>();
            }
            Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();

            // 抽取指定数量的顾客
            int amount = Value.GetValue(sender, param);
            if (amount >= customers.Count) return customers;
            else return rng.PickMany(customers, amount);
        }
    }
}