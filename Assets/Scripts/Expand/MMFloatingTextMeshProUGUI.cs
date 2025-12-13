using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class MMFloatingTextMeshProUGUI : MMFloatingText
{
    
    // 引用 UGUI 版本的 TMP
    public TextMeshProUGUI TargetTextMeshProUGUI;

    public override void SetText(string newValue)
    {
        if (TargetTextMeshProUGUI != null)
        {
            TargetTextMeshProUGUI.text = newValue;
        }
    }

    public override void SetColor(Color newColor)
    {
        if (TargetTextMeshProUGUI != null)
        {
            TargetTextMeshProUGUI.color = newColor;
        }
    }

    public override void SetOpacity(float newOpacity)
    {
        if (TargetTextMeshProUGUI != null)
        {
            TargetTextMeshProUGUI.color = new Color(TargetTextMeshProUGUI.color.r, TargetTextMeshProUGUI.color.g, TargetTextMeshProUGUI.color.b, newOpacity);
        }
    }
}
