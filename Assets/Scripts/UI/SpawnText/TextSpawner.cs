using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
public interface ITextSpawner{
    void Spawn(string content, float size, Vector3 worldPosition, Color color, float? overrideLifetime, bool useUnscaledTime);
}
public class TextSpawner : MonoBehaviour, ITextSpawner{
    public Queue<string> textQueue = new Queue<string>();
    
    [Header("Prefab & Parent")]
    [SerializeField] private TextMeshPro textPrefab;
    [SerializeField] private Transform worldParent;

    [Header("Animation Settings")]
    [SerializeField] private float lifetime => SettingManager.Instance.AnimSettings.textSpawnLifetime_默认;
    [SerializeField] private float launchDistance = 1.0f;
    [SerializeField] private float randomAngleDeg = 25f;
    [SerializeField] private float minScale = 0.75f;
    [SerializeField] private float maxScale = 1.15f;
    // 上升-回弹曲线：0 -> 1(上升峰值) -> 0.85(回弹后停留)
    [SerializeField] private float zOffset = -0.3f;
    [SerializeField] private AnimationCurve upDownCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 4f),
        new Keyframe(0.65f, 1f, 0f, 0f),
        new Keyframe(1f, 0.85f, -2f, 0f)
    );
    // 透明度曲线：淡入->保持->淡出
    [SerializeField] private AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 8f, 0f),
        new Keyframe(0.12f, 1f, 0f, 0f),
        new Keyframe(0.8f, 1f, 0f, 0f),
        new Keyframe(1f, 0f, 0f, 0f)
    );

    public void Spawn(string content, float size, Vector3 worldPosition){
        Spawn(content, size, worldPosition, Color.white, null, true);
    }

    public void Spawn(string content, float size, Vector3 worldPosition, Color color){
        Spawn(content, size, worldPosition, color, null, true);
    }

    public void Spawn(string content, float size, Transform anchor, Vector3 offset){
        Spawn(content, size, anchor.position + offset, Color.white, null, true);
    }

    public void Spawn(string content, float size, Vector3 worldPosition, Color color, float? overrideLifetime){
        Spawn(content, size, worldPosition, color, overrideLifetime, true);
    }

    public void Spawn(string content, float size, Vector3 worldPosition, Color color, float? overrideLifetime, bool useUnscaledTime){
        if (textPrefab == null) return;
        
        // 预先计算所有值，避免在激活后计算
        Vector3 startPos = worldPosition + new Vector3(0, 0, zOffset);
        // 2D：围绕 Z 轴旋转，上方随机扇形弹射
        Vector2 dir2 = Quaternion.Euler(0f, 0f, Random.Range(-randomAngleDeg, randomAngleDeg)) * Vector2.up;
        Vector3 dir = new Vector3(dir2.x, dir2.y, 0f);
        float dist = launchDistance * Random.Range(0.85f, 1.15f);
        Vector3 target = worldPosition + dir.normalized * dist + new Vector3(0, 0, zOffset);
        float life = overrideLifetime ?? lifetime;
        
        // 实例化时保持非激活状态，避免触发不必要的渲染和回调
        TextMeshPro tmp = Instantiate(textPrefab, worldParent != null ? worldParent : transform);
        tmp.gameObject.SetActive(false);
        
        // 在非激活状态下设置所有属性，避免渲染开销
        tmp.text = content;
        tmp.transform.position = startPos;
        tmp.fontSize = size;
        // 设置初始透明度为0（根据alphaCurve，0时刻alpha为0）
        Color initialColor = color;
        initialColor.a = alphaCurve.Evaluate(0f);
        tmp.color = initialColor;
        // 设置初始缩放
        float initialScale = Mathf.Lerp(minScale, maxScale, Mathf.Sin(0f * Mathf.PI));
        tmp.transform.localScale = Vector3.one * initialScale;
        
        // 所有属性设置完成后，再激活并开始动画
        tmp.gameObject.SetActive(true);
        AnimateRx(tmp, startPos, target, life, color, useUnscaledTime);
    }

    private void AnimateRx(TextMeshPro tmp, Vector3 start, Vector3 target, float life, Color baseColor, bool useUnscaledTime){
        float t = 0f;
        var disposable = new CompositeDisposable();
        
        // 立即开始动画，初始状态已在Spawn中设置好
        Observable.EveryUpdate()
            .TakeWhile(_ => t < life && tmp != null)
            .Subscribe(_ => {
                if (tmp == null) return;
                // 根据useUnscaledTime决定使用deltaTime还是unscaledDeltaTime
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float p = Mathf.Clamp01(t / life);
                float m = upDownCurve.Evaluate(p);
                // 小->大->小：用正弦实现平滑脉动
                float sin01 = Mathf.Sin(p * Mathf.PI); // 0..1..0
                float s = Mathf.Lerp(minScale, maxScale, sin01);
                float a = alphaCurve.Evaluate(p);

                tmp.transform.position = Vector3.LerpUnclamped(start, target, m);
                tmp.transform.localScale = Vector3.one * s;
                var c = baseColor; c.a = a; tmp.color = c;
            }, () => {
                // onCompleted
                if (tmp != null) Destroy(tmp.gameObject);
                disposable?.Dispose();
            })
            .AddTo(disposable);

        // 兜底超时完成
        // 如果使用unscaledTime，Timer也需要考虑timeScale的影响
        // 但UniRx的Timer默认不受timeScale影响，所以这里保持原样
        Observable.Timer(System.TimeSpan.FromSeconds(life + 0.1f))
            .Subscribe(_ => {
                if (tmp != null) Destroy(tmp.gameObject);
                disposable?.Dispose();
            })
            .AddTo(disposable);
    }
}