Shader "Hidden/UI/UI-Effect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _FadeSpeed ("Fade Speed", float) = 1

        [Header(Stencil)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 3
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        _Stencil ("Stencil ID", Float) = 1
        _StencilWriteMask ("Stencil Write Mask", Float) = 1
        _StencilReadMask ("Stencil Read Mask", Float) = 0
        [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask ("Color Mask", Float) = 15

        //这个选项是影响是否使用RectMask2D的Softness属性
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True"
        }

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

            // -------------------------------------
            // Internal Keywords
            #pragma multi_compile_local_fragment _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local_fragment _ UNITY_UI_ALPHACLIP

            // -------------------------------------
            // Material Keywords  为了避免变体管理 先全部设置成 multi_compile_local
            #pragma multi_compile_local _ _FADELOOP_ON
            #pragma multi_compile_local _ _LINERASPACE_ON
            #pragma multi_compile_local _ _ROTATE_ON
            #pragma multi_compile_local _ FILL GREY
            #pragma multi_compile_local _ DISSOLVE
            #pragma multi_compile_local _ UISHINY


            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            #ifdef _FADELOOP_ON
            float _FadeSpeed;

            void ApplyFadeLoop(inout float alpha)
            {
                float time = _Time.z * _FadeSpeed;
                float a = abs(time - floor((time + 1) / 2) * 2);
                alpha *= a;
            }
            #endif

            #ifdef GREY
            float _EffectFactor;
            #endif

            #ifdef DISSOLVE
            float4 _DissolveParams;
            half4 _DissolveColor;
            sampler2D _DissolveTex;
            #endif

            #ifdef UISHINY
            float4 _ShinyParams1, _ShinyParams2;

            #define ShinyEffectFactor   _ShinyParams1.x
            #define ShinyWidth          _ShinyParams1.y
            #define ShinySoftness       _ShinyParams1.z
            #define ShinyBrightness     _ShinyParams1.w
            #define ShinyGloss          _ShinyParams2.x
            #define ShinyRotation       _ShinyParams2.y

            void Unity_Remap_float4(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            // Apply shiny effect.
            half4 ApplyShinyEffect(half4 color, float2 newUV)
            {
                half nomalizedPos = newUV.x;
                // fixed4 param1 = tex2D(_ParamTex, float2(0.25, shinyParam.y));
                // fixed4 param2 = tex2D(_ParamTex, float2(0.75, shinyParam.y));
                float finalShinyFactor;
                Unity_Remap_float4(ShinyEffectFactor, float2(0, 1), float2(-0.2, 1.2), finalShinyFactor);
                half location = finalShinyFactor; //param1.x * 2 - 0.5;
                half normalized = 1 - saturate(abs((nomalizedPos - location) / ShinyWidth));
                half shinePower = smoothstep(0, ShinySoftness, normalized);
                half3 reflectColor = lerp(half3(1, 1, 1), color.rgb * 7, ShinyGloss);

                color.rgb += color.a * (shinePower / 2) * ShinyBrightness * reflectColor;

                return color;
            }

            float2 ApplyRotateShiny(float2 uv)
            {
                half2 center = half2(0.5, 0.5);
                // half2 center = _RotateCenter;
                half2 uvC = uv;
                half cosAngle = cos(ShinyRotation);
                half sinAngle = sin(ShinyRotation);
                half2x2 rot = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uvC -= center;
                float2 newUV = mul(rot, uvC);
                newUV += center;
                return newUV;
            }
            #endif

            #ifdef _ROTATE_ON
            float _RotateSpeed;
            float2 _RotateCenter;

            void ApplyRotate(float2 uv, out float2 newUV)
            {
                half2 center = half2(0.5, 0.5);
                // half2 center = _RotateCenter;
                half2 uvC = uv;
                half cosAngle = cos(_Time.z * _RotateSpeed);
                half sinAngle = sin(_Time.z * _RotateSpeed);
                half2x2 rot = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uvC -= center;
                newUV = mul(rot, uvC);
                newUV += center;
            }
            #endif

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.texcoord.zw = v.texcoord.zw;

                OUT.color = v.color * _Color;

                #ifdef _ROTATE_ON
                ApplyRotate(OUT.texcoord.xy, OUT.texcoord.xy);
                #endif

                return OUT;
            }

            half4 ApplyColorEffect(half4 color, half4 factor)
            {
                #ifdef FILL
                color.rgb = lerp(color.rgb, factor.rgb, factor.a);
                color.a = color.a * factor.a;
                #elif GREY
                color.rgb = lerp(color.rgb, Luminance(color.rgb), _EffectFactor);
                #else
                color.rgb = lerp(color.rgb, color.rgb * factor.rgb, factor.a);
                #endif
                return color;
            }

            half4 ApplyTransitionEffect(half4 color, float2 uv)
            {
                #ifdef DISSOLVE
                float alpha = tex2D(_DissolveTex, uv * _DissolveParams.w).a;

                fixed width = _DissolveParams.y / 4;
                fixed softness = _DissolveParams.z;
                fixed3 dissolveColor = _DissolveColor;
                float factor = alpha - _DissolveParams.x * (1 + width) + width;
                fixed edgeLerp = step(factor, color.a) * saturate((width - factor) * 16 / softness);
                // color = ApplyColorEffect(color, fixed4(dissolveColor, edgeLerp));
                color.rgb = lerp(color.rgb, color.rgb * dissolveColor.rgb, edgeLerp);
                color.a *= saturate((factor) * 32 / softness);
                #endif
                return color;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord.xy;
                float2 originUV = IN.texcoord.zw;
                // return half4(uv,0,1);
                half4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd);

                // Dissolve
                color = ApplyTransitionEffect(color, originUV);
                color = ApplyColorEffect(color, IN.color);

                #ifdef UISHINY
                color = ApplyShinyEffect(color, ApplyRotateShiny(originUV));
                #endif

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                #ifdef _FADELOOP_ON
                ApplyFadeLoop(color.a);
                #endif

                #ifdef _LINERASPACE_ON
                half3 temp = (color.a).xxx;
                half3 gammaToLinear8 = GammaToLinearSpace(temp);
                color.a = gammaToLinear8;
                #endif

                color.a *= IN.color.a;

                return color;
            }
            ENDCG
        }
    }

    // --------------------------------------------
    CustomEditor "Scarecrow.SimpleShaderGUI"
}