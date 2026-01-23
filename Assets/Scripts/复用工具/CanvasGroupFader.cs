using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isVisible = true;
    void Start()
    {
        FadeIn();
        isVisible = true;
    }
    public void FadeIn(float duration = 0.3f){
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        // canvasGroup.alpha = 1;
        canvasGroup.DOFade(1, duration).SetEase(Ease.OutSine).SetUpdate(true);

    }
    public void FadeOut(float duration = 0.3f){
        canvasGroup.DOFade(0, duration).SetEase(Ease.OutSine).SetUpdate(true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        // canvasGroup.alpha = 0;
    }

    public void ChangeVisible(){
        if (isVisible){
            FadeOut();
        }
        else{
            FadeIn();
        }
        isVisible = !isVisible;
    }
}
