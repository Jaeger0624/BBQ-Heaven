void CircleSDF_float(float2 UV, float Radius, out float Dist)
{
    Dist = length(UV) - Radius;
}

// void RectangleSDF_float(float2 UV, float2 Size, out float Dist)
// {
//     float2 d = abs(UV) - Size;
//     Dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
// }


void RectangleSDF_float(float2 UV, float2 Size, float4 CornerRounding, float cornerRounding, out float Dist)
{
    float2 centered = UV;
    float2 q = abs(centered);

    float top_mask = step(0.0, centered.y);
    float right_mask = step(0.0, centered.x);

    float left_side_rounding = lerp(CornerRounding.x, CornerRounding.y, top_mask);
    float right_side_rounding = lerp(CornerRounding.w, CornerRounding.z, top_mask);

    float r_individial = lerp(left_side_rounding, right_side_rounding, right_mask);

    float r = r_individial + cornerRounding;

    float2 d = q - Size + r;
    Dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
}

void OutlineSDF_float(float Distance, float Thickness, out float Dist)
{
    Dist = abs(Distance) - Thickness;
}

void InlineSDF_float(float Distance, float Thickness, out float Dist)
{
    Dist = max(Distance, -Distance - Thickness);
}