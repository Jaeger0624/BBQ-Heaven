    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DG.Tweening;
    using QFramework;
    using Reflex.Attributes;
    using TMPro;
    using UniRx;
    using UnityEngine;

    public interface IAnimTask
    {
        IObservable<Unit> Play();
    }

    public abstract class AbstractAnimTask : IAnimTask, ICanSendEvent, ICanGetSystem
    {
        public abstract IObservable<Unit> Play();
        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    }

    public class EmptyAnimTask : IAnimTask
    {
        public IObservable<Unit> Play()
        {
            return Observable.ReturnUnit();
        }
    }

    // 附属动画
    public class AttatchedAnimTask : AbstractAnimTask
    {
        private IAnimTask mainAnimTask;
        private List<IAnimTask> attachedAnimTasks;
        public AttatchedAnimTask(IAnimTask mainAnimTask, List<IAnimTask> attachedAnimTasks)
        {
            this.mainAnimTask = mainAnimTask;
            this.attachedAnimTasks = attachedAnimTasks;
        }
        public override IObservable<Unit> Play()
        {
            foreach (var attachedAnimTask in attachedAnimTasks)
            {
                this.GetSystem<IAnimationSystem>().DirectlyPlay(attachedAnimTask);
            }
            return mainAnimTask.Play();
        }

    }

    // 缩放动画
public class ScaleAnimationTask : IAnimTask
{
    private Transform transform;
    private float duration;
    private float scaleRatio;
    private bool noTimeScale;

    public ScaleAnimationTask(Transform transform, float scaleRatio, float duration, bool noTimeScale = false)
    {
        this.transform = transform;
        this.duration = duration;
        this.scaleRatio = scaleRatio;
        this.noTimeScale = noTimeScale;
    }

    public IObservable<Unit> Play()
    {
        float originScale = transform.localScale.x;
        float targetScale = originScale * scaleRatio;

        // 直接创建一个 DOTween Sequence
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(noTimeScale);
        
        // 添加两段动画
        seq.Append(transform.DOScale(targetScale, duration).SetLink(transform.gameObject));
        seq.Append(transform.DOScale(originScale, duration).SetLink(transform.gameObject));

        // 包装成 Task (假设你使用了我上一轮回答中修复的 TweenAnimTask)
        return new TweenAnimTask(seq).Play();
    }
}


    // 旋转动画
    public class RotateAnimationTask : IAnimTask
    {
        private Transform transform;
        private float duration;
        private bool noTimeScale;
        public RotateAnimationTask(Transform transform, float duration = 0.15f, bool noTimeScale = false)
        {
            this.transform = transform;
            this.duration = duration;
            this.noTimeScale = noTimeScale;
        }
        public IObservable<Unit> Play()
        {
            // 直接用 Sequence 保证连贯性
            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(noTimeScale);
            
            // 这种微小的旋转动效，用 Sequence 连接比两个 Task 拼接要流畅得多，不会有帧间隙
            seq.Append(transform.DOLocalRotate(new Vector3(0, 0, 10), duration).SetLink(transform.gameObject));
            seq.Append(transform.DOLocalRotate(new Vector3(0, 0, 0), duration).SetLink(transform.gameObject));

            return new TweenAnimTask(seq).Play();
        }
    }

    public class ChangeTextAnimationTask : IAnimTask
    {
        private TextMeshProUGUI text;
        private int value;
        private float duration;
        public ChangeTextAnimationTask(TextMeshProUGUI text, int value, float duration = 0.15f)
        {
            this.text = text;
            this.value = value;
            this.duration = duration;
        }
        public IObservable<Unit> Play()
        {
            // 等待duration后改变文本
            return Observable.Timer(TimeSpan.FromSeconds(duration))
                .Do(_ => { text.text = value.ToString(); })
                .AsUnitObservable();
        }
    }

    public class PlaySFXAnimationTask : IAnimTask
    {
        private string sfxName;
        private float duration;
        private IAudioService audioService => AudioManager.Instance.AudioService;
        private float randomPitch;
        private float volume = 1f;
        public PlaySFXAnimationTask(string sfxName, float duration, float randomPitch)
        {
            this.sfxName = sfxName;
            this.duration = duration;
            this.randomPitch = randomPitch;
        }
        public PlaySFXAnimationTask SetVolume(float volume)
        {
            this.volume = volume;
            return this;
        }
        public IObservable<Unit> Play()
        {
            return Observable.Timer(TimeSpan.FromSeconds(duration)).Do(_ => { audioService.Play(sfxName, volume, randomPitch: randomPitch); }).AsUnitObservable();
        }
    }


    public class SpawnTextAnimationTask : AbstractAnimTask
    {
        private string text;
        private float size;
        private (Color color, bool useColor) color;
        private Vector3 position;
        private float? lifetime;
        public SpawnTextAnimationTask(string text, float size, Color color, Vector3 position, float? lifetime = null)
        {
            this.text = text;
            this.size = size;
            this.color = (color, true);
            this.position = position;
            this.lifetime = lifetime ?? null;
        }

        public override IObservable<Unit> Play()
        {
            if (lifetime.HasValue){
                FloatingTextInfo info = new FloatingTextInfo(text, lifetime.Value, color.color, FloatingAnimType.Normal);
                FloatingTextManager.Instance.Show(position, text, color.color, info);
                return Observable.ReturnUnit();
            }
            return Observable.ReturnUnit();
        }
    }
