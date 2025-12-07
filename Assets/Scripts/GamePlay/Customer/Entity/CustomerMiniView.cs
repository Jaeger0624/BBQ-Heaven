using System;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomerMiniView : MonoBehaviour, IController, IPointerClickHandler{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public Customer customer;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI leftPatienceText;
    public Action OnClick;
    public void Init(Customer customer){
        this.customer = customer;
        UpdateVisual();

        customer.PatienceNow.Subscribe(_ => {
            UpdateLeftPatienceText();
        }).AddTo(this);
    }
    private void OnEnable(){
        this.RegisterEvent<CustomerAboutToLeaveEvent>(OnCustomerAboutToLeave);
    }
    private void OnDisable(){
        this.UnRegisterEvent<CustomerAboutToLeaveEvent>(OnCustomerAboutToLeave);
    }
    private void OnCustomerAboutToLeave(CustomerAboutToLeaveEvent e){
        if (customer.guid == e.guid){
            TurnRed();
        }
    }
    public void UpdateVisual(){
        // 更新视觉
        nameText.text = customer.name;
        Sprite sprite = Resources.Load<Sprite>("Sprites/CustomerMini" + customer.name);
        if (sprite == null){
            // Debug.LogWarning("顾客迷你视图资源不存在: " + "Sprites/CustomerMini" + customer.name);
            return;
        }
        image.sprite = sprite;
        UpdateLeftPatienceText();
    }

    private void UpdateLeftPatienceText(){
        leftPatienceText.text = (customer.PatienceMax.Value - customer.PatienceNow.Value).ToString();
    }
    private void TurnRed(){
        leftPatienceText.text = "";
        image.color = new Color(1f, 0.6f, 0.6f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        // Debug.Log("点击顾客: " + customer.GetDescription());
        OnClick?.Invoke();
        
    }
}