public class TooltipInfo{
    // public string title;
    public string description;
    public string title = null;
    public TooltipInfo(string description){
        this.description = description;
    }
    public TooltipInfo(string title, string description){
        this.title = title;
        this.description = description;
    }

    public string GetText(){
        if (title == null){
            return description;
        }
        return $"<color=white><b>{title}</b></color>\n{description}";
    }
}