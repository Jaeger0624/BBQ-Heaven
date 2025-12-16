using System.Collections.Generic;
using System.Linq;
using cfg;

/// <summary>
/// 配方实例类 - 实例层
/// </summary>
public class Recipe{
    public string id => recipeData.ID;
    public string name;
    public string description;
    public string ruleDescription;
    public string actionDescription;
    public List<RecipeRule> rules;
    private RecipeData recipeData;
    public Recipe(RecipeData recipeData){
        this.recipeData = recipeData;
        this.name = recipeData.Name;
        this.description = recipeData.Description;
        this.ruleDescription = recipeData.RuleDescription;
        this.actionDescription = recipeData.ActionDescription;
        this.rules = recipeData.Rules;
        this.recipeData = recipeData;
    }

    public int PreviewMatch(List<object> param)
    {
        foreach (var rule in rules)
        {
            if (rule.EvaluatePreview(param))
            {
                return rule.Rank;
            }
        }
        return 0;
    }
}