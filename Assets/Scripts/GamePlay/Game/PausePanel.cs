using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : UIPanel
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    
    protected override void Awake()
    {
        base.Awake();
        
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

    void OnEnable()
    {
        this.RegisterEvent<GameOverEvent>(OnGameOverEvent).UnRegisterWhenDisabled(this.gameObject);
    }

    public void OnContinueButtonClick()
    {
        // Debug.Log("【PausePanel】继续游戏");
        Hide();
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
}
