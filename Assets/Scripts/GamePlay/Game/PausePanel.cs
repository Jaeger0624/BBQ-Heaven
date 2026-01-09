using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private CanvasGroup canvasGroup;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 确保按钮引用不为空后再添加监听器
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClick);
        }
        else
        {
            Debug.LogError("【PausePanel】continueButton 未分配！");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClick);
        }
        else
        {
            Debug.LogError("【PausePanel】exitButton 未分配！");
        }
    }

    void Start()
    {
        this.SendEvent(new UIPanelEvent(UIPanelType.暂停界面, UIPanelAction.Hide));
    }

    void OnEnable()
    {
        this.RegisterEvent<GameOverEvent>(OnGameOverEvent).UnRegisterWhenDisabled(this.gameObject);
    }

    public void OnContinueButtonClick()
    {
        // Debug.Log("【PausePanel】继续游戏");
        this.SendEvent(new UIPanelEvent(UIPanelType.暂停界面, UIPanelAction.Hide));
    }
    public void OnExitButtonClick()
    {
        // Debug.Log("【PausePanel】退出游戏");
        SceneManager.LoadSceneAsync("MainMenu");

        GameArchitecture.ResetGame();
    }

    private void OnGameOverEvent(GameOverEvent evt)
    {
        OnExitButtonClick();
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}
