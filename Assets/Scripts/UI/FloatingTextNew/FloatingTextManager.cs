using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("配置")]
    [SerializeField] private FloatingTextInstance _prefab;
    [SerializeField] private Transform _uiContainer; // 必须是 Canvas 下的节点
    [SerializeField] private Camera _worldCamera;    // 如果不需要世界坐标转UI，可不填
    [SerializeField] private RectTransform _testTargetUI;


    private Queue<FloatingTextInstance> _pool = new Queue<FloatingTextInstance>();

    private void Awake()
    {
        Instance = this;
        if (_worldCamera == null) _worldCamera = Camera.main;
    }

    // ============================================
    // API 1: 普通伤害/状态 (最常用)
    // ============================================
    public void Show(Vector3 worldPos, string content, Color? color = null, FloatingTextInfo info = null)
    {
        Spawn(worldPos, content, FloatingTextMode.Normal, null, ScatterMode.Both, null, color, info);
    }
    
    // 重载：传数值
    public void Show(Vector3 worldPos, float value, Color? color = null, FloatingTextInfo info = null)
    {
        Show(worldPos, Mathf.RoundToInt(value).ToString(), color, info);
    }

    // ============================================
    // API 2: 物理散布 (仅掉落，不飞)
    // ============================================
    public void ShowDrop(Vector3 worldPos, string content, Color? color = null)
    {
        Spawn(worldPos, content, FloatingTextMode.Physics, null, ScatterMode.Both, null, color);
    }

    // ============================================
    // API 3: 收集模式 (散开 -> 飞向UI)
    // ============================================
    public void ShowGather(Vector3 worldPos, RectTransform targetUI, string content, ScatterMode scatterMode,Color? color = null,System.Action onArrive = null)
    {
        // 目标位置加一点微小随机，防止完全重叠
        Vector3 targetPos = targetUI.position + (Vector3)Random.insideUnitCircle * 0.5f;
        Spawn(worldPos, content, FloatingTextMode.Gather, targetPos, scatterMode, onArrive, color);
    }

    // ============================================
    // 内部实现
    // ============================================
    private void Spawn(Vector3 worldPos, string content, FloatingTextMode mode, Vector3? targetPos, ScatterMode scatterMode, System.Action onArrive, Color? color, FloatingTextInfo info = null)
    {
        // 1. 坐标转换 (World -> Screen)
        // 如果你的 UI 是 ScreenSpace-Overlay:
        
        // 2. 对象池获取
        var instance = GetFromPool();

        // 3. 初始化
        instance.Init(content, worldPos, mode, ReturnToPool, targetPos, scatterMode, onArrive, color, 1f, info);
    }

    private FloatingTextInstance GetFromPool()
    {
        if (_pool.Count > 0) return _pool.Dequeue();
        return Instantiate(_prefab, _uiContainer);
    }

    private void ReturnToPool(FloatingTextInstance item)
    {
        item.gameObject.SetActive(false);
        _pool.Enqueue(item);
    }

    void Update()
    {
        // 每10帧一个测试
        if (Time.frameCount % 10 == 0)
        {
            if (SettingManager.Instance.FloatingTextSettings.testFloatingText)
            {
                Test_战利品收集();
            }
        }
    }

    [Button]
    private void Test_战利品收集()
    {
        ShowGather(new Vector3(0, 2, 0), _testTargetUI, "美味度", SettingManager.Instance.FloatingTextSettings.testScatterMode, null);
    }

    [Button]
    private void Test_跳字()
    {
        Show(new Vector3(0, 2, 0), "<size=36><color=yellow>美味度</color></size> \n<size=56>1000x</size>", Color.white);
    }



    #region API - 具体使用场景封装

    public void GenerateFloatingText_美味度(string content,Transform targetTransform,Color? color = null,System.Action onArrive = null){
        ScatterMode scatterMode = ScatterMode.Right;
        ShowGather(targetTransform.position, targetTransform.GetComponent<RectTransform>(), content, scatterMode, color, onArrive);
    }
    
    public void GenerateFloatingText_珍稀度(string content,Transform targetTransform,Color? color = null,System.Action onArrive = null){
        ScatterMode scatterMode = ScatterMode.Left;
        ShowGather(targetTransform.position, targetTransform.GetComponent<RectTransform>(), content, scatterMode, color, onArrive);
    }



    #endregion
}

public class FloatingTextInfo{
    public string content;
    public float duration;
    public Color? color;
    public Vector2 direction = Vector2.up;
    public FloatingTextInfo(string content, float duration, Color? color, Vector2 direction = default){
        this.content = content;
        this.duration = duration;
        this.color = color;
        this.direction = direction == default ? Vector2.up : direction;
    }
}