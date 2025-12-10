using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHandView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
, IController, ICanSendEvent
{
    public Card card { get; private set; }
    
    [Header("UI Components")]
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Image cardImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    [HideInInspector] public Vector3 targetPos;
    [HideInInspector] public Quaternion targetRot;
    [HideInInspector] public Vector3 targetScale;
    
    // 状态标记
    public bool IsHovered { get; private set; } = false;
    public bool IsDragging { get; private set; } = false;
    
    // --- 修改点：改为 Public 属性，让 Manager 可以读取 ---
    public bool IsTargetingMode => card != null && card.targetType != CardTargetType.无;

    // 平滑参数
    private float moveSpeed = 15f;
    private float rotSpeed = 10f;
    private float scaleSpeed = 15f;

    private bool isUnScaledTime = true;
    private float deltaTime => isUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private void Awake()
    {
        if(rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if(canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        targetScale = Vector3.one;
    }

    public void Init(Card card)
    {
        if (card == null){
            Debug.LogError("Card is null");
            return;
        }
        this.card = card;
        UpdateVisual();
    }

    private void UpdateVisual() 
    { 
        // 更新图片和文字
        nameText.text = card.name;
        descriptionText.text = card.description;
        costText.text = card.cost.ToString();
    }

    private void Update()
    {
        // --- 修改点：逻辑分流 ---
        if (card == null){
            Debug.LogError("Card is null");
            return;
        }
        // 1. 普通拖拽：完全由 OnDrag 控制位置，Update 不干涉 (return)
        if (IsDragging && !IsTargetingMode) return;

        // 2. 瞄准模式 OR 静止/悬停：由 Manager 计算 targetPos，Update 负责平滑飞过去
        // 这样卡牌在瞄准时会自动飞到（或保持在）Manager 指定的“选中位置”
        
        rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPos, deltaTime * moveSpeed);
        rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, targetRot, deltaTime * rotSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HandVisualManager.Instance.IsDraggingAnyCard) return;
        IsHovered = true;
        HandVisualManager.Instance.OnCardHoverEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HandVisualManager.Instance.IsDraggingAnyCard) return;
        IsHovered = false;
        HandVisualManager.Instance.OnCardHoverExit(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsDragging = true;
        HandVisualManager.Instance.IsDraggingAnyCard = true;
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false; 
        IsHovered = false; 

        if (IsTargetingMode)
        {
            this.GetSystem<ICardSystem>().State = CardSystemState.选择目标;
            HighLight(card.targetType);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsTargetingMode)
        {
            // --- 瞄准模式逻辑 ---
            // 1. 卡牌位置：不需要在这里写代码！
            // 因为 IsDragging && !IsTargetingMode 为 false，所以 Update() 会继续运行。
            // Manager 会计算出这张卡应该在的位置（例如：原位上浮），Update() 会把它 Lerp 过去。
            
            // 2. 更新箭头
            // 起点：卡牌顶部中心 (World Space)
            Vector3 startPos = transform.TransformPoint(new Vector3(0, rectTransform.rect.height * 0.5f, 0));
            
            // 终点：鼠标位置转 World Space
            Vector3 mousePos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform.parent as RectTransform, // 使用父级做参考
                eventData.position,
                eventData.pressEventCamera,
                out mousePos
            );
            mousePos.z = 0; // 修正Z轴
            
            TargetingArrowManager.Instance.Show(startPos, mousePos);
            
            // 3. (可选) 检测目标高亮
            // CheckTargetUnderMouse(eventData);
        }
        else
        {
            // --- 普通移动模式逻辑 ---
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out localPoint
            );
            rectTransform.localPosition = localPoint;
            
            // 拖拽手感
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, Quaternion.identity, deltaTime * 20f);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * 1.1f, deltaTime * 10f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsDragging = false;
        HandVisualManager.Instance.IsDraggingAnyCard = false;
        canvasGroup.blocksRaycasts = true;
        IsHovered = false; 

        // --- 清理瞄准状态 ---
        if (IsTargetingMode)
        {
            TargetingArrowManager.Instance.Hide();

            GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;

            // 1. 如果目标为空，则刷新布局
            if (targetObject == null){
                this.GetSystem<ICardSystem>().State = CardSystemState.正常;
                goto End;
            }

            List<object> param = GetParam(card.targetType, targetObject);

            // 2. 如果参数为空，则刷新布局
            if (param == null){
                this.GetSystem<ICardSystem>().State = CardSystemState.正常;
                goto End;
            }

            this.GetSystem<ICardSystem>().UseCard(card, param);
        }
        else{
            // 1. 使用卡牌
            this.GetSystem<ICardSystem>().UseCard(card, new List<object>());
        }

        End:
        HandVisualManager.Instance.RefreshLayout();
    }


    private void HighLight(CardTargetType targetType) => TargetSelector.Highlight(targetType);

    private List<object> GetParam(CardTargetType targetType, GameObject targetObject) => TargetSelector.GetParam(targetType, targetObject);

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

