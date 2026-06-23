Shader "PS1/VertexTextureBlend"
{
    Properties
    {
        _BaseTex ("Base (unpainted areas)", 2D) = "white" {}
        _RTex    ("Red layer",   2D) = "white" {}
        _GTex    ("Green layer", 2D) = "white" {}
        _BTex    ("Blue layer",  2D) = "white" {}
        _Tint    ("Tint", Color) = (1,1,1,1)
        _SnapResolution ("PS1 Snap Resolution (lower = chunkier)", Float) = 64
        [Toggle(_AFFINE_ON)] _Affine ("Affine UV Warp", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _AFFINE_ON
            #include "UnityCG.cginc"

            sampler2D _BaseTex; float4 _BaseTex_ST;
            sampler2D _RTex;
            sampler2D _GTex;
            sampler2D _BTex;
            fixed4 _Tint;
            float _SnapResolution;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;   // <- the painted vertex colors
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                #if defined(_AFFINE_ON)
                noperspective float2 uv : TEXCOORD0;  // kills perspective correction -> PS1 warp
                #else
                float2 uv : TEXCOORD0;
                #endif
                fixed4 color : COLOR0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float4 clip = UnityObjectToClipPos(v.vertex);

                // PS1 vertex snapping (quantize to a low-res grid)
                clip.xy = floor((clip.xy / clip.w) * _SnapResolution) / _SnapResolution * clip.w;

                o.pos   = clip;
                o.uv    = TRANSFORM_TEX(v.uv, _BaseTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseTex, i.uv);

                // each channel layers its texture on top, weighted by the painted value
                col = lerp(col, tex2D(_RTex, i.uv), i.color.r);
                col = lerp(col, tex2D(_GTex, i.uv), i.color.g);
                col = lerp(col, tex2D(_BTex, i.uv), i.color.b);

                return col * _Tint;
            }
            ENDCG
        }

        // ---- Depth / shadow pass. Fills _CameraDepthTexture so water foam
        //      can detect where it meets the terrain. Opaque, so no alpha clip. ----
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            float _SnapResolution;

            struct v2f { V2F_SHADOW_CASTER; };

            v2f vert (appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

                // match the visible silhouette's snap so the foam edge tracks the terrain
                o.pos.xy = floor((o.pos.xy / o.pos.w) * _SnapResolution) / _SnapResolution * o.pos.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}
