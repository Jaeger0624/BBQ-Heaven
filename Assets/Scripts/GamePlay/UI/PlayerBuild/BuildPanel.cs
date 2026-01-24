using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示玩家构筑的面板
/// </summary>
public class BuildPanel : MonoBehaviour, IController{
    [SerializeField] private FoodDisplayContainer 食材容器;
    [SerializeField] private MascotDisplayContainer 吉祥物容器;

    [SerializeField] private CardDisplayContainer 卡牌容器;
    // [SerializeField] private RecipeDisplayContainer 配方容器;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void Start()
    {
    }

    public void Show(){
        // 清除之前的
        ClearAllDisplayViews();
        InitFoodDisplayViews();
        InitMascotDisplayViews();
        InitCardDisplayViews();
        InitRecipeDisplayViews();
    }

    private void InitFoodDisplayViews()
    {
        食材容器.GetGameObject().SetActive(true);
        List<FoodCard> foodCards = this.GetSystem<IFoodSystem>().FoodRepositorys().Values.ToList();
        List<FoodUIContext> foodInstances = foodCards.Select(food => new FoodUIContext(food)).ToList();
        食材容器.RefreshUI(foodInstances, new CollectionFoodStrategy());
    }

    private void InitMascotDisplayViews()
    {   
        吉祥物容器.GetGameObject().SetActive(true);
        List<MascotUIContext> mascotUIContexts = this.GetSystem<IMascotSystem>().Mascots.Values.Select(mascot => new MascotUIContext(mascot)).ToList();
        吉祥物容器.RefreshUI(mascotUIContexts, new CollectionMascotStrategy());
    }

    private void InitCardDisplayViews()
    {
        卡牌容器.GetGameObject().SetActive(true);
        List<CardUIContext> cardUIContexts = this.GetSystem<ICardSystem>().CardRepository.Values.Select(card => new CardUIContext(card)).ToList();
        卡牌容器.RefreshUI(cardUIContexts, new CollectionCardStrategy());
    }

    private void InitRecipeDisplayViews()
    {
        // recipeDisplayViews = new List<RecipeDisplayView>();
        // foreach (var recipe in this.GetSystem<IRecipeSystem>().Recipes())
        // {
        //     RecipeDisplayView recipeDisplayView = Instantiate(配方预制体, 配方容器).GetComponent<RecipeDisplayView>();
        //     recipeDisplayView.Bind(recipe);
        //     recipeDisplayViews.Add(recipeDisplayView);
        // }
    }

    private void ClearAllDisplayViews()
    {
        // 食材容器.GetGameObject().SetActive(false);
        // 吉祥物容器.GetGameObject().SetActive(false);
        // 配方容器.GetGameObject().SetActive(false);
    }
}