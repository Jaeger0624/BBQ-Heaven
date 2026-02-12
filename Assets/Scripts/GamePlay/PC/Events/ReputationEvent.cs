using QFramework;
using UnityEngine;

public class ReputationChangedEvent : AbstractEvent{
    public int amount;
    public ReputationChangedEvent(int amount){
        this.amount = amount;
    }
}

public class ReputationChangedEvent_飘字 : AbstractEvent, IFloatingTextEvent{
    public int amount;
    public Transform targetTransform;
    public ReputationChangedEvent_飘字(int amount, Transform targetTransform){
        this.amount = amount;
        this.targetTransform = targetTransform;
    }
    public string GetDescription(){
        return $"声望增加了 {amount}";
    }
    public Vector3 GetPosition(){
        return targetTransform.position;
    }
}