using QFramework;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(IEntityView))]
public class EntityPosShower : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private TextMeshProUGUI posText;
    private IEntityView entityView;
    void Awake()
    {
        entityView = GetComponent<IEntityView>();

        if (SettingManager.Instance.DevSettings.showEntityPosShower)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        posText.text = entityView.Entity.position.ToString();
    }
}