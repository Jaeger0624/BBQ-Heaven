/*
职责：
1. 创建新顾客
2. 记录顾客数据与来过的次数
3. 记录顾客好感度等阶
4. 记录顾客是否为VIP
*/
using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

[Serializable]
public class CustomerRecord{
    public MetaCustomer metaCustomer;
    public int visitCount;
    public CustomerRecord(MetaCustomer metaCustomer){
        this.metaCustomer = metaCustomer;
        this.visitCount = 0;
    }
}

public class CustomerRecorder : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private List<CustomerRecord> customerRecords = new();
    // 创建顾客元数据
    public MetaCustomer CreateMetaCustomer(bool needRecord){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        List<string> customerNames = new List<string>(){
            "明","华","玉","杰","强","伟","超","浩","洋","涛","花",
            "果","美","丽","娜","静","芳","婷","娜","丽","娜","丽"
        };
        List<string> customerSurnames = new List<string>(){
            "小"
        };
        string name = rng.PickOne(customerSurnames) + rng.PickOne(customerNames);
        MetaCustomer metaCustomer = new MetaCustomer(name);

        if (needRecord){
            AddRecord(metaCustomer);
        }
        return metaCustomer;
    }
    private void AddRecord(MetaCustomer metaCustomer){
        customerRecords.Add(new CustomerRecord(metaCustomer));
    }

    public Customer CreateCustomer(MetaCustomer metaCustomer){
        return new Customer(metaCustomer, 30, 2);
    }
}