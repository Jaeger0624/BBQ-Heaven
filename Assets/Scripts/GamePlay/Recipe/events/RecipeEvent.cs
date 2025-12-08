using System.Collections.Generic;
using QFramework;

public class MatchRecipeEvent : AbstractEvent{
    public List<Recipe> matchedRecipes;
    public MatchRecipeEvent(List<Recipe> matchedRecipes){
        this.matchedRecipes = matchedRecipes;
    }
}