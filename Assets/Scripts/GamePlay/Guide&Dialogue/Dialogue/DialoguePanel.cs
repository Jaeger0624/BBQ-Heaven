using System;
using System.Collections.Generic;
using cfg;
using QFramework;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;

public class DialoguePanel : MonoBehaviour, IController{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    [ReadOnly, ShowInInspector]
    private bool isShowing = false;
    void Update()
    {
        if (isShowing && Input.GetMouseButtonDown(0))
        {
            this.GetSystem<IDialogueSystem>().OnClickNext();
        }
    }
    void OnEnable()
    {
        this.RegisterEvent<ShowDialogueEvent>(OnShowDialogueEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<ShowDialogueEvent>(OnShowDialogueEvent);
    }
    private void OnShowDialogueEvent(ShowDialogueEvent evt)
    {
        UpdateDialogue(evt.dialogue);
    }
    public void SetIsShowing(bool isShowing){
        this.isShowing = isShowing;
    }

    private void UpdateDialogue(Dialogue dialogue){
        Debug.Log("更新对话: " + dialogue.Content);
        dialogueText.text = dialogue.Content;
        speakerNameText.text = dialogue.SpeakerName;
    }


    [Button("测试")]
    private void Test(){
        IObservable<Unit> observable = this.GetSystem<IDialogueSystem>().ShowDialogues(new List<Dialogue>{
            new Dialogue("小白", "你好，我是小白"),
            new Dialogue("小黑", "你好，我是小黑"),
            new Dialogue("小白", "你好，我是小白"),
            new Dialogue("小红", "你好，我是小红"),
        });

        observable.Subscribe(unit => {
            Debug.Log("对话结束");
        }).AddTo(this);
        
    }
}
