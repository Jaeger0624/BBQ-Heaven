using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示玩家构筑的面板
/// </summary>
public class BuildPanel : MonoBehaviour, IController{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button showButton;
    [SerializeField] private Button hideButton;
    [SerializeField] private Transform 食材容器;
    [SerializeField] private Transform 吉祥物容器;
    [SerializeField] private Transform 配方容器;
    [SerializeField] private GameObject 食材预制体;
    [SerializeField] private GameObject 吉祥物预制体;
    [SerializeField] private GameObject 配方预制体;
    private List<FoodDisplayView> foodDisplayViews = new List<FoodDisplayView>();
    private List<MascotDisplayView> mascotDisplayViews = new List<MascotDisplayView>();
    private List<RecipeDisplayView> recipeDisplayViews = new List<RecipeDisplayView>();

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void Start()
    {
        showButton.onClick.AddListener(Show);
        hideButton.onClick.AddListener(Hide);

        Hide();
    }
    void OnDestroy()
    {
        showButton.onClick.RemoveListener(Show);
        hideButton.onClick.RemoveListener(Hide);
    }
    public void Show()
    {
        InitFoodDisplayViews();
        InitMascotDisplayViews();
        InitRecipeDisplayViews();

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        ClearAllDisplayViews();
    }

    private void InitFoodDisplayViews()
    {
        foodDisplayViews = new List<FoodDisplayView>();
        Dictionary<string, int> foodRepositoryDict = this.GetSystem<IFoodSystem>().GetFoodRepositoryDict();
        foreach (var food in foodRepositoryDict)
        {
            FoodDisplayView foodDisplayView = Instantiate(食材预制体, 食材容器).GetComponent<FoodDisplayView>();
            FoodData foodData = this.GetSystem<IDataSystem>().GetFoodData(food.Key);
            foodDisplayView.Bind(foodData, food.Value);
            foodDisplayViews.Add(foodDisplayView);
        }
    }

    private void InitMascotDisplayViews()
    {
        mascotDisplayViews = new List<MascotDisplayView>();
        foreach (var mascot in this.GetSystem<IMascotSystem>().Mascots)
        {
            MascotDisplayView mascotDisplayView = Instantiate(吉祥物预制体, 吉祥物容器).GetComponent<MascotDisplayView>();
            mascotDisplayView.Bind(mascot.Value);
            mascotDisplayViews.Add(mascotDisplayView);
        }
    }

    private void InitRecipeDisplayViews()
    {
        recipeDisplayViews = new List<RecipeDisplayView>();
        foreach (var recipe in this.GetSystem<IRecipeSystem>().Recipes())
        {
            RecipeDisplayView recipeDisplayView = Instantiate(配方预制体, 配方容器).GetComponent<RecipeDisplayView>();
            recipeDisplayView.Bind(recipe);
            recipeDisplayViews.Add(recipeDisplayView);
        }
    }

    private void ClearAllDisplayViews()
    {
        foreach (var foodDisplayView in foodDisplayViews)
        {
            Destroy(foodDisplayView.gameObject);
        }
        foodDisplayViews.Clear();
        foreach (var mascotDisplayView in mascotDisplayViews)
        {
            Destroy(mascotDisplayView.gameObject);
        }
        mascotDisplayViews.Clear();
        foreach (var recipeDisplayView in recipeDisplayViews)
        {
            Destroy(recipeDisplayView.gameObject);
        }
        recipeDisplayViews.Clear();
    }
}