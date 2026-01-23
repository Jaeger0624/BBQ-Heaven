using System;
using System.Collections.Generic;
using System.Text;
using cfg;
using DG.Tweening;
using QFramework;
using UniRx;
using UnityEngine;

/// <summary>
/// 食材系统 - 控制器层
/// </summary>
public class EntityViewController : MonoBehaviour, IController
{
    private IFoodSystem foodSystem => this.GetSystem<IFoodSystem>();
    private FoodAnimController foodAnimController;
    [SerializeField] private GameObject EntityViewPrefab;
    [SerializeField] private GameObject FoodInstanceViewPrefab;
    [SerializeField] private Transform supplyStartPoint;
    // 需要通过BoardView定位食材实例视图
    [SerializeField] private BoardViewUGUI boardView;
    private Dictionary<string, IEntityView> entityViews = new Dictionary<string, IEntityView>();
    void Awake()
    {
        foodAnimController = gameObject.GetComponent<FoodAnimController>() ?? gameObject.AddComponent<FoodAnimController>();
    }
    void OnEnable()
    {
        // 1. 生成食材
        this.RegisterEvent<CreateFoodInstanceEvent>(OnCreateFoodInstanceEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<RemoveFoodInstanceEvent>(OnRemoveFoodInstanceEvent).UnRegisterWhenDisabled(this);

        // 2. 生成非食材
        this.RegisterEvent<CreateEntityEvent>(OnCreateEntityEvent).UnRegisterWhenDisabled(this);

        // 3. 实体移动事件
        this.RegisterEvent<MoveEntityEvent>(OnMoveEntityEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<PlaceEntityEvent>(OnPlaceEntityEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<SwapEntityEvent>(OnSwapEntityEvent).UnRegisterWhenDisabled(this);


        // 4. 碰撞事件
        this.RegisterEvent<CollisionEntityEvent>(OnCollisionEntityEvent).UnRegisterWhenDisabled(this);
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)){
            // 打印食材仓库信息
            Dictionary<string, FoodCard> foodRepositorys = foodSystem.FoodRepositorys();
            StringBuilder sb = new StringBuilder();
            foreach (var food in foodRepositorys){
                sb.AppendLine($"食材仓库信息: {food.Key} - {food.Value.foodData.Name} - {food.Value.foodData.ID}");
            }
            Debug.Log(sb.ToString());
        }
    }

    // 创建新食材实例事件
    void OnCreateFoodInstanceEvent(CreateFoodInstanceEvent e)
    {
        // 1. 获取食材实例
        FoodInstance foodInstance = e.foodInstance;
        // 2. 生成食材实例视图
        ControlView(foodInstance);
    }
    void OnCreateEntityEvent(CreateEntityEvent e)
    {
        // 1. 获取食材实例
        BoardEntity entity = e.entity;
        Debug.Log($"【EntityViewController】创建实体: {entity.name}");
        // 2. 生成食材实例视图
        ControlView(entity);
    }

    
    // 生成食材实例视图
    private IEntityView GenerateEntityView(BoardEntity entity){
        GameObject prefab = entity is FoodInstance ? FoodInstanceViewPrefab : EntityViewPrefab;
        IEntityView entityView = Instantiate(prefab, transform).GetComponent<IEntityView>();
        entityView.Bind(entity);
        return entityView;
    }

    public IArchitecture GetArchitecture() => 
        GameArchitecture.Interface;
    
    public IEntityView GetEntityView(string guid) => 
        entityViews.TryGetValue(guid, out IEntityView entityView) ? entityView : null;


    #region 具体动画
    private void ControlView(BoardEntity entity){
        IEntityView entityView = GenerateEntityView(entity);
        entityView.GO().SetActive(false);
        // 3. 设置父物体
        Transform cellTransform = boardView.GetCellTransform(entity.position);
        float scale = entityView.GO().transform.localScale.x;
        entityView.GO().transform.SetParent(cellTransform,true);
        entityView.GO().transform.localScale = new Vector3(scale, scale, 1);

        Vector3 relativeEnd = new Vector3(0, 0, -1f);
        Vector3 relativeStart = new Vector3(0, SettingManager.Instance.AnimSettings.foodInstanceCreateDistance, 0);
        // 4. 播放移动动画
        // 4.1 创建移动动画任务作为附属动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => {
                if (entityView == null) return;
                entityView.GO().SetActive(true);
            }),
            new RelativeMoveAnimationTask(entityView.GO().transform, 0.3f, cellTransform, relativeEnd, relativeStart)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveAnimDelay;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        // Debug.Log($"【FoodController】将食材实例视图移动到棋盘格子: {cellTransform.position}");
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);

        entityViews.Add(entity.guid, entityView);

    }
    void OnRemoveFoodInstanceEvent(RemoveFoodInstanceEvent e)
    {
        if (!entityViews.TryGetValue(e.foodInstance.guid, out IEntityView foodInstanceView)) return;

        //TODO: 播放消失动画
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            entityViews.Remove(e.foodInstance.guid);
            Destroy(foodInstanceView.GO());
        }));
        this.GetSystem<IAnimationSystem>().Play();
    }
    void OnMoveEntityEvent(MoveEntityEvent e)
    {
        if (!entityViews.TryGetValue(e.entity.guid, out IEntityView foodInstanceView)) return;

        Vector3 originPos = foodInstanceView.GO().transform.position;
        // 1. 获取食材实例视图的父物体
        Transform cellTransform = boardView.GetCellTransform(e.newPos);
        float scale = foodInstanceView.GO().transform.localScale.x;
        foodInstanceView.GO().transform.SetParent(cellTransform,true);
        foodInstanceView.GO().transform.localScale = new Vector3(scale, scale, 1);

        // 2. 计算目标位置
        Vector3 targetPosition = cellTransform.position;
        targetPosition.z = foodInstanceView.GO().transform.position.z - 0.1f;

        // 3. 播放移动动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => foodInstanceView.GO().SetActive(true)),
            new MoveAnimationTask(foodInstanceView.GO().transform, 0.4f, targetPosition, originPos)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveDelay_棋盘上移动;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);
    }
    void OnPlaceEntityEvent(PlaceEntityEvent e)
    {
        if (!entityViews.TryGetValue(e.entity.guid, out IEntityView foodInstanceView)) return;

        Transform cellTransform = boardView.GetCellTransform(e.newPos);
        float scale = foodInstanceView.GO().transform.localScale.x;
        foodInstanceView.GO().transform.SetParent(cellTransform,true);
        foodInstanceView.GO().transform.localScale = new Vector3(scale, scale, 1);

        Vector3 originPos = foodInstanceView.GO().transform.position;
        Vector3 targetPosition = cellTransform.position;
        targetPosition.z = foodInstanceView.GO().transform.position.z - 0.1f;

        // 3. 播放移动动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => foodInstanceView.GO().SetActive(true)),
            new MoveAnimationTask(foodInstanceView.GO().transform, 0.4f, targetPosition, originPos)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveDelay_棋盘上移动;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);
    }
    void OnSwapEntityEvent(SwapEntityEvent e)
    {
        if (!entityViews.TryGetValue(e.entity1.guid, out IEntityView foodInstanceView1)) return;
        if (!entityViews.TryGetValue(e.entity2.guid, out IEntityView foodInstanceView2)) return;

        Vector3 originPos1 = foodInstanceView1.GO().transform.position;
        Vector3 originPos2 = foodInstanceView2.GO().transform.position;
        Transform cellTransform1 = boardView.GetCellTransform(e.entity1.position);
        Transform cellTransform2 = boardView.GetCellTransform(e.entity2.position);
        float scale1 = foodInstanceView1.GO().transform.localScale.x;
        float scale2 = foodInstanceView2.GO().transform.localScale.x;
        foodInstanceView1.GO().transform.SetParent(cellTransform1,true);
        foodInstanceView2.GO().transform.SetParent(cellTransform2,true);
        foodInstanceView1.GO().transform.localScale = new Vector3(scale1, scale1, 1);
        foodInstanceView2.GO().transform.localScale = new Vector3(scale2, scale2, 1);

        Vector3 targetPosition1 = cellTransform1.position;
        targetPosition1.z = foodInstanceView1.GO().transform.position.z - 0.1f;
        Vector3 targetPosition2 = cellTransform2.position;
        targetPosition2.z = foodInstanceView2.GO().transform.position.z - 0.1f;

        // 3. 播放移动动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => foodInstanceView1.GO().SetActive(true)),
            new ActionAnimTask(() => foodInstanceView2.GO().SetActive(true)),
            new MoveAnimationTask(foodInstanceView1.GO().transform, 0.4f, targetPosition1, originPos1),
            new MoveAnimationTask(foodInstanceView2.GO().transform, 0.4f, targetPosition2, originPos2)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveDelay_棋盘上移动;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);
    }

    void OnCollisionEntityEvent(CollisionEntityEvent e)
    {
        if (!entityViews.TryGetValue(e.initiator.guid, out IEntityView initiatorView)) return;
        if (!entityViews.TryGetValue(e.receiver.guid, out IEntityView receiverView)) return;
        Debug.Log($"【EntityViewController】碰撞动画: {e.initiator.name} 和 {e.receiver.name} 方向: {e.direction}");
        Direction direction = e.direction.ToDirection();
        float distance = 50f;
        float delay = 0.1f;
        switch (direction){
            case Direction.上:
                // 会回弹
                initiatorView.GO().transform.DOLocalMove(new Vector3(0, distance, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(initiatorView.GO());

                // reciever等待0.1秒后移动
                Observable.Timer(TimeSpan.FromSeconds(delay), Scheduler.MainThread).Subscribe(_ => {
                    receiverView.GO().transform.DOLocalMove(new Vector3(0, distance, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(receiverView.GO());
                }).AddTo(this);
                break;
            case Direction.下:
                initiatorView.GO().transform.DOLocalMove(new Vector3(0, -distance, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(initiatorView.GO());
                // reciever等待0.1秒后移动
                Observable.Timer(TimeSpan.FromSeconds(delay), Scheduler.MainThread).Subscribe(_ => {
                    receiverView.GO().transform.DOLocalMove(new Vector3(0, -distance, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(receiverView.GO());
                }).AddTo(this);
                break;
            case Direction.左:
                initiatorView.GO().transform.DOLocalMove(new Vector3(-distance, 0, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(initiatorView.GO());
                // reciever等待0.1秒后移动
                Observable.Timer(TimeSpan.FromSeconds(delay), Scheduler.MainThread).Subscribe(_ => {
                    receiverView.GO().transform.DOLocalMove(new Vector3(-distance, 0, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(receiverView.GO());
                }).AddTo(this);
                break;
            case Direction.右:
                initiatorView.GO().transform.DOLocalMove(new Vector3(distance, 0, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(initiatorView.GO());
                // reciever等待0.1秒后移动
                Observable.Timer(TimeSpan.FromSeconds(delay), Scheduler.MainThread).Subscribe(_ => {
                    receiverView.GO().transform.DOLocalMove(new Vector3(distance, 0, 0), 0.4f).SetLoops(2, LoopType.Yoyo).SetLink(receiverView.GO());
                }).AddTo(this);
                break;
            default:
                Debug.LogError($"【EntityViewController】碰撞方向不支持: {direction}");
                break;
        }
    }

    #endregion
}
