using System;

[Serializable]
public class TimeInfo{
    private readonly int originalMinute;
    private readonly int originalHour;
    public int minute;
    public int hour;
    public int targetMinute;
    public int targetHour;
    private bool nextDay = false;
    public TimeInfo(int minute, int hour){
        this.originalMinute = minute;
        this.originalHour = hour;
        this.minute = minute;
        this.hour = hour;
        this.nextDay = false;
    }
    public int GetTotalTimePoint(){
        int totalMinute = hour * 60 + minute;
        if (nextDay){
            totalMinute += 24 * 60;
        }
        return totalMinute;
    }
    public void OnAddTimePoint(int timePoint){
        minute += timePoint;
        if (minute >= 60){
            hour++;
            minute -= 60;
        }
        if (hour >= 24){
            nextDay = true;
            hour -= 24;
        }
    }

    public TimeInfo GetOriginalTimeInfo(){
        return new TimeInfo(originalMinute, originalHour);
    }

    public TimeInfo Clone(int timeOffset){
        TimeInfo res = new TimeInfo(originalMinute, originalHour);
        // 加到当前时间点
        res.OnAddTimePoint(GetTotalTimePoint()-GetOriginalTimeInfo().GetTotalTimePoint());
        // 加到目标时间点
        res.OnAddTimePoint(timeOffset);
        return res;
    }
}