using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class TextInfo{
    [TextArea(3, 10)]
    public string text;
    [ShowIf("needResize")]
    public float size;
    
    public bool needResize;
    public TextInfo(string text){
        this.text = text;
        this.needResize = false;
    }

    public TextInfo(string text, float size){
        this.text = text;
        this.size = size;
        this.needResize = true;
    }
    public string GetText(){
        if (needResize){
            return $"<size={size}>{text}</size>";
        }
        return text;
    }
}