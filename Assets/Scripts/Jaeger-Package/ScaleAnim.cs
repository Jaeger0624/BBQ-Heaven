using DG.Tweening;
using UnityEngine;

public class ScaleAnim : MonoBehaviour
{
    [SerializeField] private float scaleMin = 1f;
    [SerializeField] private float scaleMax = 1.5f;  
    [SerializeField] private float duration = 0.2f;
    void Start()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(scaleMax, duration).SetEase(Ease.OutSine).UnScaledKill(gameObject));
        seq.Append(transform.DOScale(scaleMin, duration).SetEase(Ease.OutSine).UnScaledKill(gameObject));
        seq.SetLoops(-1, LoopType.Yoyo);
        seq.UnScaledKill(gameObject);
    }
    private void OnDestroy()
    {
        transform.DOKill(true);
    }
}
