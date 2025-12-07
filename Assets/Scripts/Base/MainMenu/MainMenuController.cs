using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private ChoosePCPanel choosePCPanel;
    void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);
    }
    private void OnStartButtonClick()
    {
        // Debug.Log("【MainMenu】开始游戏");
        // SceneManager.LoadScene("BBQ demo 1");
        ShowChoosePCPanel();
    }
    private void OnExitButtonClick()
    {
        Debug.Log("【MainMenu】退出游戏");
        Application.Quit();
    }

    private void ShowChoosePCPanel()
    {
        choosePCPanel.Show();
    }
}
