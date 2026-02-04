using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using QFramework;
using UniRx;
using UnityEngine;

public interface IAnimationSystem : ISystem{
    Dictionary<AnimQueue, LinkedList<IAnimTask>> Queue { get; }
    Dictionary<AnimQueue, bool> IsPlaying { get; }
    /// <summary>
    /// 添加动画任务（用于构建序列）
    /// </summary>
    IAnimationSystem Append(IAnimTask task, AnimQueue key = AnimQueue.Default);

    /// <summary>
    /// 添加与上一个动画并行的任务（类似DOTween的Join）
    /// </summary>
    IAnimationSystem Join(IAnimTask task, AnimQueue key = AnimQueue.Default);

    /// <summary>
    /// 添加延迟
    /// </summary>
    IAnimationSystem AddDelay(float delay, bool useUnscaledTime, AnimQueue key = AnimQueue.Default);
    
    /// <summary>
    /// 播放序列中的所有任务
    /// </summary>
    IObservable<Unit> PlaySequence(AnimQueue key = AnimQueue.Default);

    IAnimationSystem AddFirst(IAnimTask task, AnimQueue key = AnimQueue.Default);

    void Play(AnimQueue key = AnimQueue.Default);
    void DirectlyPlay(IAnimTask task);
    IObservable<Unit> PlayTaskAsync(IAnimTask task);

    void Stop(AnimQueue key = AnimQueue.Default);
    
    /// <summary>
    /// 清空序列
    /// </summary>
    void Clear();
}

public enum AnimQueue{
    Default,
    Customer,
}


public class AnimationSystem : AbstractSystem, IAnimationSystem
{
    // 使用双端队列，方便在队列头部添加任务

    private readonly Dictionary<AnimQueue, LinkedList<IAnimTask>> _queue = new Dictionary<AnimQueue, LinkedList<IAnimTask>>();
    public Dictionary<AnimQueue, LinkedList<IAnimTask>> Queue => _queue;
    private Dictionary<AnimQueue, bool> _isPlaying = new Dictionary<AnimQueue, bool>();
    public Dictionary<AnimQueue, bool> IsPlaying => _isPlaying;

    protected override void OnInit()
    {
        _isPlaying.Clear();
        _queue.Clear();
        _isPlaying.Add(AnimQueue.Default, false);
        _queue.Add(AnimQueue.Default, new LinkedList<IAnimTask>());

        this.RegisterEvent<TriggerAnimEvent>(OnTriggerAnimEvent);
    }

    protected override void OnDeinit()
    {
        Clear();
        _isPlaying.Clear();
        _queue.Clear();

        this.UnRegisterEvent<TriggerAnimEvent>(OnTriggerAnimEvent);
    }
    
    private void OnTriggerAnimEvent(TriggerAnimEvent e)
    {
        if (!_isPlaying[e.animQueue]) {LogKit.E($"【AnimationSystem】动画通道 {e.animQueue} 未播放"); return;};

        Observable.Timer(TimeSpan.FromSeconds(e.WaitTime)).Subscribe(_ => {
            // PlayNext
            PlayNext(e.animQueue).Subscribe().AddTo(SettingManager.Instance.gameObject);
        }).AddTo(SettingManager.Instance.gameObject);
    }
    
    private LinkedList<IAnimTask> GetQueue(AnimQueue key = AnimQueue.Default)
    {
        if (!_queue.ContainsKey(key))
        {
            _queue[key] = new LinkedList<IAnimTask>();
            _isPlaying[key] = false;
            
        }
        return _queue[key];
    }

    public IAnimationSystem Append(IAnimTask task, AnimQueue key = AnimQueue.Default)
    {
        if (task != null)
        {
            GetQueue(key).AddLast(task);
        }
        return this;
    }
    
    public IAnimationSystem Join(IAnimTask task, AnimQueue key = AnimQueue.Default)
    {
        if (task != null && GetQueue(key).Count > 0)
        {
            // 取出队列尾部的任务
            var lastTask = GetQueue(key).Last.Value;
            GetQueue(key).RemoveLast();
            
            // 创建包含两个任务的并行任务
            var parallelTask = new ParallelAnimTask(new List<IAnimTask> { lastTask, task });
            
            // 将并行任务放回队列
            GetQueue(key).AddLast(parallelTask);
        }
        else if (_queue.Count == 0)
        {
            GetQueue(key).AddLast(task);
        }
        return this;
    }

    public IAnimationSystem AddFirst(IAnimTask task, AnimQueue key = AnimQueue.Default)
    {
        GetQueue(key).AddFirst(task);
        return this;
    }

    public IAnimationSystem AddDelay(float seconds, bool useUnscaledTime, AnimQueue key = AnimQueue.Default)
    {
        GetQueue(key).AddLast(new DelayAnimTask(seconds, useUnscaledTime));
        return this;
    }

    public void Play(AnimQueue key = AnimQueue.Default)
    {

        //TODO: 技术债，需要一个全局的管理器来管理所有动画（现在只是因为只有SettingManager一个全局MonoBehaviour）
        PlaySequence(key).Subscribe().AddTo(SettingManager.Instance.gameObject);
    }

    public void DirectlyPlay(IAnimTask task)
    {
        //TODO: 技术债，需要一个全局的管理器来管理所有动画（现在只是因为只有SettingManager一个全局MonoBehaviour）
        task.Play().Subscribe().AddTo(SettingManager.Instance.gameObject);
    }

    public IObservable<Unit> PlayTaskAsync(IAnimTask task)
    {
        if (task == null) return Observable.ReturnUnit();
        return task.Play();
    }

    /// <summary>
    /// 播放序列中的所有任务
    /// </summary>
    public IObservable<Unit> PlaySequence(AnimQueue key = AnimQueue.Default)
    {
        LinkedList<IAnimTask> queue = GetQueue(key);
        if (queue.Count == 0) return Observable.ReturnUnit();
        if (_isPlaying[key]) return Observable.ReturnUnit();
        _isPlaying[key] = true;
        
        // 直接返回播放Observable
        return PlayNext(key)
            .DoOnCompleted(() => 
            {
                _isPlaying[key] = false;
            })
            .DoOnError(ex =>
            {
                LogKit.E($"AnimationSystem error: {ex}");
                PlayNext(key).Subscribe();
            });
    }

    public void Clear()
    {
        foreach (var queue in _queue)
        {
            queue.Value.Clear();
        }
        _queue.Clear();
        _isPlaying.Clear();
    }

    private IObservable<Unit> PlayNext(AnimQueue key = AnimQueue.Default)
    {
        // 如果没有任务了，返回完成
        if (GetQueue(key).Count == 0){
            _isPlaying[key] = false;
            return Observable.ReturnUnit();
        }

        var task = GetQueue(key).First.Value;
        GetQueue(key).RemoveFirst();

        _isPlaying[key] = true;
        // 执行当前任务，完成后执行下一个
        return task.Play()
            .ContinueWith(_ => PlayNext(key));
    }

    public void Stop(AnimQueue key = AnimQueue.Default)
    {
        Append(new EventTriggerAnimTask(), key);
    }
}


/// <summary>
/// 并行动画
/// </summary>
public class ParallelAnimTask : IAnimTask
{
    private readonly List<IAnimTask> _tasks = new();

    public ParallelAnimTask(List<IAnimTask> tasks)
    {
        _tasks.AddRange(tasks);
    }

    public IObservable<Unit> Play()
    {
        if (_tasks.Count == 0)
            return Observable.ReturnUnit();
        // 所有动画并行执行
        return _tasks
            .Select(t => t.Play())
            .WhenAll()
            .AsUnitObservable();
    }
}

public class SequenceAnimTask : IAnimTask
{
    private readonly List<IAnimTask> _tasks = new();
    public SequenceAnimTask(List<IAnimTask> tasks)
    {
        _tasks.AddRange(tasks);
    }
    public IObservable<Unit> Play()
    {
    if (_tasks.Count == 0)
        return Observable.ReturnUnit();
        // 所有动画顺序执行
        return _tasks
        .Aggregate(Observable.ReturnUnit(), (acc, t) => acc.ContinueWith(_ => t.Play()))
        .AsUnitObservable();
    }
}



/// <summary>
/// 延迟动画
/// </summary>
public class DelayAnimTask : IAnimTask
{
    private readonly float _delay;
    private readonly bool _useUnscaledTime;

    public DelayAnimTask(float delay, bool useUnscaledTime = false)
    {
        _delay = delay;
        _useUnscaledTime = useUnscaledTime;
    }

    public IObservable<Unit> Play()
    {
        // 核心修复：
        // 不要手动计算 timeScale。使用正确的 Scheduler。
        // MainThread = 对应 Time.time (受 Scale 和 帧率 影响)
        // MainThreadIgnoreTimeScale = 对应 Time.unscaledTime
        var scheduler = _useUnscaledTime 
            ? Scheduler.MainThreadIgnoreTimeScale 
            : Scheduler.MainThread;

        return Observable.Timer(TimeSpan.FromSeconds(_delay), scheduler)
            .AsUnitObservable();
    }
}

public class TweenAnimTask : IAnimTask
{
    private readonly Tween _tween;
    private readonly bool _autoKill;

    public TweenAnimTask(Tween tween, bool autoKill = true)
    {
        _tween = tween;
        _autoKill = autoKill;
        // 暂停以等待Play调用，防止在创建时就开始跑
        tween.Pause(); 
        // 通常建议让DOTween管理回收，但在Task系统中我们可能需要手动控制
        tween.SetAutoKill(autoKill);
    }

    public IObservable<Unit> Play()
    {
        return Observable.Create<Unit>(observer =>
        {
            // 1. 检查 Tween 有效性
            if (_tween == null || !_tween.active)
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
                return Disposable.Empty;
            }

            // 2. 绑定完成回调
            // 如果Tween已经有回调，这里实际上是追加，DOTween支持多播链式回调
            _tween.OnComplete(() =>
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            });

            // 3. 绑定被杀回调（防止外部Kill导致卡死）
            _tween.OnKill(() =>
            {
                // 如果OnKill触发时，Observable还没完成，说明是被意外Kill的
                // 这种情况下通常也应该视为任务结束，或者你可以选择报错
                 if (_tween != null && !_tween.IsComplete()) 
                 {
                     observer.OnNext(Unit.Default);
                     observer.OnCompleted();
                 }
            });

            // 4. 开始播放
            _tween.Play();

            // 5. 处理 Dispose（当外部取消订阅时，停止动画）
            return Disposable.Create(() =>
            {
                if (_tween != null && _tween.active)
                {
                    _tween.Kill();
                }
            });
        });
    }
}
public class ActionAnimTask : IAnimTask
{
    private readonly Action _action;
    public ActionAnimTask(Action action)
    {
        _action = action;
    }
    public IObservable<Unit> Play()
    {
        _action?.Invoke();
        return Observable.AsUnitObservable(Observable.ReturnUnit());
    }
}


# region 占位符动画

// 空动画，用于等待事件呼叫推进动画，永远不会完成，需要配套事件来触发完成
public class EventTriggerAnimTask : IAnimTask
{
    public IObservable<Unit> Play()
    {
        return Observable.Never<Unit>();
    }
}

// 需要触发哪一个通道的动画
public class TriggerAnimEvent : AbstractEvent{
    public AnimQueue animQueue;
    public float WaitTime = 0f;
    public TriggerAnimEvent(float waitTime,AnimQueue animQueue = AnimQueue.Default)
    {
        this.animQueue = animQueue;
        this.WaitTime = waitTime;
    }
}

#endregion