using DG.Tweening;
using UnityEngine;

public class SwingAnim : MonoBehaviour
{
    [SerializeField] private float distance = 10f;
    [SerializeField] private float duration = 2f;
    void Start()
    {
        transform.DOLocalMoveY(distance, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }
}
