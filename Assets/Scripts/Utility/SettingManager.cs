using QFramework;
using Reflex.Attributes;
using UnityEngine;
using DG.Tweening;

public class SettingManager : MonoBehaviour, IController,ICanSendEvent{
    private static SettingManager _instance;
    public static SettingManager Instance{
        get{
            if (_instance == null){
                // 创建一个全局单例
                GameObject obj = new GameObject("SettingManager");
                _instance = obj.AddComponent<SettingManager>();
            }
            return _instance;
        }
    }
    public float DefaultAnimInterval => _devSettings.defaultAnimInterval;
    private float currentTimeScale = 0;
    void Awake(){
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        // 获取Settings
        _devSettings = Resources.Load<DevSettings>("SO/DevSettings");
        _animSettings = Resources.Load<AnimSettings>("SO/AnimSettings");
        _gameplaySettings = Resources.Load<GameplaySettings>("SO/GameplaySettings");
    }
    [Inject]
    public IAudioService audioService;
    [SerializeField] private DevSettings _devSettings;
    public DevSettings DevSettings => _devSettings;
    [SerializeField] private AnimSettings _animSettings;
    public AnimSettings AnimSettings => _animSettings;
    [SerializeField] private GameplaySettings _gameplaySettings;
    public GameplaySettings GameplaySettings => _gameplaySettings;
    [SerializeField] private PrefabSettings _prefabSettings;
    public PrefabSettings PrefabSettings => _prefabSettings;
    public static T GetSetting<T>() where T : ScriptableObject{
        if (typeof(T) == typeof(DevSettings)){
            return Instance.DevSettings as T;
        }
        else if (typeof(T) == typeof(AnimSettings)){
            return Instance.AnimSettings as T;
        }
        else if (typeof(T) == typeof(GameplaySettings)){
            return Instance.GameplaySettings as T;
        }
        else{
            Debug.LogError($"【SettingManager】获取设置失败：{typeof(T).Name}");
            return null;
        }
    }

    void Update()
    {
        
        // #if UNITY_EDITOR


        if (DevSettings.UseAnimTimeScale){
            Time.timeScale = DevSettings.AnimTimeScale;
        }
        else{
            // if (!this.GetSystem<IAnimationSystem>().IsPlaying.ContainsKey("default")) return;
            if (this.GetSystem<IAnimationSystem>().IsPlaying[AnimQueue.Default]){
                
                currentTimeScale = Mathf.Min(Time.timeScale + DevSettings.AnimTimeScaleSpeed, DevSettings.maxTimeScale);
                Time.timeScale = currentTimeScale;
            }
            else{
                currentTimeScale = DevSettings.baseTimeScale;
                Time.timeScale = currentTimeScale;
            }
        }

        // #endif
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    /// <summary>
    /// 满意度文本父对象
    /// </summary>
    public Transform SatisfactionTextParent;
}