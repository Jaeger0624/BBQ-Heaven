using QFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

public class EntityPosShower : SerializedMonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI posText;
    [OdinSerialize] public IEntityView entityView;
    void Start()
    {
        gameObject.SetActive(SettingManager.Instance.DevSettings.showEntityPosShower);
    }
    void Update()
    {
        posText.text = entityView.Entity.position.ToString();
    }
}