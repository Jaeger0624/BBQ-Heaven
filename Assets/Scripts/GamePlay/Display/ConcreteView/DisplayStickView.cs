using System;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class DisplayStickView : MonoBehaviour, ICanGetSystem, IDisplayItemView<Stick>{
    public Stick stick;
    [SerializeField] private Image stickImage;
    public void Init(Stick stick){
        this.stick = stick;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    public void Bind(Stick data)
    {
        throw new NotImplementedException();
    }

    public void SetInteraction(Action<Stick> onClick)
    {
    }
    public void SetSelectedState(bool isSelected)
    {
        throw new NotImplementedException();
    }

}