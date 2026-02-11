using QFramework;
using UnityEngine;
/// <summary>
/// 负责处理交易相关的逻辑
/// 能够抓取BBQView，并将其拖拽到顾客上进行交易
/// </summary>
public class DealController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private BBQView draggingBBQView;
    private Vector3 dragOffset; 
    void OnEnable()
    {
        this.RegisterEvent<DealStartedEvent>(OnDealStartedEvent);
        this.RegisterEvent<DealCompletedEvent>(OnDealCompletedEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<DealStartedEvent>(OnDealStartedEvent);
        this.UnRegisterEvent<DealCompletedEvent>(OnDealCompletedEvent);
    }
    private void OnDealStartedEvent(DealStartedEvent dealStartedEvent)
    {
        // 可在此触发开场演出，例如高亮顾客/BBQ 等
    }
    private void OnDealCompletedEvent(DealCompletedEvent dealCompletedEvent)
    {
        DealResult result = dealCompletedEvent.result;
        CustomerSatisfaction satis = result.satisfactionSystem;
    }

}
