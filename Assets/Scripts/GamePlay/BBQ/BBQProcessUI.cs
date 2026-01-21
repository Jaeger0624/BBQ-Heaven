using System;
using QFramework;
using TMPro;
using UniRx;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Reflex.Attributes;
using cfg;
public class BBQProcessUI : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI _rarityText;
    [SerializeField] private TextMeshProUGUI _tasteText;
    [SerializeField] private Transform _rarityAddTextParent;
    [SerializeField] private Transform _tasteAddTextParent;
    [Inject]
    IAudioService audioService;
    IBBQSystem bbqSystem => this.GetSystem<IBBQSystem>();
    void OnEnable()
    {
        this.RegisterEvent<CombineBBQEvent>(OnCombineBBQ);   
        this.RegisterEvent<FinishCombineBBQEvent_动画>(OnEndCalculateBBQ);
        this.RegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.RegisterEvent<AddBBQPropertyAnimEvent>(OnAddBBQProperty);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CombineBBQEvent>(OnCombineBBQ);
        this.UnRegisterEvent<FinishCombineBBQEvent_动画>(OnEndCalculateBBQ);
        this.UnRegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.UnRegisterEvent<AddBBQPropertyAnimEvent>(OnAddBBQProperty);
    }
    private void OnCombineBBQ(CombineBBQEvent evt)
    {
        // 获取珍稀度属性
        ReactiveProperty<int> totalRarity = bbqSystem.GetCurrentBBQ().totalRarity;
        // 获取美味度属性
        ReactiveProperty<int> totalTaste = bbqSystem.GetCurrentBBQ().totalTaste;

        // 设置初始值        
        _rarityText.text = totalRarity.Value.ToString();
        _tasteText.text = totalTaste.Value.ToString();

        // 设置可见性
        _rarityText.gameObject.SetActive(true);
        _tasteText.gameObject.SetActive(true);
    }

    private void OnAddBBQProperty(AddBBQPropertyAnimEvent evt)
    {
        string addRarityText = evt.addRarity>0?"+":"";
        string addTasteText = evt.addTaste>0?"+":"";
        addRarityText += evt.addRarity.ToString();
        addTasteText += evt.addTaste.ToString();

        if (evt.isCritical){
            addRarityText += "\n暴击！";
            addTasteText += "\n暴击！";
        }

        List<IAnimTask> sequenceTasks = new List<IAnimTask>();
        if (evt.addRarity != 0){
            OnGenerateFloatingText_珍稀度(addRarityText, () => {
                this.GetSystem<IAnimationSystem>().DirectlyPlay(new TextChangeAnimTask(_rarityText, evt.totalRarity, 0.15f));
            });
        } 
        if (evt.addTaste != 0){
            OnGenerateFloatingText_美味度(addTasteText, () => {
                this.GetSystem<IAnimationSystem>().DirectlyPlay(new TextChangeAnimTask(_tasteText, evt.totalTaste, 0.15f));
            });
        }

        this.GetSystem<IAnimationSystem>().DirectlyPlay(new PlaySFXAnimationTask("Score 3", 0.01f, 0.1f));
    }

    // 程序层面结束计算的事件
    private void OnEndCalculateBBQ(FinishCombineBBQEvent_动画 evt)
    {
    }

    // 将烧烤实例添加到烧烤仓库中（UI动画）
    private void OnAddBBQToRepository(AddBBQToRepositoryEvent evt)
    {
        _rarityText.gameObject.SetActive(false);
        _tasteText.gameObject.SetActive(false);
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
    private void OnGenerateFloatingText_美味度(string content,System.Action onArrive = null){
        Color color = SettingManager.Instance.DevSettings.AddTasteTextColor;
        string text = content.ToSize(70f);
        FloatingTextManager.Instance.GenerateFloatingText_美味度(text, _tasteAddTextParent, color, onArrive);
    }
    private void OnGenerateFloatingText_珍稀度(string content,System.Action onArrive = null){
        Color color = SettingManager.Instance.DevSettings.AddRarityTextColor;
        string text = content.ToSize(70f);
        FloatingTextManager.Instance.GenerateFloatingText_珍稀度(text, _rarityAddTextParent, color, onArrive);
    }
}

#region 动画
public class TextChangeAnimTask : IAnimTask
{
    private TextMeshProUGUI text;
    private int value;
    private float duration;
    public TextChangeAnimTask(TextMeshProUGUI text, int value, float duration = 0.15f)
    {
        this.text = text;
        this.value = value;
        this.duration = duration;
    }
    public IObservable<Unit> Play()
    {
        ParallelAnimTask parallelAnimTask = new ParallelAnimTask(new List<IAnimTask>{
            new ScaleAnimationTask(text.transform, 1.8f, duration),
            new RotateAnimationTask(text.transform, duration),
            new ChangeTextAnimationTask(text, value, 0.02f),
        });
        return parallelAnimTask.Play();
    }


}
#endregion