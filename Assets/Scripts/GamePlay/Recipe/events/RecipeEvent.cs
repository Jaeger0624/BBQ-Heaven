using System.Collections.Generic;

public class MatchRecipeEvent{
    public List<Recipe> matchedRecipes;
    public MatchRecipeEvent(List<Recipe> matchedRecipes){
        this.matchedRecipes = matchedRecipes;
    }
}