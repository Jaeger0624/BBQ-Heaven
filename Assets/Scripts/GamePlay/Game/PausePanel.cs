using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour, IController
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
        Hide();
    }

    void OnEnable()
    {
        this.RegisterEvent<GameOverEvent>(OnGameOverEvent).UnRegisterWhenDisabled(this.gameObject);
    }

    public void OnContinueButtonClick()
    {
        // Debug.Log("【PausePanel】继续游戏");
        Hide();
    }
    public void Show(){
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void Hide(){
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
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
