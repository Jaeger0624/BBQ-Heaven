using QFramework;
using UnityEngine;

public class AnimationController : MonoBehaviour, IController
{
    void OnEnable()
    {
    }
    void OnDisable()
    {
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
