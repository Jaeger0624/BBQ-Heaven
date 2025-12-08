using System.Collections.Generic;
using QFramework;

public class MatchRecipeEvent : IEvent{
    public List<Recipe> matchedRecipes;
    public MatchRecipeEvent(List<Recipe> matchedRecipes){
        this.matchedRecipes = matchedRecipes;
    }
}