using System;
using System.Collections.Generic;
using System.Text;
using cfg;
using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 选择玩家角色面板
/// </summary>
public class ChoosePCPanel : UIPanel, IController{
    [SerializeField] private ChoosePCView choosePCViewPrefab;
    [SerializeField] private Transform choosePCViewsContainer;
    [Header("按钮")]
    [SerializeField] private Button choosePCButton;
    [SerializeField] private Button closeButton;
    [Header("角色信息面板")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private SetSeedPanel setSeedPanel;
    private List<ChoosePCView> choosePCViews = new List<ChoosePCView>();

    private string selectedPCID;

    protected override void Awake() {
        base.Awake();


        choosePCButton.onClick.AddListener(OnChoosePCButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);

        foreach (var pcData in this.GetSystem<IDataSystem>().GetAllPCData())
        {
            ChoosePCView choosePCView = Instantiate(choosePCViewPrefab, choosePCViewsContainer);
            choosePCView.Init(pcData, this);
            choosePCViews.Add(choosePCView);
        }

        selectedPCID = this.GetSystem<IDataSystem>().GetAllPCData()[0].ID;
        OnChoosePC(this.GetSystem<IDataSystem>().GetPCData(selectedPCID));
    }
    private void OnChoosePCButtonClick()
    {
        // Debug.Log("【ChoosePCPanel】选择玩家角色");
        int seed = setSeedPanel.GetSeed();
        LoadingManger.Instance.LoadSceneAsync("BBQ demo 1", () => {
            Debug.Log("【场景加载】完成");
            Debug.Log($"选择玩家角色: {selectedPCID}");
            // 加载完成后（0.5秒后）开始新游戏
            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => {
                this.GetSystem<IProcessSystem>().StartNewGame(new NewGameInfo(selectedPCID, seed));
            }).AddTo(LoadingManger.Instance.gameObject);
        });
    }
    private void OnCloseButtonClick()
    {
        // Debug.Log("【ChoosePCPanel】关闭面板");
        Hide();
    }

    public void OnChoosePC(PCData pcData){
        UpdateInfoPanel(pcData);
        SetButtonInteractable(pcData.ID);
        selectedPCID = pcData.ID;
    }

    private void SetButtonInteractable(string currentPCID){
        choosePCViews.ForEach(view => view.SetButtonInteractable(view.pcID != currentPCID));
    }

    private void UpdateInfoPanel(PCData pcData){
        // 更新角色名称
        nameText.text = pcData.Name;
        // 更新角色描述
        descriptionText.text = GetPCDescription(pcData);
        // 更新角色ID
        selectedPCID = pcData.ID;
    }

    private string GetPCDescription(PCData pcData){
        StringBuilder description = new StringBuilder();
        description.Append(pcData.Description);
        description.Append("\n\n");
        description.Append("<size=44>");
        description.Append(pcData.SkillDescription);
        description.Append("</size>");
        return description.ToString();
    }
}