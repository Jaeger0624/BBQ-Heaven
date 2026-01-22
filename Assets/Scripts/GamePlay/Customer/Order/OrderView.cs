using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OrderView : MonoBehaviour, ICanSendEvent
{
    public Customer customer{get; private set;}
    [SerializeField] private Image BackgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI patienceText;
    public TextMeshProUGUI customerTagsText;
    [SerializeField] private Transform requirementContainer;
    [SerializeField] private RequirementView requirementViewPrefab;
    private List<RequirementView> requirementViews = new List<RequirementView>();
    private bool isSelected = false;
    public void Bind(Customer customer){
        this.customer = customer;

        // 注册
        customer.PatienceNow.Subscribe(value => {
            UpdatePatience();
        }).AddTo(this);

        UpdateVisual();

        GenerateRequirementViews();
    }
    public void UpdateVisual(){
        // 1. 设置名字
        nameText.text = customer.name;

        UpdatePatience();
        UpdateCustomerTags();
    }

    private void GenerateRequirementViews(){
        requirementViews.Clear();
        foreach (var requirement in customer.requirements.OrderBy(x => x.StarAmount)){
            RequirementView requirementView = Instantiate(requirementViewPrefab, requirementContainer);
            requirementView.Bind(requirement);
            requirementViews.Add(requirementView);
        }

        // 刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(requirementContainer.GetComponent<RectTransform>());
    }


    private void UpdateCustomerTags(){
        string tags = string.Join(", ", customer.customerTags.Select(x => x.name));
        customerTagsText.text = $"<color=yellow>Tag：</color>{tags}";
    }
    private void UpdatePatience(){
        patienceText.text = $"{customer.PatienceNow.Value}/{customer.PatienceMax.Value}";
        patienceText.color = customer.PatienceNow.Value >= customer.PatienceMax.Value ? Color.red : Color.white;
    }

    public void ChooseCurrent(){
        this.SendEvent(new OrderClickedEvent(customer));
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    public void ChooseCurrentButtonClick(CustomerTagType tagType){
        this.SendEvent(new OrderClickedEvent(customer));
    }
    public void SetViewSelected(bool isSelected){
        this.isSelected = isSelected;
        BackgroundImage.color = isSelected ? Color.green : Color.white;
    }
}
