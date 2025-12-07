using System;
using System.Collections.Generic;
using System.Linq;

public class Rng
{
    private Random _rand;
    public int Seed { get; private set; }
    public Rng(int seed)
    {
        Seed = seed;
        _rand = new Random(seed);
    }

    public int NextInt(int min, int max)
        => _rand.Next(min, max);

    public float NextFloat()
        => (float)_rand.NextDouble();

    public bool NextBool(float trueProbability = 0.5f)
        => NextFloat() < trueProbability;

    public T PickOne<T>(IList<T> list)
        => list[NextInt(0, list.Count)];

    public List<T> PickMany<T>(IList<T> list, int count)
        => list.OrderBy(x => NextFloat()).Take(count).ToList();

    public void Reset()
        => _rand = new Random(Seed);

    public void SetSeed(int seed)
    {
        Seed = seed;
        _rand = new Random(seed);
    }
}
