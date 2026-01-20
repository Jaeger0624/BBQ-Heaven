
using cfg;
// 纯数据类，ViewModel
public class FoodUIContext
{
// 基础配置信息 (无论是否有库存，这个一定有)
    public FoodData ConfigData { get; private set; }
    
    // 实际库存对象 (图鉴模式下可能为 null)
    public FoodCard RuntimeFood { get; private set; }
    
    // 状态标识
    public CollectionState State { get; private set; } // 图鉴用：是否解锁
    public bool IsOwned => RuntimeFood != null;  // 仓库用：是否拥有
    public int Count { get; private set; }       // 数量展示

    // --- 构造函数 ---
    
    // 场景 A: 仓库/商店 (通过 Food 构建)
    public FoodUIContext(FoodCard food)
    {
        RuntimeFood = food;
        ConfigData = food.foodData; // 假设 Food 里引用了 data
        State = CollectionState.Discovered;
        Count = 1; // 假设 Food 有 Count 字段
    }

    // 场景 B: 图鉴 (通过 FoodData 构建)
    public FoodUIContext(FoodData data, CollectionState state)
    {
        RuntimeFood = null;
        ConfigData = data;
        State = state;
        Count = 0;
    }
}