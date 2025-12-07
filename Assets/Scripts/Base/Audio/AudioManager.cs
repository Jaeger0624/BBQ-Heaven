using QFramework;
using UnityEngine;

public class AudioManager : MonoBehaviour, IController{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;
    private IAudioService audioService;
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        audioService = new AudioService();
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public IAudioService AudioService => audioService;
}