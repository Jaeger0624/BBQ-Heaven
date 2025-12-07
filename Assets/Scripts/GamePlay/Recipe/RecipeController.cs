using System.Collections.Generic;
using System.Text;
using QFramework;
using UnityEngine;

public class RecipeController : MonoBehaviour, IController, ICanSendEvent
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    void OnEnable()
    {
        this.RegisterEvent<MatchRecipeEvent>(OnMatchRecipe);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<MatchRecipeEvent>(OnMatchRecipe);
    }

    private void OnMatchRecipe(MatchRecipeEvent evt)
    {
        List<IAnimTask> animTasks = new List<IAnimTask>();
        // 1. 展示配方消息
        foreach (var recipe in evt.matchedRecipes)
        {
            float time = SettingManager.Instance.AnimSettings.textSpawnLifetime_配方;
            Vector3 position = AnimUtility.GetTextSpawnPosition(Vector3.zero);
            animTasks.Add(new SpawnTextAnimationTask
            (BuildRecipeText(recipe), 8f, Color.white, position, time));
            animTasks.Add(new DelayAnimTask(time, true));
        }
        animTasks.Add(new ActionAnimTask(() => this.SendEvent(new TriggerGAEvent("配方演出动画结束"))));
        animTasks.Add(new DelayAnimTask(1f));



        IAnimTask animTask = new SequenceAnimTask(animTasks);
        this.GetSystem<IAnimationSystem>().Append(animTask);
        this.GetSystem<IAnimationSystem>().Play();
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
