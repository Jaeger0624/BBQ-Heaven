using System.Collections.Generic;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReputationInfoView : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private ReputationData reputationData;
    [SerializeField] private TextMeshProUGUI reputationNameText;
    [SerializeField] private TextMeshProUGUI reputationDescriptionText;
    [SerializeField] private TextMeshProUGUI reputationLevelText;
    [SerializeField] private Transform reputationLevelParent;
    [SerializeField] private GameObject reputationEffectTextPrefab;
    private List<TextMeshProUGUI> reputationEffectTexts;
    public void Bind(ReputationData reputationData){
        this.reputationData = reputationData;
        reputationEffectTexts = new List<TextMeshProUGUI>();
        UpdateVisual();
    }
    private void UpdateVisual(){
        if (reputationData == null){
            Debug.LogError("ReputationData is null");
            return;
        }
        reputationNameText.text = reputationData.Name;
        if (reputationDescriptionText != null){
            reputationDescriptionText.text = reputationData.Description;
        }
        reputationLevelText.text = "所需声望：" + reputationData.TotalReputation.ToString();

        foreach (var reputationEffect in reputationData.Effects){
            GameObject reputationEffectTextObject = Instantiate(reputationEffectTextPrefab, reputationLevelParent);
            reputationEffectTextObject.SetActive(true);
            TextMeshProUGUI reputationEffectText = reputationEffectTextObject.GetComponent<TextMeshProUGUI>();
            reputationEffectText.text = "· " +reputationEffect;
            reputationEffectTexts.Add(reputationEffectText);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}