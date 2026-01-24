
using cfg;
using QFramework;
// 纯数据类，ViewModel
public class CardUIContext : ICanGetSystem
{
// 基础配置信息 (无论是否有库存，这个一定有)
    public CardData ConfigData { get; private set; }
    
    // 实际库存对象 (图鉴模式下可能为 null)
    public Card RuntimeCard { get; private set; }
    
    // 状态标识
    public CollectionState State { get; private set; } // 图鉴用：是否解锁
    public bool IsOwned => RuntimeCard != null;  // 仓库用：是否拥有
    public int Count { get; private set; } // 数量展示
    public int Price { get; private set; } = -1; // 价格 = -1 表示没有价格
    // --- 构造函数 ---
    
    // 场景 A: 仓库/商店 (通过 Card 构建)
    public CardUIContext(Card card)
    {
        RuntimeCard = card;
        ConfigData = card.CardData;
        State = CollectionState.Discovered;
        Count = 1; // 假设 Card 有 Count 字段
    }

    // 场景 B: 图鉴 (通过 CardData 构建)
    public CardUIContext(CardData data, CollectionState state)
    {
        RuntimeCard = null;
        ConfigData = data;
        State = state;
        Count = 0;
    }

    public void SetPrice(int price){
        Price = price;
    }
    public void SetAmount(int amount){
        Count = amount;
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}