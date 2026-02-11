using DG.Tweening;
using QFramework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreController : MonoBehaviour, IController
{
    [SerializeField] private string scoreTextPrefix = "当前:";
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string targetScoreTextPrefix = "目标:";
    [SerializeField] private TextMeshProUGUI targetScoreText;

    private int currentScore = 0;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void Start()
    {
        scoreText.text = scoreTextPrefix + "0";
        this.RegisterEvent<ChangeScoreEvent>(OnChangeScoreEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<SetTargetScoreEvent>(OnSetTargetScoreEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    void Update()
    {
        scoreText.text = scoreTextPrefix + currentScore.ToString();
    }

    private void OnChangeScoreEvent(ChangeScoreEvent evt)
    {
        // 播放现金入账动画
        // 数字增长：
        int newScore = this.GetSystem<IScoreSystem>().Score;
        
        DOTween.To(() => currentScore, x => currentScore = x, newScore, 0.8f).SetEase(Ease.OutSine).SetUpdate(true);
    }

    private void OnSetTargetScoreEvent(SetTargetScoreEvent evt)
    {
        targetScoreText.text = targetScoreTextPrefix + evt.targetScore.ToString();
    }
}
