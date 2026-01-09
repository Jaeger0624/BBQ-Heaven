using QFramework;

public interface ICollectionSystem : ISystem{
    void UnlockItem(string itemId);
    void DiscoverItem(string itemId);
    bool GetItemState(string itemId);
}

public class CollectionSystem : AbstractSystem, ICollectionSystem{
    protected override void OnInit()
    {
    }
    protected override void OnDeinit()
    {
    }
    private void SyncWithConfig(){
        bool isDirty = false;

        
        if (isDirty){
            this.GetSystem<ISaveSystem>().SaveGame();
        }
    }
    public void UnlockItem(string itemId)
    {
        this.GetSystem<ISaveSystem>().SystemData.CollectionStates[itemId] = CollectionState.Unlocked;
    }

    public void DiscoverItem(string itemId)
    {
        this.GetSystem<ISaveSystem>().SystemData.CollectionStates[itemId] = CollectionState.Discovered;
    }

    public bool GetItemState(string itemId)
    {
        return this.GetSystem<ISaveSystem>().SystemData.CollectionStates[itemId];
    }

    private void UpdateState(string itemId, CollectionState state, ItemType itemType)
    {
        this.GetSystem<ISaveSystem>().SystemData.CollectionStates[itemId] = state;
    }
}