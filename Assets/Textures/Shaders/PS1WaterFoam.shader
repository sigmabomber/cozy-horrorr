Shader "Doody/PS1WaterFoam"
{
    Properties
    {
        [Header(Water)]
        _ShallowColor ("Shallow Color", Color) = (0.25, 0.55, 0.7, 0.7)
        _DeepColor ("Deep Color", Color) = (0.05, 0.15, 0.35, 0.9)
        _DepthFade ("Depth Fade Distance", Float) = 3.0

        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _FoamDistance ("Foam Distance", Float) = 0.6
        _FoamCutoff ("Foam Cutoff", Range(0,1)) = 0.4
        _FoamNoise ("Foam Noise Tex", 2D) = "white" {}
        _FoamNoiseScale ("Foam Noise Scale", Float) = 4.0
        _FoamSpeed ("Foam Scroll Speed", Vector) = (0.05, 0.03, 0, 0)

        [Header(Surface)]
        _MainTex ("Water Texture", 2D) = "white" {}
        _ScrollSpeed ("Water Scroll Speed", Vector) = (0.04, 0.02, 0, 0)

        [Header(Waves)]
        _WaveAmp ("Wave Amplitude", Float) = 0.08
        _WaveFreq ("Wave Frequency", Float) = 1.5
        _WaveSpeed ("Wave Speed", Float) = 1.0

        [Header(PS1)]
        _SnapResolution ("Vertex Snap Resolution", Float) = 160
        _AffineAmount ("Affine Warp Amount", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off   // double-sided, matches your other shaders

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float2 uvPersp   : TEXCOORD0;   // perspective-correct UV
                float3 uvAffine  : TEXCOORD1;   // (uv * w, w) packed for affine warp
                float4 screenPos : TEXCOORD2;
                float3 worldPos  : TEXCOORD3;
            };

            sampler2D _MainTex;          float4 _MainTex_ST;
            sampler2D _FoamNoise;        float4 _FoamNoise_ST;
            sampler2D _CameraDepthTexture;

            fixed4 _ShallowColor, _DeepColor, _FoamColor;
            float _DepthFade, _FoamDistance, _FoamCutoff, _FoamNoiseScale;
            float4 _FoamSpeed, _ScrollSpeed;
            float _WaveAmp, _WaveFreq, _WaveSpeed;
            float _SnapResolution, _AffineAmount;

            v2f vert (appdata v)
            {
                v2f o;

                // world-space wave displacement
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float wave = sin((worldPos.x + worldPos.z) * _WaveFreq + _Time.y * _WaveSpeed);
                worldPos.y += wave * _WaveAmp;
                o.worldPos = worldPos;

                float4 clip = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

                // PS1 vertex snap (xy only; w untouched so warp math stays valid)
                float2 grid = _SnapResolution;
                clip.xy = floor(clip.xy / clip.w * grid) / grid * clip.w;

                o.pos = clip;
                o.screenPos = ComputeScreenPos(clip);

                // affine texture warping:
                // packing (uv * w) and w, then dividing in frag, cancels the GPU's
                // perspective correction -> screen-linear UVs that "swim" like a PS1.
                float2 baseUV = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvPersp  = baseUV;
                o.uvAffine = float3(baseUV * clip.w, clip.w);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // depth of whatever is behind the water surface
                float sceneZ = LinearEyeDepth(
                    SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float surfaceZ = i.screenPos.w;
                float diff = sceneZ - surfaceZ;          // distance from water to mesh below

                // water color by depth
                float depthBlend = saturate(diff / _DepthFade);
                fixed4 water = lerp(_ShallowColor, _DeepColor, depthBlend);

                // affine vs perspective UV, blended by _AffineAmount, then scrolled
                float2 affineUV = i.uvAffine.xy / i.uvAffine.z;
                float2 uv = lerp(i.uvPersp, affineUV, _AffineAmount);
                uv += _ScrollSpeed.xy * _Time.y;
                water.rgb *= tex2D(_MainTex, uv).rgb;

                // intersection foam (world-projected, not warped)
                float2 nuv = i.worldPos.xz / _FoamNoiseScale + _FoamSpeed.xy * _Time.y;
                float noise = tex2D(_FoamNoise, nuv).r;
                float foamLine = 1.0 - saturate(diff / _FoamDistance);
                float foam = step(_FoamCutoff, foamLine * noise);   // banded, hard PS1 edge

                fixed4 col = lerp(water, _FoamColor, foam);
                col.a = max(water.a, foam);              // foam draws solid
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
