using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class SystemData{
    [OdinSerialize]
    public Dictionary<CollectionType, Dictionary<string, CollectionState>> CollectionStates { get; private set; }

    public SystemData(){
        CollectionStates = new Dictionary<CollectionType, Dictionary<string, CollectionState>>();
    }
    public Dictionary<string, CollectionState> GetCollectionContainer(CollectionType itemType)
    {
        if (CollectionStates == null) {
            Debug.LogError("<color=red>【SystemData】图鉴状态容器为空</color>"); 
            CollectionStates = new Dictionary<CollectionType, Dictionary<string, CollectionState>>();
        }
        if (!CollectionStates.ContainsKey(itemType))
        {
            CollectionStates[itemType] = new Dictionary<string, CollectionState>();
        }
        return CollectionStates[itemType];
    }
}