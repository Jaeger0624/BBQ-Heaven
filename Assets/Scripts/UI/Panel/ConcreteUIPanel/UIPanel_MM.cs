using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using QFramework;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

public class UIPanel_MM : MonoBehaviour, IController, IUIPanel{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private bool Single = true;
    [SerializeField] private MMF_Player mmfPlayer_Show;
    [HideIf("Single")]
    [SerializeField] private MMF_Player mmfPlayer_Hide;
    void Start()
    {
        if (Single){
            // mmfPlayer_Show.
        }
    }
    public void Show(){
        if (Single){
            mmfPlayer_Show.PlayFeedbacksTopToBottom();
        }
        else{
            mmfPlayer_Show.PlayFeedbacks();
        }
    }
    public IObservable<Unit> Hide(){
        if (Single){
            mmfPlayer_Show.PlayFeedbacksBottomToTop();
            // mmfPlayer_Show.MMEventStartListening
        }
        else{
            if (mmfPlayer_Hide == null) {Debug.LogError("UIPanel_MM: mmfPlayer_Hide is not set"); return Observable.ReturnUnit();}
            // mmfPlayer_Hide.PlayFeedbacks();
        }
        return Observable.ReturnUnit();
    }
}