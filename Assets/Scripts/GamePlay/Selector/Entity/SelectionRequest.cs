using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;

public interface ISelectionRequest : ICanGetSystem{
    string Title { get; }
    int Amount { get; set; }
    Action<SelectionBuildContext> OnSelect { get; set; }
    SelectRequest Create();
}
public abstract class AbstractSelectionRequest : ISelectionRequest , ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public abstract string Title { get; }
    public abstract int Amount { get; set; }
    public abstract Action<SelectionBuildContext> OnSelect { get; set; }
    public abstract SelectRequest Create();
}


public class SelectionRequest_自定义 : AbstractSelectionRequest{
    public override string Title { get; }
    public override int Amount { get; set; }
    public List<SelectionBuildContext> Contexts;
    public override Action<SelectionBuildContext> OnSelect { get; set; }
    public SelectionRequest_自定义(string title, int amount, List<SelectionBuildContext> contexts,Action<SelectionBuildContext> onSelect){
        Title = title;
        Amount = amount;
        Contexts = contexts;
        OnSelect = onSelect;
    }
    public override SelectRequest Create(){
        return new SelectRequest(Contexts, Title, OnSelect);
    }
}