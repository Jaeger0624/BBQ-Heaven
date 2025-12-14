using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cfg;
using UnityEditor;
using SimpleJSON;
using System.Linq;
using QFramework;
public interface IDataSystem : ISystem{
    //获取食材配置
    FoodData GetFoodData(string id);
    List<FoodData> GetAllFoodData();
    //获取烤串配置
    StickData GetStickData(string id);
    List<StickData> GetAllStickData();
    //获取顾客标签配置
    CustomerTagData GetCustomerTagData(string id);
    //获取配方配置
    RecipeData GetRecipeData(string id);
    List<RecipeData> GetAllRecipeData();
    //获取吉祥物配置
    MascotData GetMascotData(string id);
    List<MascotData> GetAllMascotData();
    //获取玩家角色配置
    PCData GetPCData(string id);
    List<PCData> GetAllPCData();
    //获取卡牌配置
    CardData GetCardData(string id);
    List<CardData> GetAllCardData();

    // 获取遭遇配置
    EncounterData GetEncounterData(string id);
    List<EncounterData> GetAllEncounterData();
    //获取关卡配置
    LevelData GetLevelData(string id);
    List<LevelData> GetAllLevelData();
    //获取难度配置
    DifficultyData GetDifficultyData(int id);
    List<DifficultyData> GetAllDifficultyData();
    //获取地块配置
    TileData GetTileData(string id);
    List<TileData> GetAllTileData();
}
public class DataSystem : AbstractSystem, IDataSystem{

    public Tables tables;
    public void GenerateData()
    {
        tables = new Tables(GetData);
    }
    private JSONNode GetData(string tableName){
        var textAsset = Resources.Load<TextAsset>("Config/" + tableName);

        return JSON.Parse(textAsset.text);
    }
    public FoodData GetFoodData(string id){
        return tables.FoodDataTable.Get(id);
    }
    public StickData GetStickData(string id){
        return tables.StickDataTable.Get(id);
    }
    public CustomerTagData GetCustomerTagData(string id){
        return tables.CustomerTagDataTable.Get(id);
    }
    public RecipeData GetRecipeData(string id){
        return tables.RecipeDataTable.Get(id);
    }
    public MascotData GetMascotData(string id){
        return tables.MascotDataTable.Get(id);
    }
    public PCData GetPCData(string id){
        return tables.PCDataTable.Get(id);
    }
    public List<RecipeData> GetAllRecipeData(){
        return tables.RecipeDataTable.DataList;
    }
    public List<MascotData> GetAllMascotData(){
        return tables.MascotDataTable.DataList;
    }
    public List<PCData> GetAllPCData(){
        return tables.PCDataTable.DataList;
    }
    public List<FoodData> GetAllFoodData(){
        return tables.FoodDataTable.DataList;
    }
    public List<StickData> GetAllStickData(){
        return tables.StickDataTable.DataList;
    }
    public CardData GetCardData(string id){
        return tables.CardDataTable.Get(id);
    }
    public List<CardData> GetAllCardData(){
        return tables.CardDataTable.DataList;
    }
    protected override void OnInit()
    {
        GenerateData();
    }

    public EncounterData GetEncounterData(string id)
    {
        return tables.EncounterDataTable.Get(id);
    }

    public List<EncounterData> GetAllEncounterData()
    {
        return tables.EncounterDataTable.DataList;
    }

    public LevelData GetLevelData(string id)
    {
        return tables.LevelDataTable.Get(id);
    }
    public List<LevelData> GetAllLevelData()
    {
        return tables.LevelDataTable.DataList;
    }
    public DifficultyData GetDifficultyData(int id)
    {
        return tables.DifficultyDataTable.Get(id);
    }
    public List<DifficultyData> GetAllDifficultyData()
    {
        return tables.DifficultyDataTable.DataList;
    }

    public TileData GetTileData(string id)
    {
        return tables.TileDataTable.Get(id);
    }

    public List<TileData> GetAllTileData()
    {
        return tables.TileDataTable.DataList;
    }
}