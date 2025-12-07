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
        this.RegisterEvent<FinishCombineBBQEvent>(OnEndCalculateBBQ);
        this.RegisterEvent<AddBBQToRepositoryEvent>(OnAddBBQToRepository);
        this.RegisterEvent<AddBBQPropertyAnimEvent>(OnAddBBQProperty);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<CombineBBQEvent>(OnCombineBBQ);
        this.UnRegisterEvent<FinishCombineBBQEvent>(OnEndCalculateBBQ);
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

        // Debug.Log($"【BBQProcessUI】{evt.adderName} OnAddBBQProperty: {evt.totalRarity} {evt.totalTaste} {evt.addRarity} {evt.addTaste}");

        List<IAnimTask> tasks = new List<IAnimTask>{
            new TextChangeAnimTask(_rarityText, evt.totalRarity, 0.15f),
            new TextChangeAnimTask(_tasteText, evt.totalTaste, 0.15f),
            new PlaySFXAnimationTask("Score 3", 0.01f, 0.1f),
        };
        
        List<IAnimTask> sequenceTasks = new List<IAnimTask>();
        if (evt.addRarity != 0) sequenceTasks.Add(new SpawnTextAnimationTask(addRarityText, 8,
        SettingManager.Instance.DevSettings.AddRarityTextColor, _rarityAddTextParent.position));
        if (evt.addTaste != 0) sequenceTasks.Add(new SpawnTextAnimationTask(addTasteText, 8,
        SettingManager.Instance.DevSettings.AddTasteTextColor, _tasteAddTextParent.position));

        IAnimTask sequence = new SequenceAnimTask(new List<IAnimTask>{
            new DelayAnimTask(0.15f),
            new ParallelAnimTask(sequenceTasks),
        });
        tasks.Add(sequence);
        IAnimTask final = new ParallelAnimTask(tasks);
        this.GetSystem<IAnimationSystem>().DirectlyPlay(final);
    }

    // 程序层面结束计算的事件
    private void OnEndCalculateBBQ(FinishCombineBBQEvent evt)
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