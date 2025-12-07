using System.Collections.Generic;
using UnityEngine;

public class HandVisualManager : MonoBehaviour
{
    public static HandVisualManager Instance => _instance;
    private static HandVisualManager _instance;

    [Header("Container")]
    public Transform cardContainer;
    public List<CardHandView> cards = new List<CardHandView>();

    [Header("Layout Settings")]
    [Tooltip("圆弧半径，越大越平")]
    public float archRadius => SettingManager.Instance.DevSettings.archRadius; 
    [Tooltip("圆心Y轴偏移")]
    public float centerOffset => SettingManager.Instance.DevSettings.centerOffset;
    
    [Header("Dynamic Spacing (插值核心)")]
    [Tooltip("当达到多少张牌时，视为'最拥挤'状态 (达到最小间距/最大推力)")]
    public int maxHandSizeForLayout => SettingManager.Instance.DevSettings.maxHandSizeForLayout;

    [Tooltip("牌很少时的最大间距角度 (例如 2张牌时)")]
    public float maxSpacingAngle => SettingManager.Instance.DevSettings.maxSpacingAngle; 
    [Tooltip("牌很多时的最小间距角度 (例如 10张牌时)")]
    public float minSpacingAngle => SettingManager.Instance.DevSettings.minSpacingAngle; 
    [Tooltip("间距插值的速率")]
    public float rate => SettingManager.Instance.DevSettings.rate;

    
    [Header("Dynamic Hover Push (动态推力)")]
    [Tooltip("牌很少时的推力 (几乎不推)")]
    public float minPushAngle => SettingManager.Instance.DevSettings.minPushAngle; 
    [Tooltip("牌很多时的推力 (大力推开)")]
    public float maxPushAngle => SettingManager.Instance.DevSettings.pushMaxAngle; 
    
    [Tooltip("推开影响的范围（索引距离）")]
    public int pushRange => SettingManager.Instance.DevSettings.pushRange; 
    [Tooltip("推力衰减曲线 (1=线性, >1=指数衰减, 使得近处的推力大，远处的推力迅速减小)")]
    public float pushFalloffPower => SettingManager.Instance.DevSettings.pushFalloffPower;

    [Header("Visual State")]
    public float hoverScale => SettingManager.Instance.DevSettings.hoverScale;
    public float hoverHeightOffset => SettingManager.Instance.DevSettings.hoverHeightOffset;
    public float targetingHeightOffset => SettingManager.Instance.DevSettings.targetingHeightOffset;
    public bool IsDraggingAnyCard { get; set; } = false;

    private void Awake()
    {
        if (_instance != null) { Destroy(_instance.gameObject); }
        _instance = this;
    }

    private void Update()
    {
        CalculateCardPositions();
    }

    public CardHandView AddCard(Card card, GameObject cardPrefab)
    {
        GameObject go = Instantiate(cardPrefab, cardContainer);
        CardHandView view = go.GetComponent<CardHandView>();
        view.Init(card);
        cards.Add(view);
        view.rectTransform.localPosition = new Vector3(-800, -500, 0); 
        return view;
    }

    public CardHandView RemoveCard(CardHandView card)
    {
        cards.Remove(card);
        Destroy(card.gameObject);
        return card;
    }

    public void RefreshLayout()
    {
        // 简单重置顺序
        for (int i = 0; i < cards.Count; i++) cards[i].transform.SetSiblingIndex(i);
    }

    public void OnCardHoverEnter(CardHandView hoveredCard)
    {
        if (IsDraggingAnyCard) return;
        hoveredCard.transform.SetAsLastSibling();
    }

    public void OnCardHoverExit(CardHandView hoveredCard)
    {
        if (IsDraggingAnyCard) return;
        int index = cards.IndexOf(hoveredCard);
        if (index >= 0 && index < cardContainer.childCount) 
        {
            hoveredCard.transform.SetSiblingIndex(index);
        }
    }

    // --- 核心算法 (完全重写) ---
    private void CalculateCardPositions()
    {
        int count = cards.Count;
        if (count == 0) return;

        // --- 1. 计算"拥挤度" (Crowding Factor) ---
        // 归一化 t：
        // 2张牌时 t=0 (最松散)
        // 10张牌时 t=1 (最拥挤)
        // 限制在 0~1 之间
        float t = 0f;
        if (count > 1)
        {
            // 分母减1是为了让count=2时结果为0
            t = (float)(count - 1 - 1) / (maxHandSizeForLayout - 1 - 1); 
            // 修正：更简单的写法是直接按比例，防止除0
            if (maxHandSizeForLayout <= 2) t = 1f; // 防御性代码
            t = Mathf.Clamp01(t); 
        }

        // --- 2. 根据拥挤度插值计算参数 ---
        
        // 间距：牌越多，间距越小 (Lerp 从 Max 到 Min)
        // 非线性插值：使用幂函数，使得在牌少时，间距增长较慢，在牌多时，间距增长较快
        float currentAnglePerCard = Mathf.Lerp(maxSpacingAngle, minSpacingAngle, Mathf.Pow(t, rate));
        
        // 推力：牌越多，推力越大 (Lerp 从 Min 到 Max)
        float currentPushForce = Mathf.Lerp(minPushAngle, maxPushAngle, Mathf.Pow(t, rate));

        // 计算总扇形角度
        float totalAngle = (count - 1) * currentAnglePerCard;
        float startAngle = totalAngle / 2f;

        // --- 3. 寻找 Hover 目标 ---
        int hoveredIndex = -1;
        if (!IsDraggingAnyCard)
        {
            for (int i = 0; i < count; i++)
            {
                if (cards[i].IsHovered)
                {
                    hoveredIndex = i;
                    break;
                }
            }
        }

        // --- 4. 遍历计算每张牌 ---
        for (int i = 0; i < count; i++)
        {
            CardHandView card = cards[i];

            // 拖拽中且非瞄准模式，跳过计算
            if (card.IsDragging && !card.IsTargetingMode) continue;

            // A. 基础角度
            float indexT = count > 1 ? (float)i / (count - 1) : 0.5f;
            float angle = Mathf.Lerp(startAngle, -startAngle, indexT);

            // B. 应用动态推力
            if (hoveredIndex != -1)
            {
                float dist = Mathf.Abs(i - hoveredIndex);
                if (dist <= pushRange && dist > 0)
                {
                    // 距离归一化 (0=最近/相邻, 1=最远)
                    // pushRange=2时: dist=1 -> norm=0.5, dist=2 -> norm=1.0 (反直觉，需修正)
                    
                    // 正确的衰减计算：
                    // 1 / dist 是最简单的非线性衰减
                    // 或者 (range - dist + 1) / range
                    
                    float pushFactor = (pushRange - dist + 1) / (float)pushRange; // 线性：1.0 -> 0.5
                    pushFactor = Mathf.Clamp01(pushFactor);
                    
                    // 应用指数曲线让近处受力更大
                    pushFactor = Mathf.Pow(pushFactor, pushFalloffPower);

                    float finalPush = pushFactor * currentPushForce;

                    if (i < hoveredIndex) angle += finalPush;
                    else angle -= finalPush;
                }
            }

            // C. 极坐标转笛卡尔坐标
            float rad = angle * Mathf.Deg2Rad;
            float x = -Mathf.Sin(rad) * archRadius; 
            float y = Mathf.Cos(rad) * archRadius;

            Vector3 finalPos = new Vector3(x, y + centerOffset, 0);
            Quaternion finalRot = Quaternion.Euler(0, 0, angle);
            Vector3 finalScale = Vector3.one;

            // D. 状态修正
            bool isHovering = (i == hoveredIndex && !IsDraggingAnyCard);
            bool isTargeting = card.IsDragging && card.IsTargetingMode;

            if (isHovering || isTargeting)
            {
                finalScale = Vector3.one * hoverScale;
                finalRot = Quaternion.identity; 
                
                float heightBonus = isTargeting ? targetingHeightOffset : hoverHeightOffset;
                finalPos += Vector3.up * heightBonus;
            }

            card.targetPos = finalPos;
            card.targetRot = finalRot;
            card.targetScale = finalScale;
        }
    }
}