public class TimeSystemInfo{
    public TimeInfo targetTime;
    public TimeInfo currentTime;
    public int totalTimePoint;
    public TimeSystemInfo(TimeInfo targetTime, TimeInfo currentTime){
        this.targetTime = targetTime;
        this.currentTime = currentTime;
        this.totalTimePoint = targetTime.GetTotalTimePoint() - currentTime.GetTotalTimePoint();
    }

    public TimeSystemInfo(int totalTimePoint){
        this.totalTimePoint = totalTimePoint;
        this.targetTime = new TimeInfo(totalTimePoint % 60, totalTimePoint / 60);
        this.currentTime = new TimeInfo(0, 0);
    }
}