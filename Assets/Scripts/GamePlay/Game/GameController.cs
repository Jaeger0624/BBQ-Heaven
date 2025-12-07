using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour, IController, ICanSendEvent
{
    public IGameSystem gameSystem => this.GetSystem<IGameSystem>();
    [SerializeField] private Button EndDayButton;
    [SerializeField] private TextMeshProUGUI DayText;

    // 不论怎么样，都一定有个冷却时间，避免玩家疯狂点击按钮
    private float endDayButtonCoolingTime = 4f;
    private float lastEndDayButtonClickTime = 0f;
    private void Awake() {
        this.RegisterEvent<StartNewDayEvent>(OnStartNewDay).UnRegisterWhenGameObjectDestroyed(this.gameObject);
    }
    
    void OnEnable()
    {
        EndDayButton.onClick.AddListener(OnEndDayButtonClick);
    }
    void OnDisable()
    {
        EndDayButton.onClick.RemoveListener(OnEndDayButtonClick);
    }

    void Update()
    {
        UpdateButtonInteractable();
    }
    private void OnStartNewDay(StartNewDayEvent evt)
    {
        if (!evt.StageMeet(EventStage.Before)) return;
        string str = evt.day == gameSystem.MaxDay ? "最后一天" : $"还剩{gameSystem.MaxDay - evt.day}天";
        DayText.text = $"{evt.month}月{evt.day}日\n{str}";
    }

    // 结束当天，意味着进入下一个阶段
    private void OnEndDayButtonClick()
    {
        lastEndDayButtonClickTime = Time.time;

        // 推动流程到下一个状态
        this.SendEvent(new ProcessMoveNextEvent());
    }

    private void UpdateButtonInteractable()
    {
        // 不受TimeScale影响
        EndDayButton.interactable = Time.time - lastEndDayButtonClickTime > endDayButtonCoolingTime;
    }
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
}
