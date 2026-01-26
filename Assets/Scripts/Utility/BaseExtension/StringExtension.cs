using Unity.VisualScripting;
using UnityEngine;

public static class StringExtension{
    public static string ToSize(this string text, float size){
        return $"<size={size}>{text}</size>";
    }

    public static string ToColor(this string text, Color color){
        return $"<color=#{color.ToHexString()}>{text}</color>";
    }
}