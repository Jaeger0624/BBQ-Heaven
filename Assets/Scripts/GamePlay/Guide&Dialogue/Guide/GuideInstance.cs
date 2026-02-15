
using QFramework;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GuideInstance : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Image guideImage;
    [SerializeField] private TextMeshProUGUI guideText;
    [SerializeField] private Transform ScaleTransform;
    
    public void ShowGuide(string content, Transform targetTransform, GuideTextDirection textDirection)
    {
        // 0. 设置目标位置
        transform.position = targetTransform.position;
        // 1. 设置文字
        guideText.text = content;
        bool isRight = textDirection == GuideTextDirection.右;
        // 2. 设置图片
        ScaleTransform.localScale = new Vector3(isRight ? 1 : -1, 1, 1);
        float textDirectionInt = isRight ? 1 : -1;
        guideText.transform.localScale = new Vector3(textDirectionInt, 1, 1);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(guideText.transform as RectTransform);

        guideText.ForceMeshUpdate();

        Show();
    }

    public void Show()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject).SetUpdate(true);
    }

    public void Hide()
    {
        transform.DOScale(0, 0.3f).SetEase(Ease.OutSine).SetLink(gameObject).SetUpdate(true);
    }
}