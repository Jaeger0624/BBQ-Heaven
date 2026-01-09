using System.Collections.Generic;
using System.IO;
using QFramework;
using Sirenix.Serialization;
using UnityEngine;

public interface ISaveSystem : ISystem{
    SystemData SystemData { get; }
    void SaveGame();
    void SaveSystemData();
    void LoadGame(GameArchive gameArchive);
    void LoadSystemData();
    GameArchive GetGameArchive();
}
public class SaveSystem : AbstractSystem, ISaveSystem{
    // 存到Persistent文件夹下
    private string savePath => "Assets/Persistent/save.json";
    private string systemDataPath => "Assets/Persistent/systemData.json";
    public SystemData SystemData { get; private set; }
    protected override void OnInit()
    {
        LoadSystemData();
    }
    protected override void OnDeinit()
    {
        SaveSystemData();
    }

    public void SaveSystemData(){
        byte[] bytes = SerializationUtility.SerializeValue(SystemData, DataFormat.Binary);
        File.WriteAllBytes(systemDataPath, bytes);
    }
    public void LoadSystemData(){
        if (File.Exists(systemDataPath)){
            byte[] bytes = File.ReadAllBytes(systemDataPath);
            SystemData = SerializationUtility.DeserializeValue<SystemData>(bytes, DataFormat.Binary);
            if (SystemData == null){
                Debug.LogError("系统数据文件解析失败！");
                return;
            }
            else{
                Debug.Log("系统数据文件解析成功！");
            }
        }
        else{
            Debug.Log("系统数据文件不存在！");
            SystemData = new SystemData();
            byte[] bytes = SerializationUtility.SerializeValue(SystemData, DataFormat.Binary);
            File.WriteAllBytes(systemDataPath, bytes);
        }
    }
    public void SaveGame(){
        // Debug.Log("Save Game");

        GameArchive gameArchive = new GameArchive();
        // 获取GameArchitecture中的所有系统
        foreach(var system in GetAllSystems()){
            system.Save(gameArchive);
        }

        // 2. 【核心修改】使用 Odin 序列化
        // DataFormat.Binary: 性能最高，体积最小，完美支持多态
        byte[] bytes = SerializationUtility.SerializeValue(gameArchive, DataFormat.Binary);
        
        File.WriteAllBytes(savePath, bytes);
        
        Debug.Log($"游戏已保存至: {savePath}");
    }
    public void LoadGame(GameArchive gameArchive){
        // Debug.Log("游戏读取成功！");

        foreach(var system in GetAllSystems()){
            system.Load(gameArchive);
        }
    }

    public GameArchive GetGameArchive(){
        if (!File.Exists(savePath)) 
        {
            Debug.LogWarning("存档文件不存在！");
            return null;
        }
        byte[] bytes = File.ReadAllBytes(savePath);
        GameArchive gameArchive = SerializationUtility.DeserializeValue<GameArchive>(bytes, DataFormat.Binary);
        if (gameArchive == null)
        {
            Debug.LogError("存档文件解析失败！");
            return null;
        }
        else
        {
            Debug.Log("存档文件解析成功！");
        }
        return gameArchive;
    }

    private IEnumerable<ISavable> GetAllSystems(){
        List<ISavable> systems = new List<ISavable>();
        foreach(var system in GameArchitecture.Interface.GetAllSystems()){
            if (system is ISystem && system is ISavable){
                ISavable systemObj = system as ISavable;
                systems.Add(systemObj);
            }
        }
        return systems;
    }
}