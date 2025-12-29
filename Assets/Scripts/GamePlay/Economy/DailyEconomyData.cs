using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 既包含统计过程，也包含结算结果
public class DailyInfo {
    // --- 过程数据 (白天积累) ---
    public int ServedCustomerCount = 0;
    public List<string> EncounterNames = new List<string>();
    // 最佳交易记录
    public DealResult BestDeal = null; 
    public int dealCount = 0;
    // 食材销量 (ID -> 数量)
    public Dictionary<string, int> FoodSales = new Dictionary<string, int>();
    public DailyEconomy DailyEconomy;
    // --- 辅助方法：记录一笔交易 ---
    public void RecordDeal(DealResult result) {
        ServedCustomerCount++;

        // 更新最佳交易
        if (BestDeal == null || result.price > BestDeal.price) {
            BestDeal = result;
        }

        // 更新销量
        foreach (var foodInstance in result.bbq.foodInstances) {
            string foodID = foodInstance.food.foodDataId;
            if (FoodSales.ContainsKey(foodID)) FoodSales[foodID]++;
            else FoodSales[foodID] = 1;
        }
    }
    // --- 辅助方法：记录遭遇 ---
    public void RecordEncounter(string name) {
        EncounterNames.Add(name);
    }
}

public class DailyEconomy{
    public int baseScore{get;} // 基础
    public int interestScore{get;} // 利息（基础最大值为5）
    public int profitScore{get;} // 利润
    public int totalScore{get;} // 总得分

    public DailyEconomy(int baseScore, int interestScore, int profitScore){
        this.baseScore = baseScore;
        this.interestScore = interestScore;
        this.profitScore = profitScore;
        this.totalScore = baseScore + interestScore + profitScore;
    }
}