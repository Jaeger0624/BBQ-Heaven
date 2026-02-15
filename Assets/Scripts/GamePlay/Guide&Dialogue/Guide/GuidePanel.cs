using UnityEngine;
using UnityEngine.UI;
using QFramework;
using TMPro;

public class GuidePanel : MonoBehaviour, IController // 继承你自己的基类
{
    [Header("UI组件")]
    public GuideRaycastFilter MaskFilter; // 拖入挂了Filter的全屏黑色Image
    public RectTransform HollowRect;   // 拖入那个透明的高亮框 Image
    [SerializeField] private GuideInstance guideInstance;
    public static GuidePanel Instance { get; private set; }

    [SerializeField] private float margin = 10f;

    private void Awake()
    {
        Instance = this;
        // 初始隐藏
        HideGuide();
    }
    void Start()
    {
    }

    public void ShowGuideFocus(RectTransform rectTransform,
        string content,
        Transform targetTransform,
        GuideTextDirection textDirection,
        bool isConcerned,
        bool isShow = true)
    {
        gameObject.SetActive(true);

        MaskFilter.gameObject.SetActive(isConcerned);
        HollowRect.gameObject.SetActive(isShow);

        // 1. 设置遮罩挖孔位置
        // 将世界坐标转为本地坐标
        // 注意：根据你的Canvas模式（Overlay/Camera），这里可能需要调整
        // 下面是通用的 Overlay/Camera 兼容写法
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, rectTransform.position), // 假设是Overlay模式相机填null
            null, // Overlay模式填null，Camera模式填 uiCamera
            out Vector2 localPos
        );

        HollowRect.position = rectTransform.position;
        // 切换pivot为中心
        HollowRect.pivot = rectTransform.pivot;
        HollowRect.sizeDelta = rectTransform.sizeDelta + new Vector2(margin * 2, margin * 2);

        guideInstance.ShowGuide(content, targetTransform, textDirection);
    }

    public void HideGuide()
    {
        gameObject.SetActive(false);
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}