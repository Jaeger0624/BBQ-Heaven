using System.Collections.Generic;
using System.Text;

public class ReviewResult
{
    public int TotalStars;
    // 记录细节，方便UI展示（比如：[√] 少于3个食材 (+1.5★)）
    public List<RequirementCheckRecord> Records = new List<RequirementCheckRecord>();
    public string ReviewInfo(){
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"评价：{TotalStars/2f:F1}星");
        sb.AppendLine($"评价记录：");
        foreach (var record in Records){
            sb.AppendLine($"{record.Description}：{record.StarValue/2f:F1}星，{(record.IsMet ? "√" : "×")}");
        }
        return sb.ToString();
    }
}
public struct RequirementCheckRecord
{
    public string Description;
    public int StarValue;
    public bool IsMet;
    public RequirementAward Award;
}

public class RequirementAward
{
    public RequirementAwardType AwardType;
    public float AwardValue;
    public RequirementAward(RequirementAwardType awardType, float awardValue){
        this.AwardType = awardType;
        this.AwardValue = awardValue;
    }
}