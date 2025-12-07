using DG.Tweening;
using QFramework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreController : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI targetScoreText;

    private int currentScore = 0;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    void Start()
    {
        scoreText.text = $"当前:0";
        this.RegisterEvent<ChangeScoreEvent>(OnChangeScoreEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        this.RegisterEvent<SetTargetScoreEvent>(OnSetTargetScoreEvent).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    void Update()
    {
        scoreText.text = $"当前:{currentScore.ToString()}";
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
        targetScoreText.text = $"目标:{evt.targetScore.ToString()}";
    }
}
