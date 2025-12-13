using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
[RequireComponent(typeof(CanvasGroup))]
public class TimeUseView : MonoBehaviour, IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerImage;
    public int currentValue = 0;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }
    void OnEnable()
    {
        this.RegisterEvent<ShowBBQPreviewEvent>(OnShowBBQPreviewEvent);
        this.RegisterEvent<HideBBQPreviewEvent>(OnHideBBQPreviewEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<ShowBBQPreviewEvent>(OnShowBBQPreviewEvent);
        this.UnRegisterEvent<HideBBQPreviewEvent>(OnHideBBQPreviewEvent);
    }
    private void OnShowBBQPreviewEvent(ShowBBQPreviewEvent evt){
        UpdateVisual(evt.preview.totalTimeCost);
        Show();
    }
    private void OnHideBBQPreviewEvent(HideBBQPreviewEvent evt){
        Hide();
    }
    public void UpdateVisual(int time){
        timerText.text = time.ToString();
        currentValue = time;
    }
    public void Show(){
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }
    public void Hide(){
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

}


public class ShowBBQPreviewEvent : AbstractEvent{
    public BBQPreview preview;
    public ShowBBQPreviewEvent(BBQPreview preview){
        this.preview = preview;
    }
}

public class HideBBQPreviewEvent : AbstractEvent{
    public HideBBQPreviewEvent(){}
}