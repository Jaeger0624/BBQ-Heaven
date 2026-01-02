using System.Collections.Generic;
using QFramework;
using UnityEngine;

// 用于提供全局访问点，主要是用于控制器之间方便的通信
public class BlackboardSystem : AbstractSystem
{
    public NewGameInfo newGameInfo = new NewGameInfo("1", Random.Range(0, 1000000), null, null);
    public BoardCell hoveredCell = null;
    public FoodInstance currentFoodInstance = null;
    private FoodInstance oldInstance = null;

    // 2026.1.1:
    // 添加一个全局上下文
    public List<object> globalContext = new List<object>();

    
    #region TimeSystem 时间
    public int makeBBQTime = 4;
    public int supplyFoodTime = 2;
    public int soldBBQTime = 1;
    #endregion
    protected override void OnInit()
    {
    }

    public void OpenCurrentFoodInstance(FoodInstance foodInstance){
        if (currentFoodInstance != null){
            oldInstance = currentFoodInstance;
        }
        currentFoodInstance = foodInstance;
    }
    public void CloseCurrentFoodInstance(){
        if (oldInstance != null){
            currentFoodInstance = oldInstance;
        }
        oldInstance = null;
    }
}