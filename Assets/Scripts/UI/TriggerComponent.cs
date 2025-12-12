using System;
using UniRx;

public interface ITriggerComponent{
    // 无参无返回值的函数
    Action TriggerEvent { get;  }
}