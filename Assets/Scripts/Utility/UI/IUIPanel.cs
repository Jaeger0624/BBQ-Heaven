using QFramework;
using UnityEngine;

public interface IUIPanel
{
    void Show();
    void Hide();
}

public class UIPanel : MonoBehaviour, IUIPanel, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    protected Canvas canvas;
    protected CanvasGroup canvasGroup;
    protected virtual void Awake()
    {
        canvas = this.GetComponent<Canvas>() == null ? this.gameObject.AddComponent<Canvas>() : this.GetComponent<Canvas>();
        canvasGroup = this.GetComponent<CanvasGroup>() == null ? this.gameObject.AddComponent<CanvasGroup>() : this.GetComponent<CanvasGroup>();
        // 确保Canvas启用
        canvas.enabled = true;
        Hide();
    }

    protected virtual void Start()
    {
        // 在Start中再次确保面板是隐藏的，防止编辑器中的默认状态影响
        Hide();
    }

    public void Hide()
    {
        if (canvas != null)
        {
            // 确保Canvas启用，但通过CanvasGroup控制可见性和交互
            canvas.enabled = true;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public void Show()
    {
        if (canvas != null)
        {
            // 确保Canvas启用
            canvas.enabled = true;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
}
