using System;
using System.Collections.Generic;
using System.Linq;
using cfg;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class ProbabilityInfo{
    [OdinSerialize]
    public Dictionary<Rank, float> probabilities;
    public ProbabilityInfo(){
        probabilities = new Dictionary<Rank, float>();
        // 找到第一级声望的稀有度概率
        ReputationData reputationData = GameArchitecture.Interface.GetSystem<IDataSystem>().GetAllReputationData().FirstOrDefault(x => x.Rank == 1);

        List<GameAction> gameActions = reputationData.Actions.SelectMany(x => x.Actions).ToList();
        GA_修改稀有度概率 ga_修改稀有度概率 = gameActions.FirstOrDefault(x => x is GA_修改稀有度概率) as GA_修改稀有度概率;
        if (ga_修改稀有度概率 != null){
            probabilities.Add(Rank.普通, ga_修改稀有度概率.Probs.Probability普通);
            probabilities.Add(Rank.稀有, ga_修改稀有度概率.Probs.Probability稀有);
            probabilities.Add(Rank.史诗, ga_修改稀有度概率.Probs.Probability史诗);
            probabilities.Add(Rank.传说, ga_修改稀有度概率.Probs.Probability传说);
        }
        else{
            Debug.LogError("第一级声望没有稀有度概率");
        }
    }
    public ProbabilityInfo(Dictionary<Rank, float> probabilities){
        this.probabilities = probabilities;
    }
    public float GetProbability(Rank rank){
        // 加权随机
        float totalProbability = probabilities.Values.Sum();
        return probabilities[rank] / totalProbability;
    }
    
}