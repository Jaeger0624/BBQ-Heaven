using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

public class RecipeController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private GameObject recipePreviewViewPrefab;
    [SerializeField] private Transform recipePreviewContainer;
    private List<RecipePreviewView> views = new List<RecipePreviewView>();
    void OnEnable()
    {
        this.RegisterEvent<MatchRecipeEvent>(OnMatchRecipe);
        this.RegisterEvent<MatchRecipePreviewEvent>(OnMatchRecipePreview);
        this.RegisterEvent<ResetRecipePreviewViewsEvent>(OnResetViews);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<MatchRecipeEvent>(OnMatchRecipe);
        this.UnRegisterEvent<MatchRecipePreviewEvent>(OnMatchRecipePreview);
        this.UnRegisterEvent<ResetRecipePreviewViewsEvent>(OnResetViews);
    }

    private void OnMatchRecipe(MatchRecipeEvent evt)
    {
        List<IAnimTask> animTasks = new List<IAnimTask>();
        // 1. 展示配方消息
        foreach (var recipeResult in evt.matchedRecipePreviews)
        {
            float time = SettingManager.Instance.AnimSettings.textSpawnLifetime_配方;
            Vector3 position = AnimUtility.GetTextSpawnPosition(Vector3.zero);
            animTasks.Add(new SpawnTextAnimationTask
            (BuildRecipeText(recipeResult.recipe), 8f, Color.white, position, time));
            animTasks.Add(new DelayAnimTask(time, true));
        }

        animTasks.Add(new DelayAnimTask(0.2f, true));



        IAnimTask animTask = new SequenceAnimTask(animTasks);
        this.GetSystem<IAnimationSystem>().Append(animTask);
        this.GetSystem<IAnimationSystem>().Play();
    }
    private void OnMatchRecipePreview(MatchRecipePreviewEvent evt)
    {
        ResetRecipePreviewViews();
        // 1. 展示配方消息
        foreach (var preview in evt.recipePreviews)
        {
            RecipePreviewView recipePreviewView = Instantiate(recipePreviewViewPrefab, recipePreviewContainer).GetComponent<RecipePreviewView>();
            recipePreviewView.Bind(preview);
            views.Add(recipePreviewView);
        }
    }
    private void OnResetViews(ResetRecipePreviewViewsEvent evt) => ResetRecipePreviewViews();

    private void ResetRecipePreviewViews(){
        foreach (var view in views)
        {
            view.Remove();
        }
        views.Clear();
    }
    private string BuildRecipeText(Recipe recipe){
        StringBuilder sb = new StringBuilder();
        sb.Append("<size=6> 激活配方:</size>");
        sb.Append("\n<color=yellow>");
        sb.Append(recipe.name);
        sb.Append("</color>");
        sb.Append("\n<color=white><size=4>");
        sb.Append($"配方:{recipe.ruleDescription}");
        sb.Append("</size></color>");
        sb.Append("\n<color=white><size=4>");
        sb.Append($"食效:{recipe.actionDescription}");
        sb.Append("</size></color>");
        return sb.ToString();
    }
}
