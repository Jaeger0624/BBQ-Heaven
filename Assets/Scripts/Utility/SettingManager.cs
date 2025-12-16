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
        
        // 配置DOTween使用smoothDeltaTime，这在低帧率下能提供更平滑的动画
        // 这是解决低帧率下动画问题的关键
        DOTween.useSmoothDeltaTime = true;
        Debug.Log("【SettingManager】启用DOTween useSmoothDeltaTime，提升低帧率下的动画平滑度");
        
        // 设置目标帧率
        // 注意：帧率越高，动画越正常；帧率越低，动画越容易出现问题
        // 建议：不限制帧率（设置为0），或者使用较高的帧率（120+）
        if (_devSettings != null && _devSettings.targetFrameRate > 0)
        {
            Application.targetFrameRate = _devSettings.targetFrameRate;
            Debug.Log($"【SettingManager】设置目标帧率: {Application.targetFrameRate} FPS");
            
            // 如果帧率设置较低（< 60），给出警告
            if (_devSettings.targetFrameRate < 60)
            {
                Debug.LogWarning($"【SettingManager】警告：目标帧率 {_devSettings.targetFrameRate} FPS 较低，可能导致动画问题。建议设置为60+或0（不限制）");
            }
        }
        else
        {
            // 如果没有配置或设置为0，不限制帧率（让帧率自然运行）
            // 这样可以获得最佳的动画效果
            Application.targetFrameRate = 0; // 0表示不限制帧率
            Debug.Log("【SettingManager】不限制帧率，让帧率自然运行（推荐，可获得最佳动画效果）");
        }

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
        #if UNITY_EDITOR


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

        #endif
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    /// <summary>
    /// 满意度文本父对象
    /// </summary>
    public Transform SatisfactionTextParent;
}