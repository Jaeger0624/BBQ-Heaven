using System.Collections.Generic;
using System.Linq;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
public class FoodSelectPanelEvent : AbstractEvent{
    public FoodContainerType foodContainerType;
    public string title;
    public List<FoodUIContext> foods;
    public int maxCount;
    public FoodSelectPanelEvent(FoodContainerType foodContainerType, string title, List<FoodUIContext> foods, int maxCount = 0){
        this.foods = foods;
        this.foodContainerType = foodContainerType;
        this.title = title;
        this.maxCount = maxCount;

        if (foodContainerType == FoodContainerType.多选删除 && maxCount == 0){
            Debug.LogError("FoodSelectPanelEvent 的 maxCount 为0");
            return;
        }
    }
}

// 单独再封装一层，避免直接使用FoodDisplayContainer
public class SelectPanel_食材 : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private FoodDisplayContainer foodDisplayContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private ButtonUI confirmButton;
    void Start()
    {
        this.RegisterEvent<FoodSelectPanelEvent>(OnShowEvent);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<FoodSelectPanelEvent>(OnShowEvent);
    }
    private void OnShowEvent(FoodSelectPanelEvent evt){
        // 1. 设置标题
        titleText.text = evt.title;
        // 2. 设置确认按钮
        confirmButton.OnClick.RemoveAllListeners();
        // 3. 创建选择策略
        IItemInteractStrategy<FoodUIContext, DisplayFoodView> itemInteractStrategy = null;
        switch (evt.foodContainerType){
            case FoodContainerType.单选删除:
                itemInteractStrategy = new DeleteFoodStrategy(() => {
                    this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Hide));
                });
                confirmButton.gameObject.SetActive(false);
                break;
            case FoodContainerType.多选删除:
                SelectionContext<FoodUIContext> context = new SelectionContext<FoodUIContext>(evt.maxCount);
                confirmButton.gameObject.SetActive(true);
                confirmButton.SetText($"确认({0}/{evt.maxCount})");
                // 绑定数量变化事件
                context.OnCountChanged += (count) => {
                    confirmButton.SetText($"确认({count}/{evt.maxCount})");
                };
                // 绑定确认事件
                context.OnConfirm += (items) => {
                    foreach (var item in items){
                        this.GetSystem<IFoodSystem>().DeleteFoodFromRepository(item.RuntimeFood);
                    }

                };
                // 绑定确认按钮点击事件
                confirmButton.OnClick.AddListener(() => {
                    context.Confirm();
                    this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Hide));    
                });
                // 初始化选择状态
                itemInteractStrategy = new MultiDeleteFoodStrategy(context);
                break;
            default:
            Debug.LogError("FoodSelectPanelEvent 的 foodContainerType 为空");
            break;
        }
        foodDisplayContainer.RefreshUI(evt.foods, itemInteractStrategy);
    }

    [Button]
    private void TestSingleDelete(){
        this.SendEvent(new FoodSelectPanelEvent(FoodContainerType.单选删除, "删除食材", this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList().Select(food => new FoodUIContext(food)).ToList()));
        this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Show));
    }

    [Button]
    private void TestMultiDelete(){
        this.SendEvent(new FoodSelectPanelEvent(FoodContainerType.多选删除, "删除食材", this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList().Select(food => new FoodUIContext(food)).ToList(), 3));
        this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Show));
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}