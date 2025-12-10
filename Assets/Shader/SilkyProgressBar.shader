Shader "UI/SilkyProgressBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Flow Noise (R Channel)", 2D) = "white" {} // 流光噪波图
        
        _Color ("Main Color", Color) = (0.2, 0.6, 1, 1)      // 填充颜色
        [HDR]_EdgeColor ("Edge Color", Color) = (1, 2, 3, 1) // 边缘发光色 (HDR)
        
        _Progress ("Progress", Range(0, 1)) = 0.5            // 进度
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05      // 发光宽度
        _Distortion ("Distortion", Range(0, 0.2)) = 0.05     // 扭曲强度
        _Speed ("Flow Speed", Float) = 0.5                   // 流动速度

        // UI 系统必须的属性
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _WriteMask ("Stencil Write Mask", Float) = 255
        _ReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            
            fixed4 _Color;
            fixed4 _EdgeColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _Progress;
            float _EdgeWidth;
            float _Distortion;
            float _Speed;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // 1. 采样噪波 (带时间流动)
                float2 noiseUV = uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                noiseUV.x -= _Time.y * _Speed; // 让纹理向左流动
                float noise = tex2D(_NoiseTex, noiseUV).r;

                // 2. 核心算法：用噪波干扰 UV.x
                // 原始进度比较是: uv.x < _Progress
                // 干扰后是: uv.x - noise * distortion < _Progress
                float distortedUVx = uv.x + (noise - 0.5) * _Distortion;

                // 3. 计算填充遮罩 (使用 smoothstep 做抗锯齿)
                // 这里用 0.01 的平滑度，防止边缘太硬
                float fillMask = 1.0 - smoothstep(_Progress, _Progress + 0.01, distortedUVx);

                // 4. 计算边缘高亮遮罩
                // 只在进度条前端显示
                float edgeMask = smoothstep(_Progress - _EdgeWidth, _Progress, distortedUVx) * fillMask;

                // 5. 组合颜色
                // 基础色 + 边缘叠加
                float4 finalColor = IN.color;
                finalColor.rgb += _EdgeColor.rgb * edgeMask; // 叠加发光
                finalColor.a *= fillMask; // 裁切透明度

                // 6. UI 系统的裁剪 (Mask / RectMask2D)
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}