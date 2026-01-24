using System;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UniRx; // 确保引用了 UniRx

public class TooltipUI : MonoBehaviour, IController {
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    [Header("Components")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Elements")]
    [SerializeField] private TooltipElement elementPrefab; // 必须拖拽赋值
    [SerializeField] private Transform container;          // 必须拖拽赋值，建议指向自身或子物体
    
    // 简单的对象池，复用 Element
    private List<TooltipElement> activeElements = new List<TooltipElement>();

    public TooltipAnchor anchor = TooltipAnchor.左上角;
    private bool followMouse = true;
    private TooltipParent parent;
    
    // 配置
    private float maxWidth => SettingManager.Instance.DevSettings.tooltipMaxWidth; // 假设你有这个
    private Vector2 padding => SettingManager.Instance.DevSettings.tooltipPadding; // 假设你有这个

    private IDisposable disposable;

    // --- 修改核心：参数改为 List<TooltipInfo> ---
    public void Show(List<TooltipInfo> dataList) {
        if (disposable != null) {
            disposable.Dispose();
            disposable = null;
        }

        // 1. 生成或复用 Elements
        RefreshElements(dataList);

        // 2. 强制刷新布局
        // 因为 ContentSizeFitter 需要一帧或者强制调用才能计算出正确的大小
        // 这对于后续的 GetAnchorPosition 计算边界非常重要
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        // 3. 动画序列
        // 淡入
        IAnimTask anim_显示 = new TweenAnimTask(canvasGroup.DOFade(1, 0.1f).SetEase(Ease.OutSine).SetUpdate(true));
        
        // 你的原逻辑中有一个 "anim_修改" 用于更新尺寸，现在 ContentSizeFitter 会自动处理尺寸
        // 但为了确保位置在尺寸更新后正确修正，我们在下一帧再强制修正一次位置
        IAnimTask anim_修正位置 = new ActionAnimTask(() => {
            Observable.NextFrame().Take(1).Subscribe(_ => {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                UpdatePosition(); // 立即更新一次位置确保不闪烁
            }).AddTo(this);
        });

        IAnimTask anim = new SequenceAnimTask(new List<IAnimTask> { anim_显示, anim_修正位置 });
        this.GetSystem<IAnimationSystem>().DirectlyPlay(anim);
    }

    // 复用 Element 的逻辑
    private void RefreshElements(List<TooltipInfo> dataList) {
        // 如果需要的比现有的多，实例化新的
        while (activeElements.Count < dataList.Count) {
            TooltipElement newEl = Instantiate(elementPrefab, container);
            activeElements.Add(newEl);
        }

        // 遍历设置数据
        for (int i = 0; i < activeElements.Count; i++) {
            if (i < dataList.Count) {
                activeElements[i].SetData(dataList[i], maxWidth);
                activeElements[i].gameObject.SetActive(true);
            } else {
                // 多余的隐藏
                activeElements[i].gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(container as RectTransform);
    }

    public void Hide() {
        if (disposable != null) {
            disposable.Dispose();
            disposable = null;
        }
        IAnimTask anim_隐藏 = new TweenAnimTask(canvasGroup.DOFade(0, 0.1f).SetEase(Ease.OutSine).SetUpdate(true));
        IAnimTask anim_设置跟随类型 = new ActionAnimTask(() => {
            SetFollowType(null);
        });
        IAnimTask anim = new SequenceAnimTask(new List<IAnimTask> { anim_隐藏, anim_设置跟随类型 });

        disposable = anim.Play().Subscribe();
    }

    private void SetFollowType(TooltipParent parent) {
        if (parent != null) {
            this.SetParent(parent);
            this.ChangeFollowMouse(false);
        } else {
            this.SetParent(null);
            this.ChangeFollowMouse(true);
        }
    }

    void Awake() {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update() {
        UpdatePosition();
    }

    // 将位置更新逻辑提取出来，方便手动调用
    private void UpdatePosition() {
        if (followMouse) {
            Vector3 mousePosition = Utility.GetMousePosition2D(); // 假设 Utility 存在
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(mousePosition);
            transform.position = GetAnchorPosition(screenPosition);
        } else {
            if (parent == null) return;
            Vector3 targetPosition = parent.targetTransform.position;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(targetPosition);
            transform.position = GetAnchorPosition(screenPosition);
        }
    }

    public void ChangeFollowMouse(bool followMouse) {
        this.followMouse = followMouse;
    }

    public void SetParent(TooltipParent parent) {
        this.parent = parent;
        if (parent == null) return;
        this.anchor = parent.anchor;
    }

    private Vector2 GetAnchorPosition(Vector3 targetPosition) {
        // --- 修改核心 ---
        // 原逻辑使用 textMesh.rectTransform.rect (文本框大小)
        // 新逻辑使用 rectTransform.rect (TooltipUI 整体容器大小)
        float width = rectTransform.rect.width * rectTransform.lossyScale.x; // 考虑缩放
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;

        switch (anchor) {
            case TooltipAnchor.左上角:
                return RevisedPosition(new Vector2(targetPosition.x - width / 2, targetPosition.y + height / 2));
            case TooltipAnchor.右上角:
                return RevisedPosition(new Vector2(targetPosition.x + width / 2, targetPosition.y + height / 2));
            case TooltipAnchor.左下角:
                return RevisedPosition(new Vector2(targetPosition.x - width / 2, targetPosition.y - height / 2));
            case TooltipAnchor.右下角:
                return RevisedPosition(new Vector2(targetPosition.x + width / 2, targetPosition.y - height / 2));
            case TooltipAnchor.中心:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y));
            case TooltipAnchor.左:
                return RevisedPosition(new Vector2(targetPosition.x - width / 2, targetPosition.y));
            case TooltipAnchor.右:
                return RevisedPosition(new Vector2(targetPosition.x + width / 2, targetPosition.y));
            case TooltipAnchor.上:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y + height / 2));
            case TooltipAnchor.下:
                return RevisedPosition(new Vector2(targetPosition.x, targetPosition.y - height / 2));
            default:
                Debug.LogError("Invalid anchor: " + anchor);
                return Vector2.zero;
        }
    }

    private Vector2 RevisedPosition(Vector2 position) {
        // 同样，这里的尺寸检查也要用 rectTransform
        float width = rectTransform.rect.width * rectTransform.lossyScale.x; 
        float height = rectTransform.rect.height * rectTransform.lossyScale.y;

        float left = position.x - width / 2;
        float right = position.x + width / 2;
        float top = position.y + height / 2;
        float bottom = position.y - height / 2;

        Vector2 revisedPosition = position;
        if (left < 0) {
            revisedPosition.x = width / 2;
        }
        if (right > Screen.width) {
            revisedPosition.x = Screen.width - width / 2;
        }
        if (top > Screen.height) {
            revisedPosition.y = Screen.height - height / 2;
        }
        if (bottom < 0) {
            revisedPosition.y = height / 2;
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