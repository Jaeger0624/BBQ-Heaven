using UnityEngine;

public interface IEarnMoneyStrategy{
    int EarnMoney(int satisfaction);
}


public class EarnMoneyStrategy_原值 : IEarnMoneyStrategy
{
    public int EarnMoney(int satisfaction)
    {
        return satisfaction;
    }
}

public class EarnMoneyStrategy_对数增长 : IEarnMoneyStrategy
{
    public int EarnMoney(int satisfaction)
    {
        return (int)(satisfaction * Mathf.Log(1.1f));
    }
}
