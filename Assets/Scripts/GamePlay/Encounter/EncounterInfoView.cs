using System.Text;
using QFramework;
using TMPro;
using UnityEngine;

public class EncounterInfoView : MonoBehaviour, IController
{
    [SerializeField] private TextMeshProUGUI encounterNameText;

    void Start()
    {
        this.RegisterEvent<AddEncounterEvent>(OnAddEncounter);
        this.RegisterEvent<RemoveEncounterEvent>(OnRemoveEncounter);
    }
    void OnDestroy()
    {
        this.UnRegisterEvent<AddEncounterEvent>(OnAddEncounter);
        this.UnRegisterEvent<RemoveEncounterEvent>(OnRemoveEncounter);
    }

    private void OnAddEncounter(AddEncounterEvent evt) => UpdateEncounterName();
    private void OnRemoveEncounter(RemoveEncounterEvent evt) => UpdateEncounterName();

    private void UpdateEncounterName()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var encounter in this.GetSystem<IEncounterSystem>().ActiveEncounters)
        {
            sb.Append(encounter.encounterData.Name + "、");
        }
        encounterNameText.text = sb.ToString().TrimEnd('、');
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
