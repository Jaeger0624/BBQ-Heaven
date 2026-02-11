using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector; // 记得引入 DOTween

public class RedDotShower : MonoBehaviour
{
    public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    private GameObject redDotPrefab => SettingManager.Instance.PrefabSettings.redDotPrefab;

    [Header("设置")]
    public Corner corner = Corner.TopRight;
    public Vector2 offset = Vector2.zero; // 微调位置

    private GameObject _instance;
    private RectTransform _rect;

    void Start()
    {
        Show();
    }

    [Button]
    public void Show()
    {
        // 1. 懒加载：第一次Show的时候才生成
        if (_instance == null)
        {
            CreateDot();
        }

        _instance.SetActive(true);

        // 2. 呼吸动画 (DOTween)
        _instance.transform.localScale = Vector3.one;
        _instance.transform.DOScale(1.2f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject).SetUpdate(true);
    }

    [Button]
    public void Hide()
    {
        if (_instance != null)
        {
            _instance.transform.DOKill(); // 停止动画防止报错
            _instance.SetActive(false);
        }
    }

    private void CreateDot()
    {
        // 生成预制体，父物体设为当前物体
        _instance = Instantiate(redDotPrefab, transform);
        _rect = _instance.GetComponent<RectTransform>();

        // 核心：设置锚点让它自动贴边
        switch (corner)
        {
            case Corner.TopLeft:     SetAnchor(0, 1); break;
            case Corner.TopRight:    SetAnchor(1, 1); break;
            case Corner.BottomLeft:  SetAnchor(0, 0); break;
            case Corner.BottomRight: SetAnchor(1, 0); break;
        }
        
        _rect.anchoredPosition = offset;
    }

    // 辅助设置锚点方法
    private void SetAnchor(float x, float y)
    {
        _rect.anchorMin = new Vector2(x, y);
        _rect.anchorMax = new Vector2(x, y);
        _rect.pivot = new Vector2(x, y); // 锚点和中心点重合，调整坐标最直观
    }

    private void OnDisable()
    {
        // 养成好习惯，物体被隐藏时杀掉动画
        if (_instance != null) _instance.transform.DOKill();
    }
}