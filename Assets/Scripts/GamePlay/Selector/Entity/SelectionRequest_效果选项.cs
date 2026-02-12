using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;


public class SelectionRequest_效果选项 : AbstractSelectionRequest{
    public override string Title => "选择一个效果";
    public override int Amount { get; set; } = 1;
    private List<OptionData> Options;
    public override Action<SelectionBuildContext> OnSelect { get; set; } = null;
    private object Sender;
    private List<object> Param;
    public SelectionRequest_效果选项(List<OptionData> options, object sender, List<object> param){
        this.Options = options;
        this.OnSelect = OnSelection;
        this.Sender = sender;
        this.Param = param;
    }
    public override SelectRequest Create(){
        return new SelectRequest(Options.Select(x => new SelectionBuildContext(SelectionType.选项, x.ID)).ToList(), Title, OnSelect);
    }

    private void OnSelection(SelectionBuildContext optionContext){
        OptionData optionData = Options.FirstOrDefault(x => x.ID == optionContext.Id);
        if (optionData == null){
            Debug.LogError("【SelectionRequest】选择的效果不存在：" + optionContext.Id);
            return;
        }
        foreach (var cga in optionData.Actions){
            this.GetSystem<IGASystem>().TriggerReaction(new CGA(cga), Sender, Param);
        }
    }
}