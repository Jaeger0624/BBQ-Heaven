using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioReferenceSO", menuName = "AudioReferenceSO")]
public class AudioReferenceSO : SerializedScriptableObject
{
    [OdinSerialize]
    public Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    [OdinSerialize]
    public Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
    public AudioClip GetBGMClip(string name){
        if (!bgmClips.TryGetValue(name, out var clip)){
            Debug.LogError($"AudioReferenceSO: 无法找到音频: {name}");
            return null;
        }
        return clip;
    }
    public AudioClip GetSFXClip(string name){
        if (!sfxClips.TryGetValue(name, out var clip)){
            Debug.LogError($"AudioReferenceSO: 无法找到音频: {name}");
            return null;
        }
        return clip;
    }

    [Button("加载SFX")]
    public void LoadSFXClips(){
        string sfxPath = "SFX";
        sfxClips = new Dictionary<string, AudioClip>();
        AudioClip[] allSFXClips = Resources.LoadAll<AudioClip>(sfxPath);
        foreach (AudioClip sfxClip in allSFXClips){
            sfxClips.Add(sfxClip.name, sfxClip);
        }   
    }
    [Button("加载BGM")]
    public void LoadBGMClips(){
        string bgmPath = "BGM";
        bgmClips = new Dictionary<string, AudioClip>();
        AudioClip[] allBGMClips = Resources.LoadAll<AudioClip>("BGM");
        foreach (AudioClip bgmClip in allBGMClips){
            bgmClips.Add(bgmClip.name, bgmClip);
        }
    }
}
