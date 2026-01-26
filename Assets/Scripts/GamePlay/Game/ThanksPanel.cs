using UnityEngine;
using UnityEngine.SceneManagement;

// 感谢游玩界面
public class ThanksPanel : MonoBehaviour
{
    public void ReturnToMainMenu(){
        // Debug.Log("【PausePanel】退出游戏");
        SceneManager.LoadSceneAsync("MainMenu");

        GameArchitecture.ResetGame();
    }
}
