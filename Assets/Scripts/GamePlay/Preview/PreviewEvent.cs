using QFramework;
public class PreviewTimeCostEvent : AbstractEvent{
    public float timeCost;
    public PreviewTimeCostEvent(float timeCost){
        this.timeCost = timeCost;
    }
}