
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
    public CustomerSatisfaction satisfactionSystem;  // 满意度乘区实例
    public readonly int rarity; // 珍稀度
    public readonly int taste;  // 美味度
    public readonly float rawPrice;  // 原始价格
    public int price;  // 价格
	public DealResult(BBQ bbq, Customer customer, float satisfaction, CustomerSatisfaction satisfactionSystem,Dictionary<string, float> otherMultipliers, int rarity, int taste, int price, float rawPrice){
		this.bbq = bbq;
		this.customer = customer;
		this.satisfaction = satisfaction;
		this.otherMultipliers = otherMultipliers;
		this.satisfactionSystem = satisfactionSystem;
		this.rarity = rarity;
		this.taste = taste;
		this.price = price;
		this.rawPrice = rawPrice;
	}

	public string DealInfo(){
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"顾客：{customer.name}");
		sb.AppendLine($"珍稀度：{rarity}");
		sb.AppendLine($"美味度：{taste}");
		sb.AppendLine($"满意度乘区：{satisfactionSystem.GetFinal()}x");
		sb.AppendLine($"满意度乘区实例：{string.Join(", ", satisfactionSystem.GetAllMultipliers().Select(x => $"[{x.name}] {x.multiplier}x"))}");
		sb.AppendLine($"其他得分乘区：{string.Join(", ", otherMultipliers.Select(x => $"[{x.Key}] {x.Value}x"))}");
		sb.AppendLine($"最终得分：{price}");
		return sb.ToString();
	}
}
