using System;
using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using UniRx;
using UnityEngine;

public class TagView : MonoBehaviour, IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private CustomerTag currentCustomerTag;
    private List<bool> currentResults;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject tagViewComponentPrefab;
    [SerializeField] private Transform componentParent;
    private List<TagViewComponent> components = new List<TagViewComponent>();
    void Start()
    {
        Hide();
    }
    void OnEnable()
    {
        this.RegisterEvent<TagExecuteEvent>(OnTagExecuteEvent);
        this.RegisterEvent<ShowTagViewEvent>(OnShowTagViewEvent);
        this.RegisterEvent<HideTagViewEvent>(OnHideTagViewEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<TagExecuteEvent>(OnTagExecuteEvent);
        this.UnRegisterEvent<ShowTagViewEvent>(OnShowTagViewEvent);
        this.UnRegisterEvent<HideTagViewEvent>(OnHideTagViewEvent);
    }
    private void OnTagExecuteEvent(TagExecuteEvent e){
        Bind(e.customerTag, e.results);
    }

    public void Bind(CustomerTag customerTag, List<bool> results){
        this.currentCustomerTag = customerTag;
        this.currentResults = results;
        UpdateVisual();
    }

    private void OnShowTagViewEvent(ShowTagViewEvent e) => Show();
    private void OnHideTagViewEvent(HideTagViewEvent e) => Hide();
    private void UpdateVisual(){
        // 1. 清理旧的（ResetVisual 负责把旧的移出列表并销毁）
        // 注意：ResetVisual 必须同步清空 components 列表，这在原来的代码里是对的
        bool hasReset = ResetVisual();

        // 2. 【核心修复】立即创建新组件，不要等 Timer
        foreach (var cga in currentCustomerTag.tagCGAs){
            TagViewComponent tagViewComponent = Instantiate(tagViewComponentPrefab, componentParent).GetComponent<TagViewComponent>();
            components.Add(tagViewComponent); // 数据立即填充，防止越界
            tagViewComponent.Bind(cga);

            // 3. 处理视觉延迟
            if (hasReset)
            {
                // 如果需要延迟显示，先把它藏起来（比如缩放为0）
                tagViewComponent.transform.localScale = Vector3.zero;
                
                // 延迟后播放出现动画
                Observable.Timer(TimeSpan.FromSeconds(0.4f)).Subscribe(_ => {
                    if(tagViewComponent != null) 
                        tagViewComponent.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }).AddTo(this.gameObject);
            }
            else
            {
                // 如果不需要延迟，确保它是可见的
                tagViewComponent.transform.localScale = Vector3.one;
            }
        } 

        // 4. 触发 Tag 自身的后续动画逻辑
        float delay = hasReset ? 0.7f : 0.3f; // 0.4 + 0.3
        Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(_ => {
            TriggerAnim();
        }).AddTo(this.gameObject);
    }

    // 修改后：添加安全检查
    public Transform GetTagViewTransform(int index)
    {
        // 1. 检查列表是否为空
        if (components == null) return null;

        // 2. 检查索引是否越界
        if (index < 0 || index >= components.Count)
        {
            Debug.LogWarning($"[TagView] 尝试访问索引 {index}，但列表长度仅为 {components.Count}。UI可能尚未刷新。");
            return null; // 或者返回 transform (即自身)，视具体动画逻辑而定
        }

        return components[index].transform;
    }

    // 同步动画
    private void TriggerAnim(){
        int index = currentResults.FindIndex(x => x == true);
        if (index == -1){
            return;
        }
        else{
            Transform transform = components[index].transform;
            this.GetSystem<IAnimationSystem>().DirectlyPlay(new ScaleAnimationTask(transform, 1.2f, 0.15f,true));

            this.SendEvent(new TriggerAnimEvent(0.4f));
        }
    }


    private bool ResetVisual()
    {
        if (components.Count == 0){
            Debug.Log("TagView ResetVisual: 没有组件，直接触发动画");
            return false;
        }
        List<TagViewComponent> temp = new List<TagViewComponent>(components);
        Debug.Log($"TagView ResetVisual: 清除{temp.Count}个组件");
        foreach (var component in temp){
            component.Hide();
        }
        Observable.Timer(TimeSpan.FromSeconds(0.15f)).Subscribe(_ => {
            //清除所有组件
            ClearAll();
        }).AddTo(this.gameObject);
        return true;
    }
    private void Show(){

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    private void Hide(){
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        ClearAll();
    }
    private void ClearAll(){
        List<TagViewComponent> temp = new List<TagViewComponent>(components);
        foreach (var component in temp){
            Destroy(component.gameObject);
        }
        components.Clear();
    }
    private void Test()
    {
        CustomerTagData customerTagData = this.GetSystem<IDataSystem>().GetCustomerTagData("1");
        CustomerTag customerTag = new CustomerTag(customerTagData);
        Bind(customerTag, new List<bool>{true, false, false});
    }
    
}
