using DG.Tweening;
using UnityEngine;

public class ChangeVisibleButton : MonoBehaviour
{
    [SerializeField] private ButtonUI button;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    private bool isVisible = true;
    void Start()
    {
        button.OnClick.AddListener(OnButtonClick);
    }
    void OnDestroy()
    {
        button.OnClick.RemoveListener(OnButtonClick);
    }
    private void OnButtonClick()
    {
        if (isVisible){
            Hide();
        }
        else{
            Show();
        }   
    }
    private void Hide()
    {
        isVisible = false;
        canvasGroup.DOFade(0, fadeDuration).SetEase(Ease.OutSine).SetUpdate(true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    private void Show()
    {
        isVisible = true;
        canvasGroup.DOFade(1, fadeDuration).SetEase(Ease.OutSine).SetUpdate(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
}
