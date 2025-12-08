using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
public class TimeUseView : MonoBehaviour, IController{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerImage;
    [SerializeField] private Vector2 offset;
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isVisible = false;
    public int currentValue = 0;
    void Start()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        Hide();
    }
    void Update()
    {
        FollowMouse();
    }
    public void FollowMouse(){
        Vector3 mousePosition = Utility.GetMousePosition2D();
        transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, 0);
    }
    public void UpdateVisual(int time){
        timerText.text = time.ToString();
        currentValue = time;
    }
    public void Show(){
        canvasGroup.alpha = 1;
        isVisible = true;
    }
    public void Hide(){
        canvasGroup.alpha = 0;
        isVisible = false;
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}


public class ShowStickTimerEvent : IEvent{
    public int time;
    public ShowStickTimerEvent(int time){
        this.time = time;
    }
}

public class HideStickTimerEvent : IEvent{
    public HideStickTimerEvent(){}
}