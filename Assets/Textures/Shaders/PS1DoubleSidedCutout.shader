Shader "Custom/PS1DoubleSidedCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _SnapResolution ("Vertex Snap (lower = chunkier)", Float) = 160
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Cull Off   // double-sided

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

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

                // PS1 vertex snap: round position to a low-res grid in NDC
                clip.xyz /= clip.w;                                  // perspective divide
                clip.xy = floor(clip.xy * _SnapResolution) / _SnapResolution;
                clip.xyz *= clip.w;                                  // undo divide

                o.vertex = clip;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                clip(col.a - _Cutoff);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
