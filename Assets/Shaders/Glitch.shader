Shader "Unlit/Glitch"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Charge("Charge", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float3 normal : NORMAL;
                float3 positionWS : POSITION_WS;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Charge;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.vertex.xyz);
                output.vertex = TransformWorldToHClip(output.positionWS);
                output.normal = TransformObjectToWorldNormal(input.normal);
                return output;
            }

            float dither(float2 uv)
            {
                float DITHER_THRESHOLDS[16] =
                {
                    1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
                };
                uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
                return DITHER_THRESHOLDS[index];
            }

            half4 frag(Varyings i) : SV_Target
            {
                i.normal = normalize(i.normal);
                half3 view = normalize(_WorldSpaceCameraPos - i.positionWS);
                float d = sqrt(1 - pow(dot(view, i.normal), 2));

                float alpha = d > _Charge ? d * 0.5 * _Charge : 1;
                clip(alpha - dither(i.vertex.xy * _ScreenParams.zw * 0.5));

                return _Color;
            }
            ENDHLSL
        }
    }
}