using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

public class ReputationController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private ReputationInfoView reputationInfoViewPrefab;
    [SerializeField] private Transform reputationInfoViewParent;

    private List<ReputationInfoView> reputationInfoViews = new List<ReputationInfoView>();

    public void Build(){
        Clear();

        List<ReputationData> reputationDatas = this.GetSystem<IDataSystem>().GetAllReputationData();
        foreach (var reputationData in reputationDatas){
            ReputationInfoView reputationInfoView = Instantiate(reputationInfoViewPrefab, reputationInfoViewParent);
            reputationInfoView.gameObject.SetActive(true);
            reputationInfoView.Bind(reputationData);

            reputationInfoViews.Add(reputationInfoView);
        }

        // Debug.Log("ReputationController Build: " + reputationInfoViews.Count);
        
    }

    public void Clear(){
        foreach (var reputationInfoView in reputationInfoViews){
            Destroy(reputationInfoView.gameObject);
        }
        reputationInfoViews.Clear();
    }
}