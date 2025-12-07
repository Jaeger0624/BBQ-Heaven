using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        // 音频服务
        builder.AddSingleton(new AudioService(), typeof(IAudioService));
        
    }
}