
using System.Collections.Generic;
using QFramework;

public enum CollectionType{
    None = 0,
    Food = 1, // 食材
    Stick = 2,
    Card = 3,
    Recipe = 4,
    Mascot = 5,
    PlayerCharacter = 6,
    NPC = 7,
    Encounter = 8,
}

public enum CollectionState
{
    Locked = 0,      // 未解锁 (不生成，图鉴不可见)
    Unlocked = 1,    // 已解锁 (可生成，图鉴显示 ???)
    Discovered = 2   // 已发现 (已遭遇，图鉴显示详情)
}


public class CollectionStateChangeEvent : AbstractEvent{
    public string ItemID;
    public CollectionState State;
    public CollectionType ItemType;
    public CollectionStateChangeEvent(string itemId, CollectionState state, CollectionType itemType){
        this.ItemID = itemId;
        this.State = state;
        this.ItemType = itemType;
    }
}

public struct CollectionItemDefinition{
    public string ID;
    public bool DefaultUnlocked;
}

public interface ICollectionDataProvider{
    CollectionType Type { get; }
    List<CollectionItemDefinition> GetAllItems();
}