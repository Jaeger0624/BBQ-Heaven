using System.Collections.Generic;
using cfg;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TagViewComponent : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI conditionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;
    void Start()
    {
        canvasGroup.alpha = 0;
    }
    public void Bind(TagCGA tagCGA){
        descriptionText.text = tagCGA.GADescription;
        conditionText.text = tagCGA.CDDescription;
        SetColor(tagCGA.Type);
        Show();
    }
    public void Show(){
        this.GetSystem<IAnimationSystem>().DirectlyPlay(ShowAnim());
    }
    public void Hide(){
        this.GetSystem<IAnimationSystem>().DirectlyPlay(HideAnim());
    }
    public IAnimTask ShowAnim(){
        SequenceAnimTask sequenceAnimTask = new SequenceAnimTask(new List<IAnimTask>{
            new TweenAnimTask(canvasGroup.DOFade(1, 0.15f).SetUpdate(true)),
            new ActionAnimTask(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }),
        });
        return sequenceAnimTask;
    }
    public IAnimTask HideAnim(){
        SequenceAnimTask sequenceAnimTask = new SequenceAnimTask(new List<IAnimTask>{
            new ActionAnimTask(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }),
            new TweenAnimTask(canvasGroup.DOFade(0, 0.15f).SetUpdate(true)),
        });
        return sequenceAnimTask;
    }

    private void SetColor(CustomerTagType type){
        switch (type){
            case CustomerTagType.中立:
                image.color = Color.white;
                break;
            case CustomerTagType.正面:
                image.color = Color.green;
                break;
            case CustomerTagType.负面:
                image.color = Color.red;
                break;
        }
    }
}