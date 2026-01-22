using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BBQView : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] public Transform foodParent;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Image stickImage;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI tasteText;
    // 烤串上的插槽位置列表
    private List<RectTransform> slotTransforms = new List<RectTransform>();
    private BBQ bbq;
    public bool IsBuilding { get; set; } = true;
    private Vector3 offset = Vector3.zero;
    private RectTransform rectTransform => transform as RectTransform;
    public void Bind(BBQ bbq){
        // 1. 绑定烧烤实例
        this.bbq = bbq;

        // 2. 创建插槽位置列表
        CreateSlotTransforms(bbq.stick.maxFoodCount);

        // 3. 更新视觉
        if (!IsBuilding){   
            UpdateVisual();
        }
        else{
            rarityText.gameObject.SetActive(false);
            tasteText.gameObject.SetActive(false);

            Sprite sprite = SettingManager.Instance.ArtSettings.StickSprites.GetSprite(bbq.stick.stickData.Sprite);
            if (sprite == null){
                Debug.LogError($"BBQView: 获取烤串图标失败: {bbq.stick.stickData.Sprite}");
            }
            else{
                stickImage.sprite = sprite;
            }
        }
    }
    public BBQ GetBBQ(){
        return bbq;
    }
    public void UpdateVisual(){
        if (bbq == null) return;
        rarityText.gameObject.SetActive(true);
        tasteText.gameObject.SetActive(true);
        rarityText.text = bbq.totalRarity.Value.ToString();
        tasteText.text = bbq.totalTaste.Value.ToString();

        Sprite sprite = SettingManager.Instance.ArtSettings.StickSprites.GetSprite(bbq.stick.stickData.Sprite);
        if (sprite == null){
            Debug.LogError($"BBQView: 获取烤串图标失败: {bbq.stick.stickData.Sprite}");
        }
        else{
            stickImage.sprite = sprite;
        }

        foreach (var foodInstance in bbq.foodInstances){
            IEntityView view = foodInstance.foodInstanceView;
            view.GO().transform.rotation = Quaternion.identity;
        }
    }
    private void CreateSlotTransforms(int slotCount){
        Reset();
        for (int i = 0; i < slotCount; i++){
            RectTransform slotObject = Instantiate(slotPrefab, foodParent).GetComponent<RectTransform>();
            slotObject.gameObject.SetActive(true);
            slotObject.anchoredPosition = Vector2.zero;
            slotTransforms.Add(slotObject);
        }
    }

    private void Reset()
    {
        List<RectTransform> tempSlotTransforms = slotTransforms.ToList();
        foreach (var slotTransform in tempSlotTransforms)
        {
            Destroy(slotTransform.gameObject);
        }
        slotTransforms.Clear();
    }

    public Transform GetSlotTransform(int slotIndex){
        return slotTransforms[slotIndex].transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        offset = worldPosition - (Vector2)rectTransform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // 将烧烤视图移动到鼠标位置
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPosition.z = 0;
        // 相机屏幕位置，不是相对位置
        rectTransform.position = worldPosition - offset;


        CustomerView customerView = eventData.RaycastGetUI<CustomerView>();

    }
    // 结束拖拽时，判断是否拖拽到了顾客身上，如果是的话，调用DealSystem的ExecuteDeal方法
    public void OnEndDrag(PointerEventData eventData)
    {
        CustomerView customerView = eventData.RaycastGetUI<CustomerView>();
        if (customerView != null){
            this.GetSystem<IDealSystem>().ExecuteDeal(bbq, customerView.currentCustomer);
            Debug.Log("拖拽到顾客身上，执行交易");
        }
        else{
            transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }
}
