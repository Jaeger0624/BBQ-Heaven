using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TooltipElement : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private GameObject _headerObj; // 用于没有标题时隐藏
    [SerializeField] private GameObject _contentObj; // 用于没有内容时隐藏

    // 初始化数据
    public void SetData(TooltipInfo info)
    {
        // 设置标题
        if (string.IsNullOrEmpty(info.title))
        {
            if (_headerObj) _headerObj.SetActive(false);
        }
        else
        {
            if (_headerObj) _headerObj.SetActive(true);
            if (_headerText) _headerText.text = info.title;
                }

        // 设置内容
        if (string.IsNullOrEmpty(info.description))
        {
            if (_contentObj) _contentObj.SetActive(false);
        }
        else
        {
            if (_contentObj) _contentObj.SetActive(true);
            if (_contentText) _contentText.text = info.description;
        }

        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}