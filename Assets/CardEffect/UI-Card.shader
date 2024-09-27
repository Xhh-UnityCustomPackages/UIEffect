Shader "Seikami/UI/UI-Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)

        [Header(Dissolve)]
        _DissolveTex ("DissolveTex", 2D) = "white" { }
        _Dissolve_progress ("Dissolve_progress", Range(-0.1, 1)) = 1

        _EdgeTex ("EdgeTex", 2D) = "black" { }
        _EdgeMask ("EdgeMask", 2D) = "white" { }
        _EdgeIntensity ("EdgeIntensity", Range(0, 5)) = 5
        _Edge_progreess ("Edge_progress", Range(-5, 5)) = 1
        [HDR]_AddColor ("AddColor", Color) = (1, 1, 1, 0)

        _GradientTex ("GradientTex", 2D) = "white" { }
        _Gradient_Intensity ("Gradient_Intensity", Float) = 0.1
        _Gradient_progress ("Gradient_progress", Range(-1, 5)) = 1

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
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
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float4 _EffectRect;

            sampler2D _DissolveTex;
            float _Dissolve_progress;
            sampler2D _EdgeTex;
            sampler2D _EdgeMask;
            float _Edge_progreess, _EdgeIntensity;
            half3 _AddColor;

            float _Gradient_Intensity;
            sampler2D _GradientTex;
            float _Gradient_progress;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            inline float GetEffectRect(in float2 position, in float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
                return inside.x * inside.y;
            }


            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                
            #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
            #endif

            #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
            #endif

                //需要转化为 _EffectRect下的UV
                float2 rectUV = IN.worldPosition.xy - _EffectRect.xy;
                rectUV /= (_EffectRect.zw - _EffectRect.xy);
                rectUV = saturate(rectUV);
                // return float4(rectUV, 0, 1);

                float effectRect = GetEffectRect(IN.worldPosition.xy, _EffectRect);
                // return effect * color;

                
                //Dissolve
                half var_DissolveTex = tex2D(_DissolveTex, rectUV).r;
                var_DissolveTex = saturate(1 - var_DissolveTex);
                half dissolve = step(var_DissolveTex, _Dissolve_progress);

                //Edge
                half var_EdgeTex = tex2D(_EdgeTex, rectUV).r;
                half var_EdgeMask = tex2D(_EdgeMask, float2(rectUV.x, rectUV.y + _Dissolve_progress + _Edge_progreess)).r;
                half edgeMask = var_EdgeTex * var_EdgeMask;
                // return edgeMask * effectRect;
                half3 edge = edgeMask * _EdgeIntensity * _AddColor ;
                
                //Gradient
                half var_Gradient = tex2D(_GradientTex, float2(rectUV.x, rectUV.y + _Dissolve_progress + _Gradient_progress)).r;
                half3 gradient = var_Gradient * _Gradient_Intensity * _AddColor;

                
                // color.rgb = lerp(color.rgb, gradient, var_Gradient);
                color.rgb = lerp(color.rgb, edge, edgeMask);
                color.a *= effectRect * dissolve;

                // color.rgb = gradient;

                return color;


                // color.a *= effectRect;
                // return half4(transition.rgb * color.rgb, color.a);

            }
            ENDCG
        }
    }
}
