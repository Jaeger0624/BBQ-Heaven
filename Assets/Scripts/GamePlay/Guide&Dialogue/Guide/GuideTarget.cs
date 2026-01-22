using UnityEngine;
using UnityEngine.UI;
using QFramework;
using UniRx; // 核心依赖
using System;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class GuideTarget : MonoBehaviour, IController
{
    public string TargetID; // 配置ID，例如 "Btn_StartGame"

    // 对外暴露点击信号
    private Subject<Unit> _onClickSubject = new Subject<Unit>();
    public IObservable<Unit> OnClickAsObservable => _onClickSubject;

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    private void OnEnable()
    {
        // 1. 注册到 GuideSystem
        this.GetSystem<IGuideSystem>().RegisterTarget(this);

        // 2. 自动绑定点击事件 (支持 Button 和 自定义 EventTrigger)
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            // 使用 UniRx 监听按钮点击，转发给 Subject
            btn.OnClickAsObservable()
                .Subscribe(_ => {_onClickSubject.OnNext(Unit.Default); Debug.Log("GuideTarget: 点击事件触发");})
                .AddTo(this);
        }
        else
        {
            // 如果不是按钮（比如是一个物品格），你可以手动调用 OnClickSubject.OnNext
            // 或者添加 EventTrigger
        }
    }

    private void OnDisable()
    {
        var guideSys = this.GetSystem<IGuideSystem>();
        guideSys?.UnregisterTarget(this);
    }

    /// <summary>
    /// 获取世界坐标 Rect (用于传给遮罩层挖孔)
    /// </summary>
    public RectTransform GetWorldRect()
    {
        var rt = transform as RectTransform;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        
        Vector2 pos = corners[0];
        Vector2 size = new Vector2(corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        
        return this.GetComponent<RectTransform>();
    }


    [Button("测试")]
    private void Test()
    {
        GuidePanel.Instance.ShowGuideFocus(GetWorldRect(), "测试");
    }
}