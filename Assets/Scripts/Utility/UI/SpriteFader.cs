using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if TMP_PRESENT
using TMPro;
#endif

[DisallowMultipleComponent]
public class SpriteGroupFader : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("组 Alpha（会与子元素各自原始 Alpha 相乘，类似 CanvasGroup）")]
    public float groupAlpha = 1f;

    [Tooltip("是否搜索未激活的子物体")]
    public bool includeInactive = true;

    [Header("交互（可选）")]
    [Tooltip("当完全隐形(Alpha<=阈值)时禁用子物体的 Collider/Collider2D")]
    public bool toggleCollidersWhenInvisible = true;
    [Range(0f, 1f)] public float invisibleThreshold = 0.01f;

    [Tooltip("Alpha 为 0 时可选地禁用所有 Renderer 以省开销")]
    public bool disableRenderersWhenInvisible = false;

    [Header("缓存查看（只读）")]
    [SerializeField] private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
#if TMP_PRESENT
    [SerializeField] private List<TMP_Text> tmpTexts = new List<TMP_Text>();
#endif
    private struct ColorSlot { public Color baseColor; public Object target; }
    private List<ColorSlot> slots = new List<ColorSlot>();
    private Coroutine fadeCo;

    void Awake()
    {
        RebuildCache();
        ApplyGroupAlphaInstant();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 在编辑器拖动滑条时实时更新
        if (!Application.isPlaying) RebuildCache();
        ApplyGroupAlphaInstant();
    }
#endif

    [ContextMenu("Rebuild Cache")]
    public void RebuildCache()
    {
        spriteRenderers.Clear();
#if TMP_PRESENT
        tmpTexts.Clear();
#endif
        slots.Clear();

        GetComponentsInChildren(includeInactive, spriteRenderers);
#if TMP_PRESENT
        GetComponentsInChildren(includeInactive, tmpTexts);
#endif

        foreach (var sr in spriteRenderers)
        {
            slots.Add(new ColorSlot { baseColor = sr.color, target = sr });
        }
#if TMP_PRESENT
        foreach (var t in tmpTexts)
        {
            slots.Add(new ColorSlot { baseColor = t.color, target = t });
        }
#endif
    }

    public void SetGroupAlpha(float a)
    {
        groupAlpha = Mathf.Clamp01(a);
        ApplyGroupAlphaInstant();
    }

    public void FadeTo(float targetAlpha, float duration, bool unscaledTime = false)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(targetAlpha, duration, unscaledTime));
    }

    IEnumerator FadeRoutine(float target, float duration, bool unscaled)
    {
        float start = groupAlpha;
        float t = 0f;

        // 确保开始时启用 Renderer，避免从 0 起不显示
        if (disableRenderersWhenInvisible && target > invisibleThreshold)
            SetAllRenderersEnabled(true);

        while (t < duration)
        {
            t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            groupAlpha = Mathf.Lerp(start, target, k);
            ApplyGroupAlphaInstant();
            yield return null;
        }
        groupAlpha = Mathf.Clamp01(target);
        ApplyGroupAlphaInstant();

        HandleVisibilitySideEffects();
        fadeCo = null;
    }

    private void ApplyGroupAlphaInstant()
    {
        // 将每个元素的原始 alpha 乘以组 alpha
        foreach (var slot in slots)
        {
            float a = slot.baseColor.a * groupAlpha;

            if (slot.target is SpriteRenderer sr)
            {
                var c = sr.color; c.a = a; sr.color = c;
            }
#if TMP_PRESENT
            else if (slot.target is TMP_Text t)
            {
                var c = t.color; c.a = a; t.color = c;
            }
#endif
        }
        HandleVisibilitySideEffects();
    }

    private void HandleVisibilitySideEffects()
    {
        bool invisible = groupAlpha <= invisibleThreshold;

        if (toggleCollidersWhenInvisible)
        {
            var cols3D = GetComponentsInChildren<Collider>(includeInactive);
            foreach (var c in cols3D) c.enabled = !invisible;

            var cols2D = GetComponentsInChildren<Collider2D>(includeInactive);
            foreach (var c in cols2D) c.enabled = !invisible;
        }

        if (disableRenderersWhenInvisible)
        {
            SetAllRenderersEnabled(!invisible);
        }
    }

    private void SetAllRenderersEnabled(bool enabled)
    {
        foreach (var sr in spriteRenderers)
            sr.enabled = enabled;
#if TMP_PRESENT
        foreach (var t in tmpTexts)
        {
            var r = t.GetComponent<Renderer>();
            if (r) r.enabled = enabled;
        }
#endif
    }
}
