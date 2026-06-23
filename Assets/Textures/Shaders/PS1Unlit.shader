Shader "Doody/PS1Unlit"
{
    Properties
    {
        [Header(Surface)]
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Alpha)]
        [Toggle(_FADE_ON)] _Fade ("Soft Faded Alpha (off = hard cutout)", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5

        [Header(PS1)]
        _SnapResolution ("Vertex Snap Resolution", Float) = 160
    }

    SubShader
    {
        // AlphaTest queue + ZWrite On so these meshes land in _CameraDepthTexture.
        // That is what the water foam reads to find intersections.
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Cull Off            // double-sided
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _FADE_ON
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;  float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff, _SnapResolution;

            v2f vert (appdata v)
            {
                v2f o;
                float4 clip = UnityObjectToClipPos(v.vertex);

                // PS1 vertex snap (xy only)
                float2 grid = _SnapResolution;
                clip.xy = floor(clip.xy / clip.w * grid) / grid * clip.w;

                o.pos = clip;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

            #if defined(_FADE_ON)
                // soft faded alpha blend
                return col;
            #else
                // hard cutout
                clip(col.a - _Cutoff);
                col.a = 1.0;
                return col;
            #endif
            }
            ENDCG
        }

        // ---- Depth / shadow pass. THIS is what fills _CameraDepthTexture. ----
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _FADE_ON
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            sampler2D _MainTex;  float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff, _SnapResolution;

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

                // match the visible silhouette's snap so foam tracks the edges
                float2 grid = _SnapResolution;
                o.pos.xy = floor(o.pos.xy / o.pos.w * grid) / grid * o.pos.w;

                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // cutout: don't let clipped pixels write depth (clean foam edge).
                // fade: clip on the same cutoff so the depth silhouette stays sane.
                fixed a = tex2D(_MainTex, i.uv).a * _Color.a;
                clip(a - _Cutoff);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    Fallback Off
}
