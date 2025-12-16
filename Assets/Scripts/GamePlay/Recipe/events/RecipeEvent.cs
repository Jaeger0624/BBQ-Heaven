using System.Collections.Generic;
using QFramework;

public class MatchRecipeEvent : AbstractEvent{
    public List<Recipe> matchedRecipes;
    public MatchRecipeEvent(List<Recipe> matchedRecipes){
        this.matchedRecipes = matchedRecipes;
    }
}

public class RecipePreview{
    public int rank;
    public Recipe recipe;
    public RecipePreview(int rank, Recipe recipe){
        this.rank = rank;
        this.recipe = recipe;
    }
}
public class MatchRecipePreviewEvent : AbstractEvent{
    public List<RecipePreview> recipePreviews;
    public MatchRecipePreviewEvent(List<RecipePreview> recipePreviews){
        this.recipePreviews = recipePreviews;
    }
}
public class ResetRecipePreviewViewsEvent : AbstractEvent{
    public ResetRecipePreviewViewsEvent(){}
}