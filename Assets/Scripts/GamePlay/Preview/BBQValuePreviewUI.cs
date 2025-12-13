using QFramework;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(CanvasGroup))]
public class BBQValuePreViewUI : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private CanvasGroup canvasGroup;
    public BBQPreview preview;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI tasteText;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }
    public void UpdateVisual(BBQPreview preview){
        this.preview = preview;
        rarityText.text = preview.totalRarity.ToString();
        tasteText.text = preview.totalTaste.ToString();
    }
    private void Hide(){
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
    private void Show(){
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
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
        UpdateVisual(evt.preview);
        Show();
    }
    private void OnHideBBQPreviewEvent(HideBBQPreviewEvent evt){
        Hide();
    }
}
