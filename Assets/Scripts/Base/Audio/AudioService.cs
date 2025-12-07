using UnityEngine;
using Reflex.Core;
using System.Collections.Generic;
using DG.Tweening;

public interface IAudioService
{
    void Play(string clipName, float volume = 1f, float randomPitch = 0f);
    void PlayBGM(string clipName, float fadeTime = 1f);
    void StopBGM(float fadeTime = 1f);
    void StopAll();
}
public enum AudioType
{
    SFX,
    BGM,
}
public class AudioService : IAudioService
{
    private Transform audioRoot;
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    public AudioService(){
        audioRoot = new GameObject("AudioRoot").transform;
        Object.DontDestroyOnLoad(audioRoot);

        _bgmSource = audioRoot.gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;

        _sfxSource = audioRoot.gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
    }

    private AudioClip GetClip(string clipName, AudioType audioType)
    {
        if (soundClips.TryGetValue(clipName, out var cached))
            return cached;

        var clip = Resources.Load<AudioClip>($"{audioType}/{clipName}");
        if (clip == null)
        {
            Debug.LogWarning($"【AudioService】 无法找到音频: {clipName}");
            return null;
        }

        soundClips[clipName] = clip;
        return clip;
    }
    public void Play(string clipName, float volume = 1, float randomPitch = 0)
    {
        var clip = GetClip(clipName, AudioType.SFX);
        if (clip == null) return;

        // 怎么随机pitch?
        float pitch = 1 + Random.Range(-randomPitch, randomPitch);
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(clip, volume);

        // Debug.Log($"【AudioService】 播放音效: {clipName}");
    }

    public void PlayBGM(string clipName, float fadeTime = 1)
    {
        var clip = GetClip(clipName, AudioType.BGM);
        if (clip == null) return;
        _bgmSource.clip = clip;
        _bgmSource.Play();
        _bgmSource.DOFade(1, fadeTime);

    }

    public void StopAll()
    {
        throw new System.NotImplementedException();
    }

    public void StopBGM(float fadeTime = 1)
    {
        throw new System.NotImplementedException();
    }
}
