using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ICustomerTag{
    string name { get; }
    void Execute(DealContext context);
    List<bool> Preview(DealContext context);
    public List<CustomerCGA> CustomerActionCGAs { get; }
}

public class CustomerTag : ICustomerTag, ICanGetSystem, ICanSendEvent
{
    public CustomerTagData customerTagData;
    public string name => customerTagData.Name;
    public List<TagCGA> tagCGAs;
    public List<CustomerCGA> CustomerCGAs;
    public List<CustomerCGA> CustomerActionCGAs => CustomerCGAs.ToList();
    public CustomerTag(CustomerTagData customerTagData){
        this.customerTagData = customerTagData;
        this.tagCGAs = new List<TagCGA>();
        foreach (var tagCGA in customerTagData.TagCGAs){
            this.tagCGAs.Add(new TagCGA(tagCGA));
        }
        this.CustomerCGAs = new List<CustomerCGA>();
        foreach (var customerCGA in customerTagData.ActionCGAs){
            this.CustomerCGAs.Add(new CustomerCGA(customerCGA));
        }
    }
    public void Execute(DealContext context)
    {
        List<bool> previewResults = Preview(context);
        if (previewResults.Any(x => x)){
            // 1. 发送事件，先播放动画（而且不是实时，必须要异步延迟）
            this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
                this.SendEvent(new TagExecuteEvent(this, previewResults));
            }));

            // 1.1 阻塞动画系统（在前置动画播放完之前，不会执行后续动画）
            this.GetSystem<IAnimationSystem>().Append(new EventTriggerAnimTask());
        }

        // 2. 实际执行
        List<bool> results = new List<bool>();
        foreach (var cga in tagCGAs){

            // 2.1 绑定一下关系
            cga.Cga.Actions.ForEach(x => x.SetRelation(previewResults));
            
            // 2.2 执行
            this.GetSystem<IGASystem>().ApplyCGA(context.Customer, cga.Cga, new List<object>{context});
        }

        int index = results.FindIndex(x => x == true);
        if (index == -1){
            if (tagCGAs.Count == 0){
                return;
            }
            else{
                Debug.Log($"【CustomerTag】{name} 未触发");
            }
        }
        else{
            Debug.Log($"【CustomerTag】{name} 触发, {tagCGAs[index].CDDescription}");
        }
    }
    
    // 不实际执行，只返回是否满足条件的结果
    public List<bool> Preview(DealContext context)
    {
        List<bool> results = new List<bool>();
        foreach (var cga in tagCGAs){


            // 1. 检测单个是否触发
            bool result = cga.Cga.Conditions.All(x => x.Evaluate(context.Customer, new List<object>{context}));
            
            // 2. 添加至最终结果
            results.Add(result);
        }
        return results;
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}


