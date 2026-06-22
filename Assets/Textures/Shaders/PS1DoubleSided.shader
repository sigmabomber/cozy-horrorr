Shader "Custom/PS1DoubleSided"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [Toggle(_FADE_ON)] _Fade ("Faded alpha (off = hard cutout)", Float) = 0
        _Cutoff ("Alpha Cutoff (cutout mode)", Range(0,1)) = 0.5
        [Toggle] _ZWrite ("ZWrite (on for cutout, off for faded)", Float) = 1
        _SnapResolution ("Vertex Snap (lower = chunkier)", Float) = 160
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off                          // double-sided
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite [_ZWrite]                  // driven by the material toggle

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature_local _FADE_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Cutoff;
            float _SnapResolution;

            v2f vert (appdata v)
            {
                v2f o;

                float4 clip = UnityObjectToClipPos(v.vertex);

                // PS1 vertex snap
                clip.xyz /= clip.w;
                clip.xy = floor(clip.xy * _SnapResolution) / _SnapResolution;
                clip.xyz *= clip.w;

                o.vertex = clip;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

            #if _FADE_ON
                // faded: keep the texture's alpha so edges blend smoothly
            #else
                // cutout: discard below threshold, keep the rest fully solid
                clip(col.a - _Cutoff);
                col.a = 1.0;
            #endif

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
