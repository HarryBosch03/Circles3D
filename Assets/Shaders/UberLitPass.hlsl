#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "./UberCommon.hlsl"

#define DECLARE_TEX(name) TEXTURE2D(name); SAMPLER(sampler##name); float4 name##_ST; float4 name##_TexelSize;
#define SAMPLE_TEX(name, uv) SAMPLE_TEXTURE2D(name, sampler##name, uv * name##_ST.xy + name##_ST.zw)

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : POSITION_WS;
    float3 normalWS : NORMAL;
    float4 tangentWS : TANGENT;
    float2 uv : TEXCOORD0;
};

DECLARE_TEX(_Albedo);
DECLARE_TEX(_NormalMap);
float _NormalStrength;
DECLARE_TEX(_Tint_Mask);
float _InvertTint;
float4 _Tint_Color;
float _Alpha;

Varyings UberLitPassVertex(Attributes input)
{
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS.xyz = TransformObjectToWorldNormal(input.tangentOS.xyz);
    output.tangentWS.w = input.tangentOS.w;

    output.uv = input.uv;
    return output;
}

float3 overlay(float3 a, float3 b)
{
    return a < 0.5 ? 2 * a * b : 1 - 2 * (1 - a) * (1 - b);
}

half4 UberLitPassFragment(Varyings input) : SV_Target
{
    input.normalWS = normalize(input.normalWS);
    input.tangentWS = normalize(input.tangentWS);

    float3x3 tsMatrix = CreateTangentToWorld(input.normalWS, input.tangentWS.xyz, input.tangentWS.w);

    half4 sample = SAMPLE_TEX(_Albedo, input.uv);
    half3 albedo = sample.rgb;
    half alpha = sample.a * _Alpha;
    
    clipDither(alpha, input.positionCS);
    
    half3 tintMask = SAMPLE_TEX(_Tint_Mask, input.uv).rgb;

    albedo = lerp(albedo, overlay(albedo, _Tint_Color.rgb), tintMask);

    InputData inputData = (InputData)0;
    inputData.positionCS = input.positionCS;
    inputData.positionWS = input.positionWS;
    inputData.normalWS = input.normalWS;
    inputData.bakedGI = unity_AmbientSky.rgb;
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    half3 normalTS = UnpackNormalScale(SAMPLE_TEX(_NormalMap, input.uv), _NormalStrength);
    half4 final = UniversalFragmentBlinnPhong(inputData, albedo, 0.0, 0.0, 0.0, 1.0, normalTS);

    return final;
}
