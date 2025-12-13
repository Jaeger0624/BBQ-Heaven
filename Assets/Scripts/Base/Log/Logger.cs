using System.Runtime.CompilerServices;
using UnityEngine;

public interface ILogger{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);

    void Log(string message,
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string file = "",
    [CallerLineNumber] int line = 0)
    {
        Debug.Log($"[{System.IO.Path.GetFileName(file)}:{line} - {memberName}] {message}");
    }
}


public class UnityLogger : ILogger{
    public void LogInfo(string message) => Debug.Log(message);
    public void LogWarning(string message) => Debug.LogWarning(message);
    public void LogError(string message) => Debug.LogError(message);
}

public class FileLogger : ILogger{

    public void LogInfo(string message) => WriteToFile($"[Info] {message}");
    public void LogWarning(string message) => WriteToFile($"[Warning] {message}");
    public void LogError(string message) => WriteToFile($"[Error] {message}");

    void WriteToFile(string msg){
        // TODO: 写入文件 
    }
}