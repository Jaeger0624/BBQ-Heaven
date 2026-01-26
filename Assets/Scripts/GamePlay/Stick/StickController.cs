using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 烤串系统 - 控制器层
/// </summary>
/// StickController -> StickView -> StickSystem
/// 创建烤串视图，更新棋盘选中范围
/// 职责：
/// 1. 创建与移除烤串视图
/// 2. 检测烤串选择，并提示UI更新
public class StickController : MonoBehaviour, IController, ICanSendEvent{
    [SerializeField] private GameObject stickViewPrefab;
    [SerializeField] private Transform stickViewContainer;  // 烤串父物体
    private IStickSystem stickSystem => this.GetSystem<IStickSystem>();
    private List<StickView> stickViews = new List<StickView>();
    private List<Transform> slots = new List<Transform>();
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    void Update()
    {
        if (this.GetSystem<IBBQSystem>().IsBBQing && stickSystem.selectedStick != null){
            stickViews.ForEach(x => x.OnUnselect());
            stickSystem.UnselectStick();
            SendClearEvents();
        }

        // 按下鼠标滚轮，则检测滚轮操作
        if (Input.GetMouseButtonDown(2)){
            if (stickSystem.selectedStick != null){
                stickViews.ForEach(x => x.OnUnselect());
                stickSystem.UnselectStick();

                SendClearEvents();
            }
        }
    }
    private void SendClearEvents(){
        this.SendEvent(new HideBBQPreviewEvent());
        this.SendEvent(new TimePreviewEvent(0));
        this.SendEvent(new ResetRecipePreviewViewsEvent());
    }
    void OnEnable()
    {
        this.RegisterEvent<AddStickEvent>(OnAddStick);
        this.RegisterEvent<RemoveStickEvent>(OnRemoveStick);
    }

    void OnDisable()
    {
        this.UnRegisterEvent<AddStickEvent>(OnAddStick);
        this.UnRegisterEvent<RemoveStickEvent>(OnRemoveStick);
    }
    void Show(){

    }
    void Hide(){
        
    }
    // 创建烤串视图
    public StickView CreateStickView(Stick stick){
        // 1. 创建一个空物体作为插槽
        // 创建一个空的UI组件
        RectTransform slot = new GameObject("Slot").AddComponent<RectTransform>();
        slot.SetParent(stickViewContainer);
        slot.localScale = new Vector3(1, 1, 1);
        slot.localPosition = Vector3.zero;
        slot.localEulerAngles = Vector3.zero;

        slots.Add(slot);
        // 2. 创建烤串视图
        StickView stickView = Instantiate(stickViewPrefab, slot).GetComponent<StickView>();
        stickView.Init(stick, slot);
        stickView.gameObject.SetActive(false);
        return stickView;
    }

    void OnAddStick(AddStickEvent e){
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            AddStickView(e.stick);
        }));
        this.GetSystem<IAnimationSystem>().Play();
    }

    private void AddStickView(Stick stick){
        StickView stickView = CreateStickView(stick);
        stickViews.Add(stickView);

        float distance = SettingManager.Instance.AnimSettings.foodInstanceCreateDistance;
        // 创建动画
        IAnimTask animTask = new RelativeMoveAnimationTask(
            stickView.transform,
            0.3f,
            stickView.slot,
            Vector3.zero,
            new Vector3(0, distance, 0),
            true).SetUseUnscaledTime(true).SetEase(Ease.OutSine);

        Observable.NextFrame().Subscribe(_ => {
            stickView.gameObject.SetActive(true);
            this.GetSystem<IAnimationSystem>().DirectlyPlay(animTask);
        }).AddTo(this.gameObject);
    }

    void OnRemoveStick(RemoveStickEvent e){
        StickView stickView = stickViews.Find(x => x.stick == e.stick);
        if (stickView != null){
            Transform slot = stickView.slot;
            Destroy(slot.gameObject);
            stickViews.Remove(stickView);
            slots.Remove(slot);
        }
    }


    // void DetectScrollWheel(){
    //     if (Time.time - lastScrollWheelTime >= scrollWheelCD) return;

    //     List<Stick> currentSticks = stickSystem.StickRepositorys();

    //     if (stickSystem.selectedStick != null){
    //         int index = currentSticks.IndexOf(stickSystem.selectedStick);
    //         if (Input.GetAxis("Mouse ScrollWheel") < 0){
    //             // 上滑
    //             int newIndex = (index + 1) % currentSticks.Count;
    //             stickViews.ForEach(x => x.OnUnselect());
    //             StickView nextStickView = stickViews.Find(x => x.stick == currentSticks[newIndex]);
    //             nextStickView.OnSelect();
    //         }
    //         else if (Input.GetAxis("Mouse ScrollWheel") > 0){
    //             int newIndex = (index - 1 + currentSticks.Count) % currentSticks.Count;
    //             stickViews.ForEach(x => x.OnUnselect());
    //             StickView previousStickView = stickViews.Find(x => x.stick == currentSticks[newIndex]);
    //             previousStickView.OnSelect();
    //         }
    //         lastScrollWheelTime = Time.time;
    //     }
    //     else{
    //         if (currentSticks.Count == 0) return;
    //         if (Input.GetAxis("Mouse ScrollWheel") < 0){
    //             // 上滑
    //             stickViews.ForEach(x => x.OnUnselect());
    //             StickView nextStickView = stickViews.Find(x => x.stick == currentSticks[0]);
    //             nextStickView.OnSelect();
    //         }
    //         else if (Input.GetAxis("Mouse ScrollWheel") > 0){
    //             // 下滑
    //             stickViews.ForEach(x => x.OnUnselect());
    //             StickView previousStickView = stickViews.Find(x => x.stick == currentSticks[currentSticks.Count - 1]);
    //             previousStickView.OnSelect();
    //         }
    //         lastScrollWheelTime = Time.time;
    //     }
    // }

}