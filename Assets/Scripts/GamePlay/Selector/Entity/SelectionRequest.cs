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


