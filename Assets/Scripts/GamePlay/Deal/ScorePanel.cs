// 得分界面
using System;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;

public class ScorePanel : MonoBehaviour, IController, ICanSendEvent{
    [SerializeField] private TextMeshProUGUI scoreText;
    private CanvasGroup canvasGroup;
    private int currentScore = 0;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private float shakeDuration => SettingManager.Instance.AnimSettings.ScoreTextAnimDuration;
    private bool isUpdateNumber = false;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }
    private void Start() {
        this.RegisterEvent<UpdateScoreViewEvent>(OnUpdateScoreViewEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<ShowScoreTextEvent>(OnShowScoreTextEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<ShowScorePanelEvent>(OnShowScorePanelEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<CloseScorePanelEvent>(OnCloseScorePanelEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }

    [Button]
    private void Show(){
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.5f).SetEase(Ease.OutElastic).SetUpdate(true);
        canvasGroup.DOFade(1, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    [Button]
    private void Hide(bool instant = false){
        if (instant){
            canvasGroup.alpha = 0;
        }
        else{
            canvasGroup.DOFade(0, 0.2f).SetEase(Ease.OutSine).SetUpdate(true);
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private void Update() {
        if (isUpdateNumber) {
            scoreText.text = $"{currentScore.ToString()}";
        }
    }


    private void OnUpdateScoreViewEvent(UpdateScoreViewEvent evt){
        isUpdateNumber = true;
        int newScore = evt.currentScore;
        DOTween.To(() => currentScore, x => currentScore = x, newScore, shakeDuration).SetEase(Ease.OutSine).SetUpdate(true);

        ShakeNumber();

        if (evt.multiplierText != null){  
            FloatingTextManager.Instance.Show(transform.position, evt.multiplierText, Color.white, new FloatingTextInfo(evt.multiplierText, 1.2f, Color.white));
        }
    }

    private void OnShowScoreTextEvent(ShowScoreTextEvent evt){
        isUpdateNumber = false;
        scoreText.text = evt.scoreText;

        ShakeNumber();


    }

    private void OnShowScorePanelEvent(ShowScorePanelEvent evt){
        Show();
    }

    private void OnCloseScorePanelEvent(CloseScorePanelEvent evt){
        Hide();
        isUpdateNumber = false;
        currentScore = 0;
    }

    private void ShakeNumber(){
        scoreText.transform.DOShakePosition(shakeDuration, 10, 10, 90, false, true).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(() => {
            scoreText.transform.localPosition = Vector3.zero;
        });
        scoreText.transform.DOShakeRotation(shakeDuration, 10, 10, 90, false).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(() => {

            scoreText.transform.localRotation = Quaternion.identity;
        });
        scoreText.transform.DOScale(1.2f, shakeDuration).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(() => {
            scoreText.transform.DOScale(1f, shakeDuration/2).SetEase(Ease.OutElastic).SetUpdate(true);
        });

        this.transform.DOShakePosition(shakeDuration, 10, 10, 90, false, true).SetEase(Ease.OutSine).SetUpdate(true).OnComplete(() => {
            this.transform.localPosition = Vector3.zero;
        });
    }

    [Button]
    public void TestShakeNumber(){
        this.SendEvent(new ChangeScoreEvent(100));
        Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => {
            this.SendEvent(new UpdateScoreViewEvent(currentScore + 100));
        }).AddTo(this);
    }
}




public class UpdateScoreViewEvent : AbstractEvent{
    public int currentScore;
    public string multiplierText;
    public UpdateScoreViewEvent(int currentScore, string multiplierText = null){
        this.multiplierText = multiplierText;
        this.currentScore = currentScore;
    }
}

public class ShowScoreTextEvent : AbstractEvent{
    public string scoreText;
    public ShowScoreTextEvent(string scoreText){
        this.scoreText = scoreText;
    }
}

public class CloseScorePanelEvent : AbstractEvent{
    public CloseScorePanelEvent(){
    }
}

public class ShowScorePanelEvent : AbstractEvent{
    public ShowScorePanelEvent(){
    }
}