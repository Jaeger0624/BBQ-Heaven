using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

/// <summary>
/// 食材系统 - 控制器层
/// </summary>
public class FoodController : MonoBehaviour, IController
{
    private IFoodSystem foodSystem => this.GetSystem<IFoodSystem>();
    private FoodAnimController foodAnimController;
    [SerializeField] private GameObject foodInstanceViewPrefab;
    [SerializeField] private Transform supplyStartPoint;
    // 需要通过BoardView定位食材实例视图
    [SerializeField] private BoardViewUGUI boardView;
    private Dictionary<string, IEntityView> foodInstanceViews = new Dictionary<string, IEntityView>();
    void Awake()
    {
        foodAnimController = gameObject.GetComponent<FoodAnimController>() ?? gameObject.AddComponent<FoodAnimController>();
    }
    void OnEnable()
    {
        this.RegisterEvent<CreateFoodInstanceEvent>(OnCreateFoodInstanceEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<RemoveFoodInstanceEvent>(OnRemoveFoodInstanceEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<MoveEntityEvent>(OnMoveEntityEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<PlaceEntityEvent>(OnPlaceEntityEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<SwapEntityEvent>(OnSwapEntityEvent).UnRegisterWhenDisabled(this);
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)){
            // 打印食材仓库信息
            Dictionary<string, Food> foodRepositorys = foodSystem.FoodRepositorys();
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
        IEntityView foodInstanceView = GenerateFoodInstanceView(foodInstance);
        foodInstanceView.GO().SetActive(false);
        // 3. 设置父物体
        Transform cellTransform = boardView.GetCellTransform(foodInstance.position);
        float scale = foodInstanceView.GO().transform.localScale.x;
        foodInstanceView.GO().transform.SetParent(cellTransform,true);
        foodInstanceView.GO().transform.localScale = new Vector3(scale, scale, 1);

        Vector3 relativeEnd = new Vector3(0, 0, -1f);
        Vector3 relativeStart = new Vector3(0, SettingManager.Instance.AnimSettings.foodInstanceCreateDistance, 0);
        // 4. 播放移动动画
        // 4.1 创建移动动画任务作为附属动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => {
                if (foodInstanceView == null) return;
                foodInstanceView.GO().SetActive(true);
            }),
            new RelativeMoveAnimationTask(foodInstanceView.GO().transform, 0.3f, cellTransform, relativeEnd, relativeStart)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveAnimDelay;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        // Debug.Log($"【FoodController】将食材实例视图移动到棋盘格子: {cellTransform.position}");
        this.GetSystem<IAnimationSystem>().Append(anim);

        foodInstanceViews.Add(foodInstance.guid, foodInstanceView);
    }
    void OnRemoveFoodInstanceEvent(RemoveFoodInstanceEvent e)
    {
        if (!foodInstanceViews.TryGetValue(e.foodInstance.guid, out IEntityView foodInstanceView)) return;

        //TODO: 播放消失动画
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            foodInstanceViews.Remove(e.foodInstance.guid);
            Destroy(foodInstanceView.GO());
        }));
        this.GetSystem<IAnimationSystem>().Play();
    }
    void OnMoveEntityEvent(MoveEntityEvent e)
    {
        if (!foodInstanceViews.TryGetValue(e.entity.guid, out IEntityView foodInstanceView)) return;

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
        if (!foodInstanceViews.TryGetValue(e.entity.guid, out IEntityView foodInstanceView)) return;

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
        if (!foodInstanceViews.TryGetValue(e.entity1.guid, out IEntityView foodInstanceView1)) return;
        if (!foodInstanceViews.TryGetValue(e.entity2.guid, out IEntityView foodInstanceView2)) return;

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

    
    // 生成食材实例视图
    private IEntityView GenerateFoodInstanceView(FoodInstance foodInstance){
        IEntityView foodInstanceView = Instantiate(foodInstanceViewPrefab, transform).GetComponent<IEntityView>();
        foodInstanceView.Bind(foodInstance);
        return foodInstanceView;
    }

    public IArchitecture GetArchitecture() => 
        GameArchitecture.Interface;
    
    public IEntityView GetFoodInstanceView(string guid) => 
        foodInstanceViews.TryGetValue(guid, out IEntityView foodInstanceView) ? foodInstanceView : null;
}
