using System;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private RectTransform ImageRectTransform;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    public TooltipAnchor anchor = TooltipAnchor.左上角;
    private bool followMouse = true;
    private TooltipParent parent;
    private float maxWidth => SettingManager.Instance.DevSettings.tooltipMaxWidth;
    private Vector2 padding => SettingManager.Instance.DevSettings.tooltipPadding;

    private IDisposable disposable;
    public void Show(string tooltipText){
        if (disposable != null){
            disposable.Dispose();
            disposable = null;
        }

        textMesh.text = tooltipText;
        // 强制更新文本框的大小
        textMesh.ForceMeshUpdate();

        // 创建一个显示动画
        IAnimTask anim_显示 = new TweenAnimTask(canvasGroup.DOFade(1, 0.1f).SetEase(Ease.OutSine).SetUpdate(true));
        IAnimTask anim_修改 = new ActionAnimTask(() => {
            Observable.NextFrame().Take(1).Subscribe(_ => {
                // 更新文本框的大小
                ImageRectTransform.sizeDelta = new Vector2(textMesh.rectTransform.rect.width, textMesh.rectTransform.rect.height);
            }).AddTo(this);
        });
        IAnimTask anim = new SequenceAnimTask(new List<IAnimTask>{ anim_修改, anim_显示 });

        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);

        
    }
    public void Hide(){
        if (disposable != null){
            disposable.Dispose();
            disposable = null;
        }
        IAnimTask anim_隐藏 = new TweenAnimTask(canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutSine).SetUpdate(true));
        IAnimTask anim_设置跟随类型 = new ActionAnimTask(() => {
            SetFollowType(null);
        });
        IAnimTask anim = new SequenceAnimTask(new List<IAnimTask>{ anim_隐藏, anim_设置跟随类型 });

        disposable = anim.Play().Subscribe();
    }
    private void SetFollowType(TooltipParent parent){
        if (parent != null){
            this.SetParent(parent);
            this.ChangeFollowMouse(false);
        }
        else{
            this.SetParent(null);
            this.ChangeFollowMouse(true);
        }
    }
    void Awake()
    {
        rectTransform = transform.GetComponent<RectTransform>();
    }
    void Start()
    {
    }
    void Update()
    {
        float height = textMesh.rectTransform.rect.height;
        // 设置文本框的最大宽度
        textMesh.rectTransform.sizeDelta = new Vector2(maxWidth, height);

        if (followMouse)
        {
            Vector3 mousePosition = Utility.GetMousePosition2D();
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(mousePosition);
            transform.position = GetAnchorPosition(screenPosition);
        }
        else{
            if (parent == null) return;
            Vector3 targetPosition = parent.targetTransform.position;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPosition);
            transform.position = GetAnchorPosition(screenPosition);
        }
    }

    public void ChangeFollowMouse(bool followMouse){
        this.followMouse = followMouse;
    }
    public void SetParent(TooltipParent parent){
        this.parent = parent;
        if (parent == null) return;
        this.anchor = parent.anchor;
    }
    private Vector2 GetAnchorPosition(Vector3 targetPosition){
        switch (anchor){
            case TooltipAnchor.左上角:
                return RevisedPosition(new Vector2(targetPosition.x - textMesh.rectTransform.rect.width / 2, targetPosition.y + textMesh.rectTransform.rect.height / 2));
            case TooltipAnchor.右上角:
                return RevisedPosition(new Vector2(targetPosition.x + textMesh.rectTransform.rect.width / 2, targetPosition.y + textMesh.rectTransform.rect.height / 2));
            case TooltipAnchor.左下角:
                return RevisedPosition(new Vector2(targetPosition.x - textMesh.rectTransform.rect.width / 2, targetPosition.y - textMesh.rectTransform.rect.height / 2));
            case TooltipAnchor.右下角:
                return RevisedPosition(new Vector2(targetPosition.x + textMesh.rectTransform.rect.width / 2, targetPosition.y - textMesh.rectTransform.rect.height / 2));
            case TooltipAnchor.中心:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y));
            case TooltipAnchor.左:
                return RevisedPosition(new Vector2(targetPosition.x - textMesh.rectTransform.rect.width / 2, targetPosition.y));
            case TooltipAnchor.右:
                return RevisedPosition(new Vector2(targetPosition.x + textMesh.rectTransform.rect.width / 2, targetPosition.y));
            case TooltipAnchor.上:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y + textMesh.rectTransform.rect.height / 2));
            case TooltipAnchor.下:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y - textMesh.rectTransform.rect.height / 2));
            default:
                Debug.LogError("Invalid anchor: " + anchor);
                return Vector2.zero;
        }
    }

    private Vector2 RevisedPosition(Vector2 position){
        float left = position.x - textMesh.rectTransform.rect.width / 2;
        float right = position.x + textMesh.rectTransform.rect.width / 2;
        float top = position.y + textMesh.rectTransform.rect.height / 2;
        float bottom = position.y - textMesh.rectTransform.rect.height / 2;

        Vector2 revisedPosition = position;
        if (left < 0){
            revisedPosition.x = textMesh.rectTransform.rect.width / 2;
        }
        if (right > Screen.width){
            revisedPosition.x = Screen.width - textMesh.rectTransform.rect.width / 2;
        }
        if (top > Screen.height){
            revisedPosition.y = Screen.height - textMesh.rectTransform.rect.height / 2;
        }
        if (bottom < 0){
            revisedPosition.y = textMesh.rectTransform.rect.height / 2;
        }
        return revisedPosition;
    }

}


public enum TooltipAnchor{
    左上角,
    右上角,
    左下角,
    右下角,
    中心,
    左,
    右,
    上,
    下,
}