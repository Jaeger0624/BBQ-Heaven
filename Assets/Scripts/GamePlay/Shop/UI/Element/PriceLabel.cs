using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PriceLabel : MonoBehaviour{
    // 是否有打折
    private bool isDiscount = false;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI originalPriceText;
    [SerializeField] private TextMeshProUGUI discountPriceText;
    [SerializeField] private Image discountImage;
    public void SetPrice(int price){
        priceText.text = price.ToString();
        if (isDiscount){
            discountImage.gameObject.SetActive(true);
        }
    }
}