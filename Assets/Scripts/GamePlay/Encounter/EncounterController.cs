using System;
using System.Text;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;

public class EncounterController : MonoBehaviour, IController, ICanSendEvent
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(DebugInfo());
        }
    }

    void OnEnable()
    {
        this.RegisterEvent<AddEncounterEvent>(OnAddEncounter);
        this.RegisterEvent<RemoveEncounterEvent>(OnRemoveEncounter);
    }

    void OnDisable()
    {
        
        this.UnRegisterEvent<AddEncounterEvent>(OnAddEncounter);
        this.UnRegisterEvent<RemoveEncounterEvent>(OnRemoveEncounter);
    }

    private void OnAddEncounter(AddEncounterEvent evt)
    {
        Debug.Log($"添加事件: {evt.activeEncounter.encounterData.ID}");
        nameText.text = evt.activeEncounter.encounterData.Name;
        descriptionText.text = evt.activeEncounter.encounterData.Description;

        this.SendEvent(new UIPanelEvent(UIPanelType.EncounterPanel, UIPanelAction.Show));

        // 3s（unscaledTime）后
        Observable.Timer(TimeSpan.FromSeconds(3), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ => {
            this.SendEvent(new UIPanelEvent(UIPanelType.EncounterPanel, UIPanelAction.Hide));
        });
    }

    private void OnRemoveEncounter(RemoveEncounterEvent evt)
    {
        Debug.Log($"移除事件: {evt.activeEncounter.encounterData.ID}");
        nameText.text = "";
        descriptionText.text = "";
    }


    private string DebugInfo(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("当前活跃事件:");
        foreach (var encounter in this.GetSystem<IEncounterSystem>().ActiveEncounters)
        {
            sb.AppendLine($"- 事件名称: {encounter.encounterData.Name}, 事件类型: {encounter.encounterData.Type}, 事件进度: {encounter.currentNum}/{encounter.totalNum}");
        }
        return sb.ToString();
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }


    [Button]
    private void TestAddEncounter(){
        this.GetSystem<IEncounterSystem>().StartEncounter("1");
    }
}
