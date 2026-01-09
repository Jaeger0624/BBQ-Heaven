using MoreMountains.Feedbacks;
using QFramework;
using Sirenix.OdinInspector;
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
    public void Hide(){

        if (Single){
            mmfPlayer_Show.PlayFeedbacksBottomToTop();
        }
        else{
            if (mmfPlayer_Hide == null) {Debug.LogError("UIPanel_MM: mmfPlayer_Hide is not set"); return;}
            mmfPlayer_Hide.PlayFeedbacks();
        }
    }
}