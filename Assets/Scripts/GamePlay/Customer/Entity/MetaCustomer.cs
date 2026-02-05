using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

[Serializable]
public class MetaCustomer : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public readonly string guid;
    public string name;
    public List<ICustomerTag> customerTags;
    public Sprite customerLook;
    public MetaCustomer(string name){
        this.guid = Guid.NewGuid().ToString();
        this.name = name;
        GenerateCustomerTag();
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        this.customerLook = rng.PickOne(SettingManager.Instance.ArtSettings.CustomerSprites.sprites);
    }

    public void GenerateCustomerTag(){
        customerTags = new List<ICustomerTag>();
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        List<CustomerTagData> tagDatas = this.GetSystem<IDataSystem>().GetAllCustomerTagData();

        // 随机选取一定数量的标签
        List<CustomerTagData> selectedTagDatas = rng.PickMany(tagDatas, 1);
        customerTags.Add(CustomerTagFactory.CreateCustomerTag(selectedTagDatas[0]));
    }
}