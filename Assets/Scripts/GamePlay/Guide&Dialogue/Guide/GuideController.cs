using cfg;
using MoreMountains.Feedbacks;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class GuideController : MonoBehaviour, IController, ICanSendEvent{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    public void StartGuide(string guideID)
    {
        this.GetSystem<IGuideSystem>().StartGuide(guideID);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.SendEvent(new PlayerActionEvent(PlayerActionType.点击任意处));
        }
    }
}