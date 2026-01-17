
using System;
using cfg;
using QFramework;

[Serializable]
public class Buff : ICanGetSystem{
    public string ID;
    public string name => buffData.Name;
    public BuffData buffData;
    public bool isStackable;
    private int stackNumber;
    private Action onRemoved;
    public Buff(string id, Action onRemoved, int stackNumber = 1){
        this.ID = id;

        // 通过名字获取buff数据
        this.buffData = this.GetSystem<IDataSystem>().GetBuffData(id);
        this.isStackable = buffData.IsStackable;
        this.onRemoved = onRemoved;
        if (!isStackable) stackNumber = 1;
        AddStackNumber(stackNumber);
    }
    public int GetStackNumber(){
        return isStackable ? stackNumber : 1;
    }
    public void AddStackNumber(int amount){
        if (!isStackable) return;
        stackNumber += amount;
    }
    public int RemoveStackNumber(int amount){
        if (!isStackable){
            onRemoved?.Invoke();
            return 0;
        }
        if (stackNumber <= amount){
            onRemoved?.Invoke();
            return stackNumber;
        }
        else{
            stackNumber -= amount;
            return stackNumber;
        }
    }
    public void SetStackNumber(int amount){
        if (!isStackable) return;
        stackNumber = amount;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}