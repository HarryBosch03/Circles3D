Shader "Hidden/HurtOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            float _HurtWeight;
            float4 _HurtColor;
            
            half4 frag(v2f i) : SV_Target
            {
                half f = lerp(4, 12, _HurtWeight);
                half a = f / 6;
                half alpha = pow(abs(i.uv * 2 - 1), 10 + a * sin(_Time.y * f)) * _HurtWeight;
                
                return float4(_HurtColor.rgb, saturate(_HurtColor.a * alpha));
            }
            ENDHLSL
        }
    }
}