using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class CreateBBQController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private BBQView currentBBQView;
    [LabelText("食材实例目标位置")]
    [SerializeField] private Transform foodInstanceTarget;
    [LabelText("烧烤视图预制体")]
    [SerializeField] private GameObject BBQViewPrefab;
    [LabelText("烧烤视图容器")]
    [SerializeField] private Transform BBQViewContainer;

    public Vector3 showPosition;
    public Vector3 hidePosition;
    void OnEnable()
    {
        this.RegisterEvent<CombineBBQEvent>(OnCombineBBQEvent);
        this.RegisterEvent<PlaceFoodInstanceEvent>(OnPlaceFoodInstance);
        this.RegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.RegisterEvent<FinishCombineBBQEvent_动画>(OnFinishBBQ);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CombineBBQEvent>(OnCombineBBQEvent);
        this.UnRegisterEvent<PlaceFoodInstanceEvent>(OnPlaceFoodInstance);
        this.UnRegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.UnRegisterEvent<FinishCombineBBQEvent_动画>(OnFinishBBQ);
    }
    
    // 创建一个DOTween动画，将烧烤视图从hidePosition移动到showPosition
    [Button]
    private void Show(){
        (transform as RectTransform).DOAnchorPos(showPosition, 0.5f).SetUpdate(true).SetEase(Ease.OutBack).SetLink(gameObject);
    }
    [Button]
    private void Hide(){
        (transform as RectTransform).DOAnchorPos(hidePosition, 0.5f).SetUpdate(true).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    private void OnCombineBBQEvent(CombineBBQEvent e){
        Show();
        currentBBQView = Instantiate(BBQViewPrefab, BBQViewContainer).GetComponent<BBQView>();
        currentBBQView.transform.localPosition = new Vector3(0, 0, -1);
        // Debug.Log($"【CreateBBQController】创建烧烤视图: {currentBBQView.name}");
        currentBBQView.Bind(e.bbq);
    }
    private void OnFinishBBQ(FinishCombineBBQEvent_动画 e) => Hide();
    private void OnPlaceFoodInstance(PlaceFoodInstanceEvent e){
        if (currentBBQView == null) return;
        IAnimationSystem animationSystem = this.GetSystem<IAnimationSystem>();
        // 1. 先放置在
        IAnimTask animTask = AnimCombine_放置烤串食材.Anim_放置单个食材(e.view, e.slotIndex, foodInstanceTarget, currentBBQView);
        animationSystem.DirectlyPlay(animTask);
    }

    private void OnAddBBQToRepository(AddBBQToRepositoryEvent e){
        // 1. 移动烧烤视图到烧烤仓库
        this.SendEvent(new MoveBBQToRepositoryEvent(currentBBQView));
        // 2. 清空当前烧烤视图
        currentBBQView = null;
    }

}

public class PlaceFoodInstanceEvent : AbstractEvent{
    public IEntityView view;
    public int slotIndex;
    public PlaceFoodInstanceEvent(IEntityView view, int slotIndex){
        this.view = view;
        this.slotIndex = slotIndex;
    }
}