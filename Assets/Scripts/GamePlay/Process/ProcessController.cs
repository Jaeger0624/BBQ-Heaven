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
    [SerializeField] private GameObject mainGamePlayUI;
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
        mainGamePlayUI.SetActive(false);
    }
    [Button("打开主游戏界面")]
    public void ShowMainGamePlay(){
        mainGamePlayUI.SetActive(true);
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
    }
    private void HideKitchenPanel(){
    }

    private void ShowCustomerPanel(){

    }
    private void HideCustomerPanel(){
    }
    
    
}

public class HideMainGamePlayEvent{}
public class ShowMainGamePlayEvent{}
public class ChangePanelEvent{
    public ProcessPanel newPanel;
    public ChangePanelEvent(ProcessPanel newPanel){
        this.newPanel = newPanel;
    }
}