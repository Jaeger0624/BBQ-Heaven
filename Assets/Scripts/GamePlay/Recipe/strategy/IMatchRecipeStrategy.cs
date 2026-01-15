using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 配方匹配策略接口（算法型，不会改变匹配规则，只是可能可以优化）
/// </summary>
public interface IMatchRecipeStrategy{
    /// <summary>
    /// 匹配配方，传入现有配方列表，返回匹配成功的配方列表
    /// </summary>
    /// <param name="bbq">当前烧烤</param>
    /// <param name="recipes">现有配方列表</param>
    /// <returns>匹配成功的配方列表</returns>
    List<RecipeResult> Match(BBQ bbq, List<Recipe> recipes);
}

public class MatchRecipeStrategy_默认 : IMatchRecipeStrategy
{
    public List<RecipeResult> Match(BBQ bbq, List<Recipe> recipes)
    {
        BBQProcessContext context = new BBQProcessContext(bbq);
        List<RecipeResult> matchedRecipePreviews = new List<RecipeResult>();
        foreach (var recipe in recipes)
        {
            int rank = recipe.PreviewMatch(new List<object>{context});
            if (rank > 0)
            {
                matchedRecipePreviews.Add(new RecipeResult(rank, recipe));
            }
        }
        return matchedRecipePreviews;
    }
}
