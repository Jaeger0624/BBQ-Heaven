using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public enum ProcessPanel{
    Kitchen,
    Customer,
}
public class ProcessController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [ReadOnly]
    public ProcessPanel currentPanel = ProcessPanel.Kitchen;
    [SerializeField] private CanvasGroup mainGamePlayUI;
    [SerializeField] private Button changePanelButton;
    void Start()
    {
        HideMainGamePlay();
        changePanelButton.onClick.AddListener(OnChangePanelButtonClick);

        this.RegisterEvent<ChangePanelEvent>(OnChangePanel).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    void OnEnable()
    {
        this.RegisterEvent<ShowMainGamePlayEvent>(OnShowMainGame).UnRegisterWhenDisabled(this.gameObject);
        this.RegisterEvent<HideMainGamePlayEvent>(OnHideMainGame).UnRegisterWhenDisabled(this.gameObject);
    }
    [Button("关闭主游戏界面")]
    public void HideMainGamePlay(){
        // mainGamePlayUI.alpha = 0;
        // mainGamePlayUI.blocksRaycasts = false;
        // mainGamePlayUI.interactable = false;
    }
    [Button("打开主游戏界面")]
    public void ShowMainGamePlay(){
        // mainGamePlayUI.alpha = 1;
        // mainGamePlayUI.blocksRaycasts = true;
        // mainGamePlayUI.interactable = true;
    }
    private void OnShowMainGame(ShowMainGamePlayEvent evt){
        ShowMainGamePlay();
    }
    private void OnHideMainGame(HideMainGamePlayEvent evt){
        HideMainGamePlay();
    }
    private void OnChangePanel(ChangePanelEvent evt){
        if (evt.newPanel == ProcessPanel.Kitchen){
            ShowKitchenPanel();
            HideCustomerPanel();
        }else{
            ShowCustomerPanel();
            HideKitchenPanel();
        }

    }
    private void OnChangePanelButtonClick(){
        if (currentPanel == ProcessPanel.Kitchen){
            ShowCustomerPanel();
            HideKitchenPanel();
            currentPanel = ProcessPanel.Customer;
            this.SendEvent(new ChangePanelEvent(ProcessPanel.Customer));

        }else{
            ShowKitchenPanel();
            HideCustomerPanel();
            currentPanel = ProcessPanel.Kitchen;
            this.SendEvent(new ChangePanelEvent(ProcessPanel.Kitchen));
        }
    }

    // 厨房视图
    private void ShowKitchenPanel(){
        this.SendEvent(new UIPanelEvent(UIPanelType.后厨界面, UIPanelAction.Show));
    }
    private void HideKitchenPanel(){
        this.SendEvent(new UIPanelEvent(UIPanelType.后厨界面, UIPanelAction.Hide));
    }

    private void ShowCustomerPanel(){

        this.SendEvent(new UIPanelEvent(UIPanelType.顾客界面, UIPanelAction.Show));
    }
    private void HideCustomerPanel(){
        this.SendEvent(new UIPanelEvent(UIPanelType.顾客界面, UIPanelAction.Hide));
    }
    
    
}

public class HideMainGamePlayEvent : AbstractEvent{}
public class ShowMainGamePlayEvent : AbstractEvent{}
public class ChangePanelEvent : AbstractEvent{
    public ProcessPanel newPanel;
    public ChangePanelEvent(ProcessPanel newPanel){
        this.newPanel = newPanel;
    }
}