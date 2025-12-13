using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ContentSizeFitter))]
public class ForceChangeSize : MonoBehaviour
{
    private ContentSizeFitter contentSizeFitter;
    void Awake()
    {
        contentSizeFitter = GetComponent<ContentSizeFitter>();
    }

    void LateUpdate()
    {
        contentSizeFitter.SetLayoutHorizontal();
        contentSizeFitter.SetLayoutVertical();
    }
}
