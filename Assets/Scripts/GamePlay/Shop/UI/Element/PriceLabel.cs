using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PriceLabel : MonoBehaviour{
    // 是否有打折
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI originalPriceText;
    [SerializeField] private TextMeshProUGUI discountPriceText;
    [SerializeField] private Image discountImage;
    public void SetPrice(PriceContext priceContext){

        if (priceContext.IsDiscount){
            priceText.text = $"<s>{priceContext.OriginalPrice}</s> {priceContext.Price}<color=yellow>Q</color>";
        }
        else{
            // originalPriceText.gameObject.SetActive(false);
            priceText.text = $"{priceContext.Price}<color=yellow>Q</color>";
        }
    }
    public void Hide(){
        gameObject.SetActive(false);
    }
    public void Show(){
        gameObject.SetActive(true);
    }
}