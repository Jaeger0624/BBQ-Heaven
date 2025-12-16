using System;
using QFramework;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour, IController, ICanSendEvent
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
        this.SendEvent(new UIPanelEvent(UIPanelType.主菜单交互界面, UIPanelAction.Show));
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
        // 1. 清除当前新游戏信息
        this.GetSystem<BlackboardSystem>().newGameInfo = new NewGameInfo("1", UnityEngine.Random.Range(0, 1000000), null, null);


        // 2. 显示选择玩家角色面板
        this.SendEvent(new UIPanelEvent(UIPanelType.选关界面, UIPanelAction.Show));
        this.SendEvent(new UIPanelEvent(UIPanelType.主菜单交互界面, UIPanelAction.Hide));
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
