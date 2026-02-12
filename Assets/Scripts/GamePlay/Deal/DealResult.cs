
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 交易结果 - 只读数据载体
/// </summary>
public class DealResult{
    public readonly BBQ bbq;
    public readonly Customer customer;
    public float satisfaction;  // 满意度总乘区
	public Dictionary<string, float> otherMultipliers;
    public readonly int rarity; // 珍稀度
    public readonly int taste;  // 美味度
    public readonly float rawPrice;  // 原始价格
    public int price { get; private set; }
	public List<ScoreRecord> scoreRecords;
	public DealResult(BBQ bbq, Customer customer, int rarity, int taste, float rawPrice)
	{
		this.bbq = bbq;
		this.customer = customer;
		this.otherMultipliers = new Dictionary<string, float>();
		this.rarity = rarity;
		this.taste = taste;
		this.rawPrice = rawPrice;
		this.scoreRecords = new List<ScoreRecord>();
	}
	public DealResult(BBQ bbq, Customer customer, float satisfaction, CustomerSatisfaction satisfactionSystem,Dictionary<string, float> otherMultipliers, int rarity, int taste, int price, float rawPrice, List<ScoreRecord> scoreRecords){
		this.bbq = bbq;
		this.customer = customer;
		this.satisfaction = satisfaction;
		this.otherMultipliers = otherMultipliers;
		this.rarity = rarity;
		this.taste = taste;
		this.price = price;
		this.rawPrice = rawPrice;
		this.scoreRecords = scoreRecords;
	}

	public string DealInfo(){
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"顾客：{customer.name}");
		sb.AppendLine($"珍稀度：{rarity}");
		sb.AppendLine($"美味度：{taste}");
		sb.AppendLine($"其他得分乘区：{string.Join(", ", otherMultipliers.Select(x => $"[{x.Key}] {x.Value}x"))}");
		sb.AppendLine($"最终得分：{price}");
		return sb.ToString();
	}

	public void SetPrice(int price){
		this.price = price;
	}
}

public class ScoreRecord{
	public string name;
	public float multiplier;
	public float currentScore;
	public ScoreRecord(string name, float multiplier, float currentScore){
		this.name = name;
		this.multiplier = multiplier;
		this.currentScore = currentScore;
	}
}
