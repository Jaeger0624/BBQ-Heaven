using cfg;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodRepositoryView : MonoBehaviour, IController{
    [SerializeField] private Image foodImage;
    [SerializeField] private TextMeshProUGUI foodAmount;
    public string foodId;

    public void Init(string foodId, int amount){
        FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(foodId);
        Sprite sprite = Resources.Load<Sprite>("Sprites/" + foodData.Sprite);
        if (sprite == null){
            Debug.LogError($"食材资源不存在: {foodData.Sprite}");
            return;
        }
        if (foodImage == null){
            Debug.LogError($"FoodRepositoryView {foodId} 的 foodImage 组件未赋值");
            return;
        }
        if (foodAmount == null){
            Debug.LogError($"FoodRepositoryView {foodId} 的 foodAmount 组件未赋值");
            return;
        }
        foodImage.sprite = sprite;
        this.foodId = foodId;
        this.foodAmount.text = amount.ToString();
    }

    public void UpdateVisual(int amount){
        if (foodAmount == null){
            Debug.LogError($"FoodRepositoryView {foodId} 的 foodAmount 组件未赋值");
            return;
        }
        this.foodAmount.text = amount.ToString();
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}