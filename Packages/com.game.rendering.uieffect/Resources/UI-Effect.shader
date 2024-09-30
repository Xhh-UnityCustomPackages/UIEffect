Shader "Hidden/UI/UI-Effect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _FadeSpeed ("Fade Speed", float) = 1

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        //这个选项是影响是否使用RectMask2D的Softness属性
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

            // -------------------------------------
            // Internal Keywords
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            // -------------------------------------
            // Material Keywords  为了避免变体管理 先全部设置成 multi_compile_local
            #pragma multi_compile_local _ _FADELOOP_ON
            #pragma multi_compile_local _ _LINERASPACE_ON
            #pragma multi_compile_local _ _ROTATE_ON
            #pragma multi_compile_local _ FILL GREY
            #pragma multi_compile_local _ DISSOLVE
            

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

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;

            #ifdef _ROTATE_ON
                ApplyRotate(OUT.texcoord, OUT.texcoord);
            #endif

                return OUT;
            }

            half4 ApplyColorEffect(half4 color, half4 factor)
            {
            #ifdef FILL
                color.rgb = lerp(color.rgb, factor.rgb, factor.a);
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
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                // Dissolve
                color = ApplyTransitionEffect(color, IN.texcoord);
                color = ApplyColorEffect(color, IN.color);
                
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

                return color;
            }
            ENDCG
        }
    }

    // --------------------------------------------
    CustomEditor "Scarecrow.SimpleShaderGUI"
}
