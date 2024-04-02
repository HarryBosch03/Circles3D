Shader "Skybox/Custom/Skybox"
{
    Properties
    {
        _ZenithColor("Zenith", Color) = (1, 1, 1, 1)
        _HorizonColor("Horizon", Color) = (1, 1, 1, 1)
        _SunColor("Sun", Color) = (1, 1, 1, 1)
        _SunSize("Sun Size", Range(0, 1)) = 1
        _SunBrightness("Sun Brightness", float) = 1
        
        _CloudSize("Cloud Size", float) = 3
        _CloudHeight("Cloud Height", float) = 0.1
        _CloudRoundness("Cloud Roundness", float) = 0.3
        _CloudDensity("Cloud Density", float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = TransformObjectToHClip(input.vertex);
                output.uv = input.uv;
                output.normal = input.vertex.xyz;
                return output;
            }

            half4 _ZenithColor;
            half4 _HorizonColor;
            half4 _SunColor;

            float _SunSize;
            float _SunBrightness;
            
            float _CloudSize;
            float _CloudRoundness;
            float _CloudDensity;
            float _CloudHeight;

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float sqr(float x) { return x * x; }

            float2 randomGradient(int2 p0)
            {
                float angle = hash12(p0) * PI * 2;
                return float2(cos(angle), sin(angle));
            }

            float dotGridGradient(int x0, int y0, float2 p)
            {
                int2 p0 = int2(x0, y0);
                float2 gradient = randomGradient(p0);
                float2 pd = p - p0;

                return dot(pd, gradient);
            }

            float smootherstep(float x)
            {
                return x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
            }

            float interpolate(float a, float b, float t)
            {
                return lerp(a, b, smootherstep(t));
            }

            float perlin(float2 position)
            {
                int2 p0 = (int2)floor(position);
                float2 pd = position - p0;

                float g0 = dotGridGradient(p0.x + 0, p0.y + 0, position);
                float g1 = dotGridGradient(p0.x + 1, p0.y + 0, position);
                float g01 = interpolate(g0, g1, pd.x);

                float g2 = dotGridGradient(p0.x + 0, p0.y + 1, position);
                float g3 = dotGridGradient(p0.x + 1, p0.y + 1, position);
                float g23 = interpolate(g2, g3, pd.x);

                float g0123 = interpolate(g01, g23, pd.y);

                return g0123 * 0.5 + 0.5;
            }

            float multisamplePerlin(float2 position)
            {
                float sample = 0;
                float max = 0;
                for (int i = 0; i < 3; i++)
                {
                    float f = pow(2.0, i);
                    float a = pow(0.5, i);;

                    sample += perlin(position * f) * a;
                    max += a;
                }
                return sample / max;
            }

            float4 SampleClouds(Varyings input)
            {
                if (input.normal.y < 0) return 0;

                int steps = 32;
                for (int i = 0; i < steps; i++)
                {
                    float percent = i / (float)steps;
                    float height = 1 + percent * _CloudHeight;
                    float brightness = percent * 0.5 + 0.5;
                    float offset = sqrt(1 - sqr(2 * percent - 1));
                    float2 uv = input.normal.xz * (height / input.normal.y) + float2(_Time.y * 0.01, 0);
                    if (multisamplePerlin(uv * _CloudSize) + offset * _CloudRoundness * 0.1 - _CloudDensity > 0.5) return float4(brightness.xxx, 1.0);
                }

                return 0;
            }

            half4 frag(Varyings input) : SV_Target
            {
                input.normal = normalize(input.normal);

                float t = pow(saturate(input.uv.y), 0.5);
                half3 col = lerp(_HorizonColor, _ZenithColor, t);
                
                half4 clouds = SampleClouds(input);
                col.rgb += clouds.rgb * clouds.a * sqr(input.normal.y);

                float sun = pow(dot(_MainLightPosition, input.normal), 100 / _SunSize) * _SunBrightness;
                float sunOccluded = pow(dot(_MainLightPosition, input.normal), 1 / _SunSize) * _SunBrightness * 0.02;
                sun = lerp(sun, sunOccluded, clouds.a);
                col += _SunColor * max(sun, 0);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}