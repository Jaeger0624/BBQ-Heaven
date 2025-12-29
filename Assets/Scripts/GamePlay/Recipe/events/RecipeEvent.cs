using System.Collections.Generic;
using QFramework;

public class MatchRecipeEvent : AbstractEvent{
    public List<RecipeResult> matchedRecipePreviews;
    public MatchRecipeEvent(List<RecipeResult> matchedRecipePreviews){
        this.matchedRecipePreviews = matchedRecipePreviews;
    }
}

public class RecipeResult{
    public int rank;
    public Recipe recipe;
    public RecipeResult(int rank, Recipe recipe){
        this.rank = rank;
        this.recipe = recipe;
    }
}
public class MatchRecipePreviewEvent : AbstractEvent{
    public List<RecipeResult> recipePreviews;
    public MatchRecipePreviewEvent(List<RecipeResult> recipePreviews){
        this.recipePreviews = recipePreviews;
    }
}
public class ResetRecipePreviewViewsEvent : AbstractEvent{
    public ResetRecipePreviewViewsEvent(){}
}