public static class StringExtension{
    public static string ToSize(this string text, float size){
        return $"<size={size}>{text}</size>";
    }
}