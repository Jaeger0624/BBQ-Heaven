using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface ICollectionSystem : ISystem{
    // 查询API
    CollectionState GetItemState(string itemId, CollectionType itemType);
    bool IsUnlocked(string itemId, CollectionType itemType);
    bool IsDiscovered(string itemId, CollectionType itemType);

    // 修改API
    void UnlockItem(string itemId, CollectionType itemType);
    void DiscoverItem(string itemId, CollectionType itemType);

    // 注册数据源
    void RegisterProvider(ICollectionDataProvider dataProvider);
}

public class CollectionSystem : AbstractSystem, ICollectionSystem{
    protected override void OnInit()
    {
        RegisterProvider(new FoodCollectionProvider());
        RegisterProvider(new StickCollectionProvider());
        RegisterProvider(new CardCollectionProvider());
        RegisterProvider(new RecipeCollectionProvider());
        RegisterProvider(new MascotCollectionProvider());
    }
    private SystemData SystemData => SaveSystem.SystemData;
    private ISaveSystem SaveSystem => this.GetSystem<ISaveSystem>();
    // 缓存数据源，用于后续校验
    private List<ICollectionDataProvider> DataProviders = new List<ICollectionDataProvider>();
    public void RegisterProvider(ICollectionDataProvider dataProvider)
    {
        DataProviders.Add(dataProvider);
        SyncDataWithProvider(dataProvider);
    }
    private void SyncDataWithProvider(ICollectionDataProvider provider)
    {
        var type = provider.Type;
        var container = SystemData.GetCollectionContainer(type);
        bool isDirty = false;

        foreach (var item in provider.GetAllItems())
        {
            // 如果存档里没有这个ID，说明是新版本新增的物品
            if (!container.ContainsKey(item.ID))
            {
                var initialState = item.DefaultUnlocked ? CollectionState.Unlocked : CollectionState.Locked;
                container.Add(item.ID, initialState);
                isDirty = true;
                Debug.Log($"[Collection] 新增图鉴条目: [{type}] {item.ID}");
            }
        }

        if (isDirty) SaveSystem.SaveSystemData();
    }
    public CollectionState GetItemState(string itemId, CollectionType itemType)
    {
        var container = SystemData.GetCollectionContainer(itemType);
        return container.TryGetValue(itemId, out var state) ? state : CollectionState.Locked;
    }
    public bool IsUnlocked(string itemId, CollectionType itemType) => GetItemState(itemId, itemType) == CollectionState.Unlocked;
    public bool IsDiscovered(string itemId, CollectionType itemType) => GetItemState(itemId, itemType) == CollectionState.Discovered;

    public void UnlockItem(string itemId, CollectionType itemType) => UpdateState(itemId, CollectionState.Unlocked, itemType);
    public void DiscoverItem(string itemId, CollectionType itemType) => UpdateState(itemId, CollectionState.Discovered, itemType);
    private void UpdateState(string itemId, CollectionState state, CollectionType itemType){
        var container = SystemData.GetCollectionContainer(itemType);
        
        if (!container.ContainsKey(itemId)) return;

        CollectionState currentState = container[itemId];

        if (state > currentState){
            container[itemId] = state;
            this.SendEvent(new CollectionStateChangeEvent(itemId, state, itemType));
        }
    }
}