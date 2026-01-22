// 通知DisplayContainer刷新本质上是在执行一套固定的任务，而这个任务序列可以由DisplayContainer内部存储，由外部以DisplayTask的形式传入
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface IDisplayTask<TData, TView> where TData : class where TView : MonoBehaviour, IDisplayItemView<TData>{
    IItemInteractStrategy<TData, TView> GetStrategy();
    IEnumerable<TData> GetList();
}

