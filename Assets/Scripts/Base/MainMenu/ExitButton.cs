using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    void Awake()
    {
        exitButton = this.GetComponent<Button>();
        exitButton.onClick.AddListener(OnExitButtonClick);
    }

    private void OnExitButtonClick()
    {
        LogKit.I("【ExitButton】退出游戏");
        // 退出游戏
        Application.Quit();
    }

}
