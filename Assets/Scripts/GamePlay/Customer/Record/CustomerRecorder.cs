/*
职责：
1. 创建新顾客
2. 记录顾客数据与来过的次数
3. 记录顾客好感度等阶
4. 记录顾客是否为VIP
*/
using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;


[Serializable]
public class CustomerRecord{
    public MetaCustomer metaCustomer;
    public int visitCount;
    public int totalSpent;
    
    /// <summary>
    /// 上次来访的日期（GameSystem.CurrentDay），0表示未来过
    /// </summary>
    public int lastVisitDay;
    
    public CustomerRecord(MetaCustomer metaCustomer){
        this.metaCustomer = metaCustomer;
        this.visitCount = 0;
        this.totalSpent = 0;
        this.lastVisitDay = 0;
    }
}

public class CustomerRecorder : ICanGetSystem, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private List<CustomerRecord> customerRecords = new();
    // 创建顾客元数据
    public MetaCustomer CreateMeta(){
        MetaCustomer metaCustomer = new MetaCustomer(GetName());
        return metaCustomer;
    }
    private void AddRecord(MetaCustomer metaCustomer){
        CustomerRecord record = new CustomerRecord(metaCustomer);
        customerRecords.Add(record);

        this.SendEvent(new CreateMetaCustomerEvent(record));
    }
    /// <summary>
    /// 当顾客被服务时调用，更新顾客记录
    /// </summary>
    public void OnServeCustomer(DealContext context, DealResult result){
        // 如果已经在顾客志中
        if (!customerRecords.Any(record => record.metaCustomer == context.Customer.meta)){
            AddRecord(context.Customer.meta);
        }

        CustomerRecord record = customerRecords.FirstOrDefault(record => record.metaCustomer == context.Customer.meta);
        record.visitCount++;
        record.totalSpent += result.price;
        
        // 记录上次来访日期
        record.lastVisitDay = this.GetSystem<IGameSystem>().CurrentDay;
    }
    private Customer CreateCustomer(MetaCustomer metaCustomer){
        return new Customer(metaCustomer, 30, 2);
    }

    public Customer CreateCustomer(bool isOld){
        if (isOld){
            
            // 从老顾客中筛选出未到达的顾客
            return CreateCustomer(GetMeta(notArrived: true));
        }
        else{
            return CreateCustomer(CreateMeta());
        }
    }

    private MetaCustomer GetMeta(bool notArrived){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        List<MetaCustomer> metaCustomers = customerRecords.Select(record => record.metaCustomer).ToList();

        // 筛选出未到达的顾客
        if (notArrived){
            metaCustomers = metaCustomers.Where(meta => !this.GetSystem<ICustomerSystem>().ArrivedCustomers.Contains(meta)).ToList();
        }
        
        if (metaCustomers.Count == 0){
            Debug.LogWarning("【CustomerRecorder】顾客记录为空，无法获取顾客元数据");
            return CreateMeta();
        }
        else{
            return rng.PickOne(metaCustomers);
        }
    }

    private string GetName(){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        List<string> customerNames = new List<string>(){
            "明","华","玉","杰","强","伟","超","浩","洋","涛","花",
            "果","美","丽","娜","静","芳","婷","娜","丽","娜","丽"
        };
        List<string> customerSurnames = new List<string>(){
            "小"
        };
        string name = rng.PickOne(customerSurnames) + rng.PickOne(customerNames);
        return name;
    }
}


