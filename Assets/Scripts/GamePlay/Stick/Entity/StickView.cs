using System.IO;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StickView : MonoBehaviour, ICanGetSystem, ICanRegisterEvent, IPointerClickHandler{
    public Stick stick;
    public Transform slot{get; private set;}
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI timeCostText;
    private bool isSelected = false;

    public void Init(Stick stick, Transform slot){
        this.stick = stick;
        this.slot = slot;
        UpdateVisual();
    }

    void Start()
    {
        this.RegisterEvent<UnselectStickEvent>(OnUnselectStickEvent);
    }

    void OnDestroy()
    {
        this.UnRegisterEvent<UnselectStickEvent>(OnUnselectStickEvent);
    }
    void OnUnselectStickEvent(UnselectStickEvent e){
        isSelected = false;
    }

    void Update()
    {
        if (isSelected){
            image.color = Color.grey;
        }else{
            image.color = Color.white;
        }
        int baseTimeCost = SettingManager.Instance.GameplaySettings.makeBBQTime_默认;
        timeCostText.text = (stick.extraTimeCost + baseTimeCost).ToString();
    }

    private void UpdateVisual(){
        
        Sprite sprite = SettingManager.Instance.ArtSettings.StickSprites.GetSprite(stick.stickData.Sprite);
        if (sprite == null){
            Debug.LogError($"StickView: 获取烤串图标失败: {stick.stickData.Sprite}");
        }
        else{
            image.sprite = sprite;
        }
    }

    public void OnSelect(){

        this.GetSystem<IStickSystem>().SetSelectedStick(stick);
        isSelected = true;
    }

    public void OnUnselect(){
        this.GetSystem<IStickSystem>().UnselectStick();
        isSelected = false;
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelected){
            OnUnselect();
        }
        else{
            OnSelect();
        }
    }
}