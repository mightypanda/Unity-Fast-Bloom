Shader "Hidden/BloomDisplay"
{
    Properties
    {
        _BloomTex ("Bloom", 2D) = "black" {}
        _NoiseTex ("Noise", 2D) = "grey" {}
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Overlay"
        }

        ZTest Always Cull Off ZWrite Off
        Blend One One

        CGINCLUDE

        #include "UnityCG.cginc"

        struct VertexInput
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct VertexOutput
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        sampler2D_float _BloomTex;
        sampler2D_float _NoiseTex;
        float2 _NoiseTexScale;

        VertexOutput vert (VertexInput IN)
        {
            VertexOutput OUT;
            OUT.vertex = float4(IN.vertex.xy, 0.0, 1.0);
            OUT.uv = IN.uv;
            if (_ProjectionParams.x < 0)
                OUT.uv.y = 1 - OUT.uv.y;
            return OUT;
        }

        float GetNoise (float2 uv)
        {
            float noise = tex2D(_NoiseTex, uv * _NoiseTexScale).a;
            noise = noise * 2.0 - 1.0;
            return noise / 255.0;
        }

        float4 frag (VertexOutput IN) : SV_Target
        {
            float3 bloom = tex2D(_BloomTex, IN.uv).rgb;
            bloom += GetNoise(IN.uv);
            bloom = LinearToGammaSpace(bloom);
            return float4(bloom, 1.0);
        }

        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}