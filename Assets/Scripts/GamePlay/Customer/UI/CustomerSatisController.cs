using System;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
public class CustomerSatisController : MonoBehaviour, IController{
    [SerializeField] private TextMeshProUGUI customerSatisText;
    [SerializeField] private List<Image> statisBars;
    [SerializeField] private TagView tagView;
    [SerializeField] private CanvasGroup canvasGroup;
    private List<float> statisThresholds => SettingManager.GetSetting<GameplaySettings>().默认顾客满意度区间;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    private ReactiveProperty<float> currentValue = new ReactiveProperty<float>(1f);
    private bool isFirstSecond = true;
    private bool isFirstThird = true;
    [SerializeField] private Transform barParent;
    void Start()
    {
        Hide();
        currentValue.Subscribe(value => {
            UpdateStatisBars(value);
        });

        if (statisBars.Count != statisThresholds.Count || statisBars.Count == 0){
            Debug.LogError("【CustomerSatisController】满意度条和阈值数量不匹配");
            return;
        }
    }
    void OnEnable()
    {
        // 1. 注册更新满意度事件
        this.RegisterEvent<UpdateStatisEvent>(OnUpdateStatisEvent);

        // 2. 注册隐藏满意度条事件
        this.RegisterEvent<HideStatisBarEvent>(OnHideStatisBarEvent);

        // 3. 注册显示满意度条事件
        this.RegisterEvent<ShowStatisBarEvent>(OnShowStatisBarEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<UpdateStatisEvent>(OnUpdateStatisEvent);
        this.UnRegisterEvent<HideStatisBarEvent>(OnHideStatisBarEvent);
        this.UnRegisterEvent<ShowStatisBarEvent>(OnShowStatisBarEvent);
    }
    private void OnUpdateStatisEvent(UpdateStatisEvent evt) => UpdateCustomerSatis(evt.name, evt.satis, evt.multiplier, evt.previewResults, evt.animTarget, evt.hasReset);
    private void OnHideStatisBarEvent(HideStatisBarEvent evt){
        ResetStatisBars(true);
        Hide();
    }
    private void OnShowStatisBarEvent(ShowStatisBarEvent evt){
        ResetStatisBars(false);
        Show();
    }
    public void UpdateCustomerSatis(string name, float satis, float multiplier, List<bool> previewResults, Transform animTarget, bool hasReset){

        if (hasReset) ResetStatisBars(true);
        Transform targetTransform = animTarget; // 默认为传入的 animTarget (可能为空)

        // 1. 若有预览结果，则根据预览结果更新目标Transform
        if (previewResults != null){
            int index = previewResults.FindIndex(x => x);
            if (index != -1){
                // 这里调用 TagView
                var tf = tagView.GetTagViewTransform(index);
                // 只有当 TagView 返回的不是 null 时才覆盖
                if (tf != null) targetTransform = tf;
            }
        }
        
        // 2. 兜底逻辑：如果上面的逻辑跑完 targetTransform 还是 null
        if (targetTransform == null)
        {
             // 最后的防线：飞向文本父物体
             targetTransform = SettingManager.Instance.SatisfactionTextParent;
        }

        // 跳字动画
        IAnimTask animTask_跳字 = AnimCombine_顾客Tag.Anim_Tag触发_满意度乘区文本(multiplier, name, targetTransform);

        // 取小数点后两位
        satis = Mathf.Round(satis * 100) / 100;
        customerSatisText.text = $"{satis}x";
        
        float value = RevisedValue(satis);

        IAnimTask animTask_进度条 = UpdateStatisBarAnimTask(value, 0.5f);

        IAnimTask animTask_组合 = new SequenceAnimTask(new List<IAnimTask>{
            animTask_跳字,
            animTask_进度条,
        });
        
        this.GetSystem<IAnimationSystem>().DirectlyPlay(animTask_组合);

    }

    private void UpdateStatisBars(float value){
        if (value < 1f){
            statisBars[0].fillAmount = value;
            statisBars[1].fillAmount = 0f;
            statisBars[2].fillAmount = 0f;
        }else if (value < 2f){
            statisBars[0].fillAmount = 1f;
            statisBars[1].fillAmount = value - 1f;
            statisBars[2].fillAmount = 0f;
        }else if (value < 3f){
            statisBars[0].fillAmount = 1f;
            statisBars[1].fillAmount = 1f;
            statisBars[2].fillAmount = value - 2f;
        }
        else{
            statisBars[0].fillAmount = 1f;
            statisBars[1].fillAmount = 1f;
            statisBars[2].fillAmount = 1f;
        }
    }

    private float RevisedValue(float satis){
        float baseValue = 1f;

        for (int i = 0; i < statisThresholds.Count; i++)
        {
            if (satis < statisThresholds[i]){
                if (i == 0){
                    return baseValue + (satis - 1f) / (statisThresholds[i] - 1f);
                }
                return baseValue + (satis - statisThresholds[i-1]) / (statisThresholds[i] - statisThresholds[i-1]);
            }
            else{
                baseValue += 1f;
            }
        }
        return baseValue;
    }
    public IAnimTask UpdateStatisBarAnimTask(float value, float duration){
        // 将currentValue设置为value
        IAnimTask animTask = new TweenAnimTask(DOTween.To(() => currentValue.Value, x => currentValue.Value = x, value, duration).SetEase(Ease.OutSine).SetUpdate(true));
        
        // 第一次RevisedValue超过2f/3f时，需触发震动动画
        isFirstSecond = true;
        isFirstThird = true;
        
        IDisposable disposable = currentValue.Subscribe(x => {
            if (isFirstSecond && x >= 2f){
                isFirstSecond = false;
                ShakeBarParent(0.12f);
            }
            if (isFirstThird && x >= 3f){
                isFirstThird = false;
                ShakeBarParent(0.12f);
            }
        });
        return animTask;
    }

    private void ShakeBarParent(float duration){
        barParent.DOScale(1.5f, duration).SetEase(Ease.OutSine).UnScaledKill(barParent.gameObject).OnComplete(() => {
            barParent.DOScale(1f, duration).SetEase(Ease.OutSine).UnScaledKill(barParent.gameObject);
        });
        barParent.DOLocalRotate(new Vector3(0, 0, 10), duration).SetEase(Ease.OutSine).UnScaledKill(barParent.gameObject).OnComplete(() => {
            barParent.DOLocalRotate(new Vector3(0, 0, 0), duration).SetEase(Ease.OutSine).UnScaledKill(barParent.gameObject);
        });
    }

    private void ResetStatisBars(bool isDirectly = false){
        if (isDirectly){
            currentValue.Value = 1f;
            // 更新文字
            customerSatisText.text = "1x";
            return;
        }
        this.GetSystem<IAnimationSystem>().DirectlyPlay(UpdateStatisBarAnimTask(1f, 0.1f));
    }

    private void Show(){
        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutSine).UnScaledKill(canvasGroup.gameObject);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Hide(){
        canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutSine).UnScaledKill(canvasGroup.gameObject);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}


public class UpdateStatisEvent : AbstractEvent, ICanGetSystem{
    public string name = "未知";
    public float satis;
    public float multiplier;
    public List<bool> previewResults = null;
    public bool hasReset;
    public Transform animTarget = SettingManager.Instance.SatisfactionTextParent;
    public UpdateStatisEvent(float multiplier, List<bool> previewResults, Transform animTarget = null, bool hasReset = true){
        this.satis = this.GetSystem<ICustomerSystem>().Satisfaction.GetFinal();
        this.hasReset = hasReset;
        this.multiplier = multiplier;
        this.previewResults = previewResults;
        this.animTarget = animTarget ?? SettingManager.Instance.SatisfactionTextParent;
    }

    public UpdateStatisEvent SetName(string name){
        this.name = name;
        return this;
    }


    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}

public class ShowStatisBarEvent : AbstractEvent{
    public float satis;
}

public class HideStatisBarEvent : AbstractEvent{}
