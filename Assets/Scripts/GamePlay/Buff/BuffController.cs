using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class BuffController : MonoBehaviour, IController{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [SerializeField] private GameObject buffViewPrefab;
    [SerializeField] private Transform buffContainer;
    private Dictionary<string, BuffView> buffViews = new Dictionary<string, BuffView>();
    void Start()
    {
        this.RegisterEvent<BuffAddedEvent>(OnBuffAddedEvent).UnRegisterWhenGameObjectDestroyed(this);
        this.RegisterEvent<BuffRemovedEvent>(OnBuffRemovedEvent).UnRegisterWhenGameObjectDestroyed(this);
        this.RegisterEvent<BuffClearedEvent>(OnBuffClearedEvent).UnRegisterWhenGameObjectDestroyed(this);
    }
    private void OnBuffAddedEvent(BuffAddedEvent e){
        if (buffViews.TryGetValue(e.buff.ID, out BuffView buffView)){
            buffView.UpdateVisual();
            return;
        }
        else{
            CreateBuffView(e.buff);
        }
    }
    private void OnBuffRemovedEvent(BuffRemovedEvent e){
        if (buffViews.TryGetValue(e.buff.ID, out BuffView buffView)){
            buffView.UpdateVisual();
            return;
        }
    }
    private void OnBuffClearedEvent(BuffClearedEvent e) => ClearBuffView(e.buffID);
    private BuffView CreateBuffView(Buff buff){
        BuffView buffView = Instantiate(buffViewPrefab, buffContainer, false).GetComponent<BuffView>();
        buffView.Bind(buff);
        buffViews.Add(buff.ID, buffView);
        return buffView;
    }
    private void ClearBuffView(string buffID){
        if (buffViews.TryGetValue(buffID, out BuffView buffView)){
            Destroy(buffView.gameObject);
            buffViews.Remove(buffID);
        }
        else{
            Debug.LogWarning($"Buff {buffID} 不存在,但尝试清除");
        }
    }
}


public class BuffAddedEvent : AbstractEvent{
    public Buff buff;
    public int amount;
    public BuffAddedEvent(Buff buff, int amount){
        this.amount = amount;
        this.buff = buff;
    }
}

public class BuffRemovedEvent : AbstractEvent{
    public Buff buff;
    public int amount;
    public BuffRemovedEvent(Buff buff, int amount){
        this.amount = amount;
        this.buff = buff;
    }
}

public class BuffClearedEvent : AbstractEvent{
    public string buffID;
    public BuffClearedEvent(string buffID){
        this.buffID = buffID;
    }
}