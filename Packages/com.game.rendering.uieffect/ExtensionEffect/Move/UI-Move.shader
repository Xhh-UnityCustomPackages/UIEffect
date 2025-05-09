Shader "Hidden/UI/UI-Move"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" { }
        _MoveTex ("Move Texture", 2D) = "white" { }
        _MoveSpeed ("Move Speed", float) = 1

        

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
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 texcoord : TEXCOORD0;
                // float4 texcoord2 : TEXCOORD1;
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


            //---
            sampler2D _MoveTex;
            float _MoveSpeed;
            float4 _MoveTex_ST;
            float4 _UVRect;
            //---

            inline float2 NormalizeAtlasSpriteUV(float2 uv)
            {
                float width = _UVRect.z - _UVRect.x;
                float height = _UVRect.w - _UVRect.y;
                return float2((uv.x - _UVRect.x) / width, (uv.y - _UVRect.y) / height);
            }
            
            inline float2 ReverseNormalizeAtlasSpriteUV(float2 uv)
            {
                float2 r = uv;
                r.x = _UVRect.x + (_UVRect.z - _UVRect.x) * r.x;
                r.y = _UVRect.y + (_UVRect.w - _UVRect.y) * r.y;
                return r;
            }

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
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                
            #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
            #endif

            #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
            #endif

                float2 uv = IN.texcoord.zw;
                float2 uvMove = uv * _MoveTex_ST.xy;
                uvMove.x += frac(_Time.z * - _MoveSpeed);
                half4 move = tex2D(_MoveTex, uvMove);
                // move.a = color.a;
                half moveMask = step(0.5, move.a);
                half3 finalRGB = color.rgb * (1 - move.a) + move * move.a;
                // return half4(uv, 0, 1);
                return half4(finalRGB, color.a);
                // color.rgb += move;
                // color.a = move.a;
                // return color;

            }
            ENDCG
        }
    }

    // --------------------------------------------
    CustomEditor "Scarecrow.SimpleShaderGUI"
}
