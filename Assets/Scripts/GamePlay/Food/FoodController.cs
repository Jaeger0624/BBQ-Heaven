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
    private Dictionary<string, FoodInstanceView> foodInstanceViews = new Dictionary<string, FoodInstanceView>();
    void Awake()
    {
        foodAnimController = gameObject.GetComponent<FoodAnimController>() ?? gameObject.AddComponent<FoodAnimController>();
    }
    void OnEnable()
    {
        this.RegisterEvent<CreateFoodInstanceEvent>(OnCreateFoodInstanceEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<RemoveFoodInstanceEvent>(OnRemoveFoodInstanceEvent).UnRegisterWhenDisabled(this);
        this.RegisterEvent<MoveFoodInstanceEvent>(OnMoveFoodInstanceEvent).UnRegisterWhenDisabled(this);
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
        FoodInstanceView foodInstanceView = GenerateFoodInstanceView(foodInstance);
        foodInstanceView.gameObject.SetActive(false);
        // 3. 设置父物体
        Transform cellTransform = boardView.GetCellTransform(foodInstance.position);
        float scale = foodInstanceView.transform.localScale.x;
        foodInstanceView.transform.SetParent(cellTransform,true);
        foodInstanceView.transform.localScale = new Vector3(scale, scale, 1);

        Vector3 relativeEnd = new Vector3(0, 0, -1f);
        Vector3 relativeStart = new Vector3(0, SettingManager.Instance.AnimSettings.foodInstanceCreateDistance, 0);
        // 4. 播放移动动画
        // 4.1 创建移动动画任务作为附属动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => {
                if (foodInstanceView == null) return;
                foodInstanceView.gameObject.SetActive(true);
            }),
            new RelativeMoveAnimationTask(foodInstanceView.transform, 0.3f, cellTransform, relativeEnd, relativeStart)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveAnimDelay;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        // Debug.Log($"【FoodController】将食材实例视图移动到棋盘格子: {cellTransform.position}");
        this.GetSystem<IAnimationSystem>().Append(anim);

        foodInstanceViews.Add(foodInstance.guid, foodInstanceView);
    }
    void OnRemoveFoodInstanceEvent(RemoveFoodInstanceEvent e)
    {
        if (!foodInstanceViews.TryGetValue(e.foodInstance.guid, out FoodInstanceView foodInstanceView)) return;

        //TODO: 播放消失动画
        this.GetSystem<IAnimationSystem>().Append(new ActionAnimTask(() => {
            foodInstanceViews.Remove(e.foodInstance.guid);
            Destroy(foodInstanceView.gameObject);
        }));
        this.GetSystem<IAnimationSystem>().Play();
    }
    void OnMoveFoodInstanceEvent(MoveFoodInstanceEvent e)
    {
        if (!foodInstanceViews.TryGetValue(e.guid, out FoodInstanceView foodInstanceView)) return;

        Vector3 originPos = foodInstanceView.transform.position;
        // 1. 获取食材实例视图的父物体
        Transform cellTransform = boardView.GetCellTransform(e.targetPosition);
        float scale = foodInstanceView.transform.localScale.x;
        foodInstanceView.transform.SetParent(cellTransform,true);
        foodInstanceView.transform.localScale = new Vector3(scale, scale, 1);

        // 2. 计算目标位置
        Vector3 targetPosition = cellTransform.position;
        targetPosition.z = foodInstanceView.transform.position.z - 0.1f;

        // 3. 播放移动动画
        var attachedAnimTasks = new List<IAnimTask>
        {
            new ActionAnimTask(() => foodInstanceView.gameObject.SetActive(true)),
            new MoveAnimationTask(foodInstanceView.transform, 0.4f, targetPosition, originPos)   
        };
        float delay = SettingManager.Instance.AnimSettings.foodInstanceMoveDelay_棋盘上移动;
        IAnimTask anim = new AttatchedAnimTask(new DelayAnimTask(delay), attachedAnimTasks);
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);
    }
    // 生成食材实例视图
    private FoodInstanceView GenerateFoodInstanceView(FoodInstance foodInstance){
        FoodInstanceView foodInstanceView = Instantiate(foodInstanceViewPrefab, transform).GetComponent<FoodInstanceView>();
        foodInstanceView.Bind(foodInstance);
        return foodInstanceView;
    }

    public IArchitecture GetArchitecture() => 
        GameArchitecture.Interface;
    
    public FoodInstanceView GetFoodInstanceView(string guid) => 
        foodInstanceViews.TryGetValue(guid, out FoodInstanceView foodInstanceView) ? foodInstanceView : null;
}
