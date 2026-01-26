using System.Collections.Generic;
using System.Linq;
using QFramework;

public class RequirementBuilder : ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

    // 星数范围配置
    private float minStarAmount = 6;
    private float maxStarAmount = 9;
    
    // 要求数量范围配置
    private int minRequirementCount = 2;
    private int maxRequirementCount = 3;
    
    // 最大重试次数，避免死循环
    private int maxRetryCount = 100;
    private List<CustomerRequirement> requirementsTemplates = new List<CustomerRequirement>(){
        new Requirement_是否包含食材种类(),

        new Requirement_食材少于(),
        // new Requirement_食材多于(),
        new Requirement_食材范围(),
        
        new Requirement_具体食材站位(),

        new Requirement_食材连续(),
    };

    /// <summary>
    /// 生成符合规则的要求组
    /// </summary>
    public List<CustomerRequirement> GenerateGroup(Customer customer){
        Rng rng = this.GetSystem<IRngSystem>().GetSubRng<ICustomerSystem>();
        
        int retryCount = 0;
        while (retryCount < maxRetryCount){
            List<CustomerRequirement> result = TryGenerateGroup(customer, rng);
            
            if (result != null){
                return result;
            }
            
            retryCount++;
        }
        
        // 如果重试多次仍无法生成，返回一个基础的要求组（放宽限制）
        UnityEngine.Debug.LogWarning($"【RequirementBuilder】无法在{maxRetryCount}次尝试内生成符合规则的要求组，返回基础要求组");
        return GenerateFallbackGroup(customer, rng);
    }

    /// <summary>
    /// 尝试生成一组要求
    /// </summary>
    private List<CustomerRequirement> TryGenerateGroup(Customer customer, Rng rng){
        List<CustomerRequirement> result = new List<CustomerRequirement>();
        
        // 随机决定要生成的要求数量
        int targetCount = rng.NextInt(minRequirementCount, maxRequirementCount + 1);
        
        // 生成指定数量的要求
        for (int i = 0; i < targetCount; i++){
            CustomerRequirement requirement = TryAddRequirement(result, customer, rng);
            
            if (requirement == null){
                // 无法添加新要求，生成失败
                return null;
            }
            
            result.Add(requirement);
        }
        
        // 检查总星数是否在范围内
        int totalStars = result.Sum(req => req.StarAmount);
        if (!CheckStarAmount(totalStars)){
            return null;
        }
        
        return result;
    }

    /// <summary>
    /// 尝试添加一个新要求，确保不与已有要求冲突
    /// </summary>
    private CustomerRequirement TryAddRequirement(List<CustomerRequirement> existingRequirements, Customer customer, Rng rng){
        int maxAttempts = 50; // 单个要求的最大尝试次数
        
        for (int attempt = 0; attempt < maxAttempts; attempt++){
            // 从模板中随机选择一个要求类型
            CustomerRequirement template = rng.PickOne(requirementsTemplates);
            
            // 克隆要求
            CustomerRequirement requirement = template.Clone();
            
            // 检查是否与已有要求冲突
            bool hasConflict = false;
            foreach (var existingReq in existingRequirements){
                if (requirement.IsConflicted(existingReq) || existingReq.IsConflicted(requirement)){
                    hasConflict = true;
                    break;
                }
            }
            
            if (!hasConflict){
                // 初始化要求（这会设置StarAmount等参数）
                requirement.Init(customer, new List<object>(), rng);
                return requirement;
            }
        }
        
        // 尝试多次仍无法找到不冲突的要求
        return null;
    }

    /// <summary>
    /// 检查总星数是否在允许范围内
    /// </summary>
    private bool CheckStarAmount(float sum){
        return sum >= minStarAmount && sum <= maxStarAmount;
    }

    /// <summary>
    /// 生成一个基础的要求组（当无法生成符合规则的要求组时的后备方案）
    /// </summary>
    private List<CustomerRequirement> GenerateFallbackGroup(Customer customer, Rng rng){
        List<CustomerRequirement> result = new List<CustomerRequirement>();
        
        // 至少生成一个要求
        CustomerRequirement requirement = rng.PickOne(requirementsTemplates).Clone();
        requirement.Init(customer, new List<object>(), rng);
        result.Add(requirement);
        
        return result;
    }
}