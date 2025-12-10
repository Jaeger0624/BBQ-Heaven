using System;
using QFramework;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour, IController
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private ChoosePCPanel choosePCPanel;
    void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClick);
        exitButton.onClick.AddListener(OnExitButtonClick);
        continueButton.onClick.AddListener(OnContinueButtonClick);
    }
    void Start()
    {
        // 检查是否存在存档
        if (this.GetSystem<ISaveSystem>().GetGameArchive() != null)
        {
            continueButton.interactable = true;
        }
        else{
            continueButton.interactable = false;
        }
    }
    private void OnContinueButtonClick()
    {
        GameArchive gameArchive = this.GetSystem<ISaveSystem>().GetGameArchive();
        LoadingManger.Instance.LoadSceneAsync("BBQ demo new", () => {
            Debug.Log("【场景加载】完成");
            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => {
                this.GetSystem<IProcessSystem>().LoadGame(gameArchive);
            }).AddTo(LoadingManger.Instance.gameObject);
        });
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

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
