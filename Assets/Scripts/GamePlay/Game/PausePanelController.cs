using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour, IController
{
    // 显示在游戏中的
    [SerializeField] private Button pauseButton;
    [SerializeField] private PausePanel pausePanel;

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void Awake()
    {
        pauseButton.onClick.AddListener(OnPauseButtonClick);
    }
    private void OnPauseButtonClick()
    {
        pausePanel.Show();
        Debug.Log("【PausePanelController】暂停游戏");
    }
    
}
