Shader "Seikami/UI/UIUberEffect (UI特效)"//为了适配图集
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _SpeedX ("SpeedX", range(-1, 1)) = 0
        _SpeedY ("SpeedY", range(-1, 1)) = 0

        [Foldout(1, 1, 1, 1)] _F_Warp ("Warp_Foldout", float) = 1
        [Tex][NoScaleOffset] _WarpTex ("WarpTex", 2D) = "white" { }
        _WarpIntensity ("Warp Intensity", Range(0, 1)) = 0.5
        _WarpTilling ("Warp Tilling", float) = 1
        _WarpSpeedX ("Warp SpeedX", range(-1, 1)) = 0
        _WarpSpeedY ("Warp SpeedY", range(-1, 1)) = 0
        [Foldout_Out(1)] _F_Warp_Out ("Warp_Out_Foldout", float) = 1

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [Toggle(UNITY_UI_ATLAS)] _UseSpriteAtlas ("Use Sprite Atlas  (需要UIEffect脚本)", Float) = 0
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
        Blend One OneMinusSrcAlpha
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

            #pragma shader_feature_local _ UNITY_UI_ATLAS
            #pragma shader_feature_local _F_WARP_ON

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
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _SpeedX, _SpeedY;
            sampler2D _WarpTex;
            float _WarpIntensity, _WarpTilling;
            float _WarpSpeedX, _WarpSpeedY;


            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;
                OUT.texcoord.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.texcoord.zw = v.texcoord.zw;
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                #ifdef UNITY_UI_ATLAS
                float2 originUV = IN.texcoord.zw;
                #else
                float2 originUV = IN.texcoord.xy;
                #endif

                half warp = 0;
                #ifdef _F_WARP_ON
                float2 warpUV = originUV * _WarpTilling - _Time.y * float2(_WarpSpeedX, _WarpSpeedY);
                warp = (tex2D(_WarpTex, warpUV) + _TextureSampleAdd) * _WarpIntensity;
                #endif
                float2 flowUV = originUV.xy - _Time.y * float2(_SpeedX, _SpeedY);
                half4 color = IN.color * (tex2D(_MainTex, flowUV + warp) + _TextureSampleAdd);


                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
            ENDCG
        }
    }
    // --------------------------------------------
    CustomEditor "Scarecrow.SimpleShaderGUI"
}