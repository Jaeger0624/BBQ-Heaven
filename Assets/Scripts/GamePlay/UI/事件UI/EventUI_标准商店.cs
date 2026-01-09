using System.Linq;
using QFramework;
using UnityEngine;

// 标准商店功能：购买食材、吉祥物，删食材入口
public class EventUI_标准商店 : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private ButtonUI deleteFoodButton;

    void Start()
    {
        deleteFoodButton.OnClick.AddListener(OnDeleteFoodButtonClick);
    }
    private void OnDeleteFoodButtonClick(){

        // 1. 发送食材展示事件
        this.SendEvent(new FoodSelectPanelEvent(FoodContainerType.多选删除, "删除食材", 
        this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList().Select(food => new FoodUIContext(food)).ToList(), 3));

        // 2. 发送UIPanel事件
        this.SendEvent(new UIPanelEvent(UIPanelType.食材展示界面, UIPanelAction.Show));
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
