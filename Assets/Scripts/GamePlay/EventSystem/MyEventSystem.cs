using QFramework;
using UnityEngine;

public interface IMyEventSystem : ISystem{
    void RandomSelectEvent(int amount);
}
public class MyEventSystem : AbstractSystem, IMyEventSystem{
    Rng rng => this.GetSystem<IRngSystem>().GetSubRng<IMyEventSystem>();
    protected override void OnInit()
    {

    }
    // 创建事件选项的方法
    public void RandomSelectEvent(int amount){
        Debug.Log($"【MyEventSystem】随机选择事件: {amount}");


        // 1. 一个随机商店


        // 2. 一个强化事件
        

        // 3. 一个趣味事件


    }
}