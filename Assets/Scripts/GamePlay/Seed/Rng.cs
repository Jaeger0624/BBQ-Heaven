using System;
using System.Collections.Generic;
using System.Linq;

public class Rng
{
    private Random _rand;
    public readonly int _initialSeed;

    // 【关键】记录当前已经调用了多少次，用于存档
    public int CallCount { get; private set; }
    public Rng(int initSeed)
    {
        _initialSeed = initSeed;
        Reset();
    }

    public int NextInt(int min, int max){
        CallCount++;
        return _rand.Next(min, max);
    }

    public float NextFloat()
    {
        CallCount++;
        return (float)_rand.NextDouble();
    }

    public bool NextBool(float trueProbability = 0.5f){
        return NextFloat() < trueProbability;
    }
    public T PickOne<T>(IList<T> list){
        if (list.Count == 0) return default(T);
        return list[NextInt(0, list.Count)];
    }
    public List<T> PickMany<T>(IList<T> list, int count){
        var result = new List<T>(list);
        int n = result.Count;
        count = Math.Min(count, n);

        for (int i = 0; i < count; i++)
        {
            int r = i + NextInt(0, n - i);
            (result[r], result[i]) = (result[i], result[r]);
        }
        return result.GetRange(0, count);
    }
    public void Reset(){
        _rand = new Random(_initialSeed);
        CallCount = 0;
    }

    public void RestoreState(int targetCallCount){
        Reset();
        while (CallCount < targetCallCount){
            _rand.Next();
            CallCount++;
        }
    }
}