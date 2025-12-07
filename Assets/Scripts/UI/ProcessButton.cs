using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class ProcessButton : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private Button button;
    void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }
    void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }
    private void OnClick()
    {
        this.SendEvent(new ProcessMoveNextEvent());
    }
}
