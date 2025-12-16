using cfg;

public class NewGameInfo{
    public string pcID;
    public int seed;
    public DifficultyData difficultyData;
    public LevelData levelData;
    public NewGameInfo(string pcID, int seed, DifficultyData difficultyData, LevelData levelData){
        this.seed = seed;
        this.pcID = pcID;
        this.difficultyData = difficultyData;
        this.levelData = levelData;
    }
}