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
    public List<GameAction> GA;
    private RecipeData recipeData;
    public Recipe(RecipeData recipeData){
        this.recipeData = recipeData;
        this.name = recipeData.Name;
        this.description = recipeData.Description;
        this.ruleDescription = recipeData.RuleDescription;
        this.actionDescription = recipeData.ActionDescription;
        this.rules = recipeData.Rules;
        this.GA = recipeData.GA.Select(x => x.Clone()).ToList();
        this.recipeData = recipeData;
    }
}