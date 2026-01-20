using System;
using UnityEngine;

public static class Distribution{
    public static float SimpleNormal(float mean, float stdDev, int amount = 3, Rng rng = null){
        if (rng == null) rng = new Rng(UnityEngine.Random.Range(0, 1000000));
        // 叠加 amount 次均匀分布
        float rand = 0;
        for (int i = 0; i < amount; i++){
            rand += rng.NextFloat();
        }
        float normal = rand - amount / 2f; 
        return mean + normal * stdDev;
    }


    public static float BoxMullerNormal(float mean, float stdDev, Rng rng = null){
        if (rng == null) rng = new Rng(UnityEngine.Random.Range(0, 1000000));
        float u1 = rng.NextFloat();
        float u2 = rng.NextFloat();
        float z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
        return mean + z0 * stdDev;
    }
}

public static class MathExtension{
    public static Vector2 Rotate(this Vector2 vector, float angle){
        float radius = angle * Mathf.Deg2Rad;
        return new Vector2(vector.x * Mathf.Cos(radius) - vector.y * Mathf.Sin(radius), vector.x * Mathf.Sin(radius) + vector.y * Mathf.Cos(radius));
    }
}