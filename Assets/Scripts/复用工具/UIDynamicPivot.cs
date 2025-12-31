
using UnityEngine;

public enum LengthType{
    Width,
    Height,
}
public enum DirectionType{
    Left,
    Right,
    Top,
    Bottom,
}
public class UIDynamicPivot : MonoBehaviour
{

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private LengthType lengthType = LengthType.Width;
    [SerializeField] private DirectionType direction = DirectionType.Left;
    [SerializeField] private RectTransform targetTransform;
    void Update()
    {
        float length = lengthType == LengthType.Width ? rectTransform.rect.width : rectTransform.rect.height;
        switch (direction)
        {
            case DirectionType.Left:
                targetTransform.anchoredPosition = parentTransform.anchoredPosition + new Vector2(-length, 0);
                break;
            case DirectionType.Right:
                targetTransform.anchoredPosition = parentTransform.anchoredPosition + new Vector2(length, 0);
                break;
            case DirectionType.Top:
                targetTransform.anchoredPosition = parentTransform.anchoredPosition + new Vector2(0, length);
                break;
            case DirectionType.Bottom:
                targetTransform.anchoredPosition = parentTransform.anchoredPosition + new Vector2(0, -length);
                break;
        }
    }
}
