using System;

[Serializable]
public class TextInfo{
    public string text;
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