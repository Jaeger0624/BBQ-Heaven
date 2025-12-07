using System.Collections.Generic;
using System.Linq;
using System.Text;
using QFramework;
using UnityEngine;

public interface IRecipeSystem : ISystem{
    List<Recipe> Recipes();
    /// <summary>
    /// 执行配方匹配
    /// </summary>
    /// <param name="bbq"></param>
    void MatchRecipe(List<object> param);
    void RegisterRecipe(string id);
    void RegisterAllRecipes();
}
public class RecipeSystem : AbstractSystem, IRecipeSystem
{
    private List<Recipe> recipeRepository;
    public List<Recipe> Recipes() => recipeRepository;
    private IMatchRecipeStrategy matchRecipeStrategy;   
    // 在BBQ构建完毕后，执行配方匹配
    public void MatchRecipe(List<object> param)
    {
        // Debug.Log($"【RecipeSystem】执行配方匹配");

        BBQProcessContext context = param?.FirstOrDefault() as BBQProcessContext;
        if (context == null){
            Debug.LogError("上下文为空");
            return;
        }
        BBQ bbq = context.targetBBQ;

        // 1. 获取匹配成功的配方列表
        List<Recipe> matchedRecipes = matchRecipeStrategy.Match(bbq, recipeRepository);
        if (matchedRecipes.Count == 0)
        {
            Debug.Log($"【RecipeSystem】没有匹配到任何配方");
        }
        else{
            string matchedRecipesString = string.Join(", ", matchedRecipes.Select(x => x.name));
            Debug.Log($"【RecipeSystem】匹配成功的配方: {matchedRecipesString}");
        }

        
        // 1.1 发送匹配成功事件
        this.SendEvent(new MatchRecipeEvent(matchedRecipes));


        // 2. 执行匹配成功的配方
        matchedRecipes.ForEach(recipe => ExecuteRecipe(recipe, param));
    }
    protected override void OnInit()
    {
        recipeRepository = new List<Recipe>();
        matchRecipeStrategy = new MatchRecipeStrategy_默认();
    }
    protected override void OnDeinit()
    {
        recipeRepository.Clear();
    }
    public void RegisterRecipe(string id){
        Recipe recipe = new Recipe(this.GetSystem<IDataSystem>().GetRecipeData(id));
        // Debug.Log($"【RecipeSystem】注册配方: {recipe.name}");
        recipeRepository.Add(recipe);
    }

    private void ExecuteRecipe(Recipe recipe, List<object> param){
        recipe.GA.ForEach(ga => this.GetSystem<IGASystem>().ApplyGA(recipe, ga, param));
    }

    public void RegisterAllRecipes(){
        StringBuilder sb = new StringBuilder();
        this.GetSystem<IDataSystem>().GetAllRecipeData().ForEach(recipeData => {
            RegisterRecipe(recipeData.ID);
            sb.AppendLine($"- {recipeData.Name}");
        });
        Debug.Log($"【RecipeSystem】注册所有配方（测试用）: \n{sb.ToString()}");
    }
}

