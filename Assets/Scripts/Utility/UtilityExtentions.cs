using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public static class UtilityExtentions{

    public static float Remap(this float value, float from1, float to1, float from2, float to2){
        return Mathf.Lerp(from2, to2, Mathf.InverseLerp(from1, to1, value));
    }
}