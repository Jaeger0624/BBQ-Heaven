
// using System.Collections.Generic;
// using System.Linq;
// using QFramework;
// using UnityEngine;

// public class CustomerLookMaker : ICanGetSystem{
//     private Dictionary<CustomerLookType, List<int>> customerLooks = new Dictionary<CustomerLookType, List<int>>();
//     private Dictionary<string, CustomerLook> lookDict = new Dictionary<string, CustomerLook>();

//     public IArchitecture GetArchitecture() => GameArchitecture.Interface;

//     /// <summary> 生成顾客外观 </summary>
//     public CustomerLook GetCustomerLook(string name, IGetCustomerLookStrategy getCustomerLookStrategy){
//         return getCustomerLookStrategy.GetCustomerLook(name, customerLooks);
//     }

//     /// <summary> 重置 </summary>
//     public void Reset(){
//         customerLooks.Clear();
//         InitUnusedIndices();
//     }

//     /// <summary> 初始化未使用的索引 </summary>
//     private void InitUnusedIndices(){
//         customerLooks[CustomerLookType.Appearance] = new List<int>(Enumerable.Range(0, 15));
//     }

//     /// <summary> 获取未使用的索引 </summary>
//     private int GetUnusedIndex(CustomerLookType lookType){
//         List<int> indices = customerLooks[lookType];
//         if (indices.Count == 0){
//             // 如果空了，则重新生成
//             InitUnusedIndices();
//         }
//         indices = customerLooks[lookType];

//         Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
//         int index = rng.PickOne(indices);
//         indices.Remove(index);
//         return index;
//     }
// }

// public interface IGetCustomerLookStrategy{
//     CustomerLook GetCustomerLook(string name, Dictionary<CustomerLookType, List<int>> customerLooks);
// }

// public class GetCustomerLookStrategy_纯随机 : IGetCustomerLookStrategy{
//     public CustomerLook GetCustomerLook(string name, Dictionary<CustomerLookType, List<int>> customerLooks){
//         Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
//         // 纯随机，直接返回一个随机索引
//         return new CustomerLook(rng.PickOne(customerLooks[CustomerLookType.Appearance]));
//     }
// }

// public class GetCustomerLookStrategy_根据字典生成 : IGetCustomerLookStrategy{
//     private Dictionary<string, CustomerLook> lookDict;
//     public GetCustomerLookStrategy_根据字典生成(Dictionary<string, CustomerLook> lookDict){
//         this.lookDict = lookDict;
//     }
//     public CustomerLook GetCustomerLook(string name, Dictionary<CustomerLookType, List<int>> customerLooks){
        
//         if (lookDict.TryGetValue(name, out CustomerLook customerLook)){
//             return customerLook;
//         }
//         else{
//             CustomerLook newCustomerLook = 
//                 new CustomerLook(GetUnusedIndex(CustomerLookType.Appearance, customerLooks));
//             lookDict.Add(name, newCustomerLook);
//             return newCustomerLook;
//         }
//     }

//     private int GetUnusedIndex(CustomerLookType lookType, Dictionary<CustomerLookType, List<int>> customerLooks){
//         List<int> indices = customerLooks[lookType];
//         if (indices.Count == 0){
//             // 如果空了，则重新生成
//             int maxAmount = SettingManager.Instance.ArtSettings.CustomerSprites.sprites.Count;
//             customerLooks[lookType] = new List<int>(Enumerable.Range(0, maxAmount));
//         }
//         indices = customerLooks[lookType];
        
//         Rng rng = GameArchitecture.Interface.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
//         int index = rng.PickOne(indices);
//         indices.Remove(index);
//         return index;
//     }
// }

// public enum CustomerLookType{
//     Appearance,
//     Accessory,
//     Hair,
//     Expression,
//     Action,
// }


// public class CustomerLook{
//     public int SpriteIndex;

//     // 外观 -> 配饰 -> 发型 -> 表情 -> 动作
//     public CustomerLook(int spriteIndex){
//         this.SpriteIndex = spriteIndex;
//     }

//     public bool Equals(CustomerLook other){
//         return this.SpriteIndex == other.SpriteIndex;
//     }
// }