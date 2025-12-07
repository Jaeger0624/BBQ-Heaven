using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ICustomerTag{
    string name { get; }
    void Execute(Customer customer, BBQ bbq);
    List<bool> Preview(Customer customer, BBQ bbq);
}

public class CustomerTag : ICustomerTag, ICanGetSystem, ICanSendEvent
{
    public CustomerTagData customerTagData;
    public string name => customerTagData.Name;
    public List<TagCGA> tagCGAs;
    public CustomerTag(CustomerTagData customerTagData){
        this.customerTagData = customerTagData;
        this.tagCGAs = new List<TagCGA>();
        foreach (var tagCGA in customerTagData.TagCGAs){
            this.tagCGAs.Add(new TagCGA(tagCGA));
        }
    }
    public void Execute(Customer customer, BBQ bbq)
    {
        List<bool> previewResults = Preview(customer, bbq);
        // 0. 创建上下文
        CustomerSatisfaction satisfaction = this.GetSystem<ICustomerSystem>().Satisfaction;
        DealContext context = new DealContext(bbq, customer, satisfaction);


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
            this.GetSystem<IGASystem>().ApplyCGA(customer, cga.Cga, new List<object>{context});
        }

        int index = results.FindIndex(x => x == true);
        if (index == -1){
            Debug.Log($"【CustomerTag】{name} 未触发");
        }
        else{
            Debug.Log($"【CustomerTag】{name} 触发, {tagCGAs[index].CDDescription}");
        }
    }
    
    // 不实际执行，只返回是否满足条件的结果
    public List<bool> Preview(Customer customer, BBQ bbq)
    {
        DealContext context = new DealContext(bbq, customer, this.GetSystem<ICustomerSystem>().Satisfaction);

        List<bool> results = new List<bool>();
        foreach (var cga in tagCGAs){


            // 1. 检测单个是否触发
            bool result = cga.Cga.Conditions.All(x => x.Evaluate(customer, new List<object>{context}));
            
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


