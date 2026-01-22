
using cfg;
// 纯数据类，ViewModel
public class MascotUIContext
{
// 基础配置信息 (无论是否有库存，这个一定有)
    public MascotData ConfigData { get; private set; }
    
    // 实际库存对象 (图鉴模式下可能为 null)
    public Mascot RuntimeMascot { get; private set; }
    
    // 状态标识
    public CollectionState State { get; private set; } // 图鉴用：是否解锁
    public bool IsOwned => RuntimeMascot != null;  // 仓库用：是否拥有
    public int Count { get; private set; } // 数量展示

    public int Price { get; private set; } = -1; // 价格 = -1 表示没有价格
    // --- 构造函数 ---
    
    // 场景 A: 仓库/商店 (通过 Mascot 构建)
    public MascotUIContext(Mascot mascot)
    {
        RuntimeMascot = mascot;
        ConfigData = mascot.data; // 假设 Mascot 里引用了 data
        State = CollectionState.Discovered;
        Count = mascot.StackNumber; // 假设 Mascot 有 Count 字段
    }

    // 场景 B: 图鉴 (通过 MascotData 构建)
    public MascotUIContext(MascotData data, CollectionState state)
    {
        RuntimeMascot = null;
        ConfigData = data;
        State = state;
        Count = 1;
    }

    public void SetPrice(int price){
        Price = price;
    }
    public void SetAmount(int amount){
        Count = amount;
    }
}