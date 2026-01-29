using QFramework;
using UnityEngine;

public class FloatingTextHandler : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public void OnEnable() {
        this.RegisterEvent<CollisionEntityEvent>(OnCollisionEntityEvent).UnRegisterWhenDisabled(this);

        this.RegisterEvent<ChangeCustomerPatienceEvent_飘字>(OnChangeCustomerPatienceEvent_飘字).UnRegisterWhenDisabled(this);
    }

    // 实体碰撞跳字
    private void OnCollisionEntityEvent(CollisionEntityEvent evt)
    {
        BoardEntity entity = evt.initiator;
        IEntityView entityView = entity.GetEntityView();
        if (entityView != null){
            string text = $"<color=yellow>{entity.name}</color> 撞击了 <color=yellow>{evt.receiver.name}</color>";
            FloatingTextManager.Instance.Show(entityView.GO().transform.position, text, Color.white, new FloatingTextInfo(text, 1.2f, Color.white));
        }
        else{
            Debug.LogError($"【FloatingTextHandler】实体视图不存在: {entity.guid}");
        }
    }

    // 顾客等待值变化跳字
    private void OnChangeCustomerPatienceEvent_飘字(ChangeCustomerPatienceEvent_飘字 evt)
    => FloatingTextManager.Instance.Show(evt.GetPosition(), evt.GetDescription(), Color.white, new FloatingTextInfo(evt.GetDescription(), 1.2f, Color.white, Vector2.right));
    
}