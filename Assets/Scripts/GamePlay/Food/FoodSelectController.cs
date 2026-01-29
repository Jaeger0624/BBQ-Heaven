using QFramework;
using UnityEngine;

public class FoodSelectController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }

    void Update()
    {
        
        DetectSelectFood();
    }

    private void DetectSelectFood(){
        // 在有烤串被选中的情况下，如果点击食材
        if (this.GetSystem<IStickSystem>().selectedStick != null && Input.GetMouseButtonDown(0)){
            if (this.GetSystem<BlackboardSystem>().hoveredCell == null) return;
        }
    }
}
