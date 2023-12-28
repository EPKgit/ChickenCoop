Shader "Unlit/OrbShader"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        [NoScaleOffset] _HighlightTex ("Highlight Texture", 2D) = "white" {}
        _HighlightStrength ("Highlight Strength", Range(0,1)) = 1
        _HighlightFrequency ("Highlight Frequency", float) = 1
        _ShimmerTint("Shimmer Tint", Color) = (1,1,1,1)
        _ShimmerSize ("Shimmer Size", Range(0,1)) = 0.1
        _ShimmerFrequency ("Shimmer Frequency", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend One Zero
        AlphaToMask On 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            inline float unity_noise_randomValue (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
            }

            inline float unity_noise_interpolate (float a, float b, float t)
            {
                return (1.0-t)*a + (t*b);
            }

            inline float unity_valueNoise (float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = unity_noise_randomValue(c0);
                float r1 = unity_noise_randomValue(c1);
                float r2 = unity_noise_randomValue(c2);
                float r3 = unity_noise_randomValue(c3);

                float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
                float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
                float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
                return t;
            }

            float Unity_SimpleNoise_float(float2 UV, float Scale)
            {
                float t = 0.0;

                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3-0));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3-1));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3-2));
                t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

                return t;
            }

            struct appdata
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            
            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.position);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            sampler2D _MainTex;
            fixed4 _Tint;
            sampler2D _HighlightTex;
            float _HighlightStrength;
            float _HighlightFrequency;
            fixed4 _ShimmerTint;
            float _ShimmerFrequency;
            float _ShimmerSize;
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTextureSample = tex2D(_MainTex, i.uv);
                mainTextureSample *= _Tint; // shader side tint
                mainTextureSample *= i.color; // vertex color which brings in the sprite renderer set color

                fixed4 highlightTextureSample = tex2D(_HighlightTex, i.uv);
                float highlightStrength = _HighlightStrength * highlightTextureSample.a;

                highlightStrength *= (sin(_Time * _HighlightFrequency) * 0.5) + 0.75;
                highlightStrength = clamp(highlightStrength, 0, 1);

                fixed3 mainTextureColor = mainTextureSample.rgb * (1 - highlightStrength) + (highlightTextureSample.rgb * highlightStrength);
                float shimmerPosition = _Time * _ShimmerFrequency;
                shimmerPosition -= (int)shimmerPosition; //only get the fracitonal portion
                bool isInShimmer = abs(shimmerPosition - i.uv.x) < _ShimmerSize;
                mainTextureColor = mainTextureColor * !isInShimmer + isInShimmer * mainTextureColor * _ShimmerTint;
                return fixed4(mainTextureColor.rgb, mainTextureSample.a);
            }

            
            ENDCG
        }
    }
}
