using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class OrderView : MonoBehaviour
{
    private Customer customer;
    [SerializeField] private Image BackgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI patienceText;
    public TextMeshProUGUI customerTagsText;

    public void Bind(Customer customer){
        this.customer = customer;

        // 注册
        customer.PatienceNow.Subscribe(value => {
            UpdatePatience();
        }).AddTo(this);

        UpdateVisual();
    }
    public void UpdateVisual(){
        // 1. 设置名字
        nameText.text = customer.name;

        UpdatePatience();
        UpdateCustomerTags();
    }


    private void UpdateCustomerTags(){
        string tags = string.Join(", ", customer.customerTags.Select(x => x.name));
        customerTagsText.text = $"<color=yellow>Tag：</color>{tags}";
    }
    private void UpdatePatience(){
        patienceText.text = $"{customer.PatienceNow.Value}/{customer.PatienceMax.Value}";
        patienceText.color = customer.PatienceNow.Value >= customer.PatienceMax.Value ? Color.red : Color.white;
    }
}
