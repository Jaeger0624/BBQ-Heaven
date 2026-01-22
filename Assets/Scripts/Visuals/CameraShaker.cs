using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
[Serializable]
public class ShakeInfo{
    public float amplitude = 1f;
    public Transform target;
}
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [Header("默认配置")]
    [SerializeField] private float _defaultDuration = 0.2f;
    [SerializeField] private float _defaultStrength = 1f; // 震动幅度
    [SerializeField] private int _defaultVibrato = 20;    // 震动频率（越高越急促）
    [SerializeField] private float _defaultRandomness = 90f; // 随机性

    [Header("引用")]
    public List<ShakeInfo> TargetToShake; // 拖入你的 Main Camera，或者 Canvas 的 RectTransform

    private Tweener _shakeTweenerPos;
    private Tweener _shakeTweenerRot;
    private Vector3 _originalPos;
    private Quaternion _originalRot;

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        if (TargetToShake == null) TargetToShake = new List<ShakeInfo>(){new ShakeInfo(){target = transform}};
        
        _originalPos = TargetToShake[0].target.localPosition;
        _originalRot = TargetToShake[0].target.localRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)){
            HeavyShake();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)){
            LightShake();
        }
    }

    public void ShakeAll(float strength = -1f, float duration = -1f)
    {
        foreach (var shakeInfo in TargetToShake){
            Shake(shakeInfo, strength, duration);
        }
    }

    /// <summary>
    /// 执行震动
    /// </summary>
    /// <param name="strength">强度：小震动0.5，大震动2-3</param>
    /// <param name="duration">时长：通常0.1-0.3秒</param>
    public void Shake(ShakeInfo shakeInfo, float strength = -1f, float duration = -1f)
    {
        if (strength < 0) strength = shakeInfo.amplitude;
        if (duration < 0) duration = _defaultDuration;

        // 1. 杀掉旧动画，防止叠加导致飞出宇宙
        if (_shakeTweenerPos != null) _shakeTweenerPos.Kill(true); // true = 完成并归位
        if (_shakeTweenerRot != null) _shakeTweenerRot.Kill(true);

        // 2. 归位 (双重保险)
        shakeInfo.target.localPosition = _originalPos;
        shakeInfo.target.localRotation = _originalRot;

        // 3. 位移震动 (只震动 X, Y 轴，锁定 Z 轴防止裁切问题)
        // Strength 参数传 Vector3 可以控制每个轴的力度
        _shakeTweenerPos = shakeInfo.target.DOShakePosition(duration, new Vector3(strength, strength, 0), _defaultVibrato, _defaultRandomness, true)
            .SetUpdate(true); // 忽略 TimeScale，游戏暂停也能震

        // 4. 旋转震动 (增加一点点旋转会让打击感翻倍，Z轴旋转)
        // 旋转强度通常要小一点，比如位移强度的 1/2
        float rotStrength = strength * 0.5f; 
        _shakeTweenerRot = shakeInfo.target.DOShakeRotation(duration, new Vector3(0, 0, rotStrength), _defaultVibrato, _defaultRandomness, true)
            .SetUpdate(true);

            // Debug.Log("震动");
    }
    
    // 受到重击（预设的大震动）
    [Button("重击")]
    public void HeavyShake() => ShakeAll(3f, 0.4f);
    
    // 轻微震动（UI交互反馈）
    [Button("轻微震动")]
    public void LightShake() => ShakeAll(0.5f, 0.1f);
}