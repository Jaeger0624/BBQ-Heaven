using UnityEngine;
using TMPro;
using UnityEngine.UI; // 引用 LayoutElement

// 建议添加此属性，确保Prefab上有LayoutElement组件
[RequireComponent(typeof(LayoutElement))]
public class TooltipElement : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private GameObject _headerObj; 
    [SerializeField] private GameObject _contentObj;
    
    // 引用自身的 LayoutElement
    [SerializeField] private LayoutElement _layoutElement;

    private void Awake()
    {
        // 如果没有拖拽赋值，自动获取
        if (_layoutElement == null) _layoutElement = GetComponent<LayoutElement>();
    }

    // 修改方法签名，增加 maxWidth 参数
    public void SetData(TooltipInfo info, float maxWidth)
    {
        // --- 1. 原有的设置内容逻辑 ---
        if (string.IsNullOrEmpty(info.title))
        {
            if (_headerObj) _headerObj.SetActive(false);
        }
        else
        {
            if (_headerObj) _headerObj.SetActive(true);
            if (_headerText) 
            {
                _headerText.text = info.title;
                _headerText.textWrappingMode = TextWrappingModes.Normal; // 确保开启换行
            }
        }

        if (string.IsNullOrEmpty(info.description))
        {
            if (_contentObj) _contentObj.SetActive(false);
        }
        else
        {
            if (_contentObj) _contentObj.SetActive(true);
            if (_contentText) 
            {
                _contentText.text = info.description;
                _contentText.textWrappingMode = TextWrappingModes.Normal; // 确保开启换行
            }
        }
        
        gameObject.SetActive(true);

        // --- 2. 新增：宽度限制逻辑 ---
        UpdateLayoutLimit(maxWidth);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void UpdateLayoutLimit(float maxWidth)
    {
        if (_layoutElement == null) return;

        // 强制更新 Mesh 以便获取准确的 preferredWidth
        if (_headerText != null) _headerText.ForceMeshUpdate();
        if (_contentText != null) _contentText.ForceMeshUpdate();

        // 获取当前文本不换行时的自然宽度
        float headerW = (_headerText != null && _headerObj.activeSelf) ? _headerText.preferredWidth : 0;
        float contentW = (_contentText != null && _contentObj.activeSelf) ? _contentText.preferredWidth : 0;

        // 取两者中较宽的一个
        float finalWidth = Mathf.Max(headerW, contentW);

        // 如果自然宽度 > 最大宽度，限制为最大宽度；否则使用自然宽度
        // 这样父物体的 ContentSizeFitter 就能得到正确的大小
        _layoutElement.preferredWidth = (finalWidth > maxWidth) ? maxWidth : finalWidth;

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}