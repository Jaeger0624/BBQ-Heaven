using MoreMountains.Feedbacks;
using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;
public enum CharacterEnterDirection
{
    Left = 0,
    Right = 1,
    Center = 2,
}
public class GuideController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private MMF_Player leftEnterFeedback;
    [SerializeField] private MMF_Player rightEnterFeedback;
    [SerializeField] private MMF_Player centerEnterFeedback;
    void Start()
    {
        
    }

    void OnDestroy()
    {
        
    }

    [Button("测试")]
    private void Test(CharacterEnterDirection direction){
        switch (direction)
        {
            case CharacterEnterDirection.Left:
                leftEnterFeedback.PlayFeedbacks();
                break;
            case CharacterEnterDirection.Right:
                rightEnterFeedback.PlayFeedbacks();
                break;
            case CharacterEnterDirection.Center:
                centerEnterFeedback.PlayFeedbacks();
                break;
            default:
                Debug.LogError("Unknown direction: " + direction);
                break;
        }
    }
}