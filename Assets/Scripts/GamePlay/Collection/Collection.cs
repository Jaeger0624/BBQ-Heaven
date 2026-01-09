
public enum ItemType{
    Food, // 食材
    Stick, // 串签
    Card, // 卡牌
    Recipe, // 配方
    Mascot, // 吉祥物
    PlayerCharacter, // 玩家角色
    NPC, // 非玩家角色
    Encounter, // 遭遇
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
    public ItemType ItemType;
    public CollectionStateChangeEvent(string itemId, CollectionState state, ItemType itemType){
        this.ItemID = itemId;
        this.State = state;
        this.ItemType = itemType;
    }
}
