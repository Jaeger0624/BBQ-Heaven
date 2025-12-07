using System;
using System.Collections;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManger : MonoBehaviour
{
    private static LoadingManger _instance;
    public static LoadingManger Instance => _instance;
    [SerializeField] private CanvasGroup canvasGroup;
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void LoadSceneAsync(string sceneName, Action onComplete)
    {
        canvasGroup.DOFade(1, 0.7f);
        
        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => {
            SceneManager.LoadSceneAsync(sceneName).completed += (operation) => {
                onComplete?.Invoke();
            };
        }).AddTo(this.gameObject);

        Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => {
            canvasGroup.DOFade(0, 0.7f);
        }).AddTo(this.gameObject);
    }
}
