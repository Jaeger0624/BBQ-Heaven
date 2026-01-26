using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerView : MonoBehaviour{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    private List<Sprite> sprites => SettingManager.Instance.ArtSettings.CustomerSprites.sprites;
    public Customer currentCustomer { get; private set; } = null;
    [SerializeField] private TextMeshProUGUI descriptionText;

    void Start()
    {
        nameText.text = "";
    }
    public void Bind(Customer customer){
        this.currentCustomer = customer;
        UpdateVisual();
    }
    public void UpdateVisual(){
        if (currentCustomer == null){
            nameText.text = "";
            image.gameObject.SetActive(false);
            return;
        }

        image.sprite = currentCustomer.customerLook;
        image.gameObject.SetActive(true);
        nameText.text = currentCustomer.name;

        UpdateDescription();
    }

    private void UpdateDescription(){
        string description = "";
        foreach (var tooltipInfo in GetTooltipInfo()){
            description += tooltipInfo.description + "\n";
        }
        descriptionText.text = description;
    }

    public List<TooltipInfo> GetTooltipInfo()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>
        {
            new TooltipInfo($"<size=36>{currentCustomer.name}</size>")
        };
        

        currentCustomer.customerTags.ForEach(tag => {
            tooltipInfos.Add(new TooltipInfo($"<size=24>{tag.name}</size>"));
        });
        return tooltipInfos;
    }
}