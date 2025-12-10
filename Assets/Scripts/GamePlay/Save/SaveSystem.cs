using System.Collections.Generic;
using System.IO;
using QFramework;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEngine;

public interface ISaveSystem : ISystem{
    void SaveGame();
    void LoadGame();
}
public class SaveSystem : AbstractSystem, ISaveSystem{
    private string savePath => Application.persistentDataPath + "/save.json";
    protected override void OnInit()
    {
    }
    protected override void OnDeinit()
    {
    }
    public void SaveGame(){
        Debug.Log("Save Game");

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
    public void LoadGame(){
        if (!File.Exists(savePath)) 
        {
            Debug.LogWarning("存档文件不存在！");
            return;
        }

        try 
        {
            // 1. 【核心修改】使用 Odin 反序列化
            byte[] bytes = File.ReadAllBytes(savePath);
            var gameArchive = SerializationUtility.DeserializeValue<GameArchive>(bytes, DataFormat.Binary);

            if (gameArchive == null){
                Debug.LogError("读取存档失败: 存档文件为空");
                return;
            }

            Debug.Log("游戏读取成功！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取存档失败: {e.Message}");
        }
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