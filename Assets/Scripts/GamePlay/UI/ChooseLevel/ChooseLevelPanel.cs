using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseLevelPanel : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image levelImage;
    private int currentDifficulty = 0;
    private int maxDifficulty;

    private int currentLevel = 1;
    private int maxLevel;

    void Start()
    {
        maxDifficulty = this.GetSystem<IDataSystem>().GetAllDifficultyData().Count-1;
        maxLevel = this.GetSystem<IDataSystem>().GetAllLevelData().Count;
    }
    public void ChoosePreviousLevel()
    {
        currentLevel--;
        if (currentLevel < 1)
        {
            currentLevel = 1;
            return;
        }
        UpdateVisual();
    }
    public void ChooseNextLevel()
    {
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            currentLevel = maxLevel;
            return;
        }
        UpdateVisual();
    }
    public void ChoosePreviousDifficulty()
    {
        currentDifficulty--;
        if (currentDifficulty < 0)
        {
            currentDifficulty = 0;
            return;
        }
        UpdateVisual();
    }
    public void ChooseNextDifficulty()
    {
        currentDifficulty++;
        if (currentDifficulty > maxDifficulty)
        {
            currentDifficulty = maxDifficulty;
            return;
        }
        UpdateVisual();
    }
    public void ChooseDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        // 1. 更新难度文本
        string difficultyName = this.GetSystem<IDataSystem>().GetDifficultyData(currentDifficulty).Name;
        difficultyText.text = difficultyName;
        
        // 2. 更新关卡文本
        string levelName = this.GetSystem<IDataSystem>().GetLevelData(currentLevel.ToString()).Name;
        levelText.text = levelName;
    }
}
