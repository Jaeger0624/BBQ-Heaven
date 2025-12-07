using UnityEngine;

public class ScheduleInfo{
    public Customer customer; // null 代表不指定顾客
    public float arriveTime; // -1 代表不指定到达时间, 0 代表立刻到达
    public ScheduleInfo(Customer customer, float arriveTime){
        if (customer == null && arriveTime == -1){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是顾客和到达时间都为空");
            
            return;
        }
        if (arriveTime > 1){
            Debug.LogWarning("预定顾客时，尝试预定一个顾客，但是到达时间大于1");
            return;
        }
        this.customer = customer;
        this.arriveTime = arriveTime;
    }
}