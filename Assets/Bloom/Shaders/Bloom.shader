Shader "Hidden/Bloom"
{
    Properties
    {
        _MainTex ("Bloom", 2D) = "black" {}
        _SourceTex ("Source", 2D) = "black" {}
        _NoiseTex ("Noise", 2D) = "grey" {}
    }

    SubShader
    {
        ZTest Always Cull Off ZWrite Off

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

        struct VertexBlurOutput
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 neighbours[4] : TEXCOORD1;
        };

        sampler2D_half _MainTex;
        sampler2D_half _SourceTex;
        sampler2D_half _NoiseTex;
        float2 _TexelSize;
        float _Intensity;
        float _Threshold;
        float3 _Curve;
        float2 _NoiseTexScale;

        half4 BoxFilter (sampler2D tex, VertexBlurOutput IN)
        {
            half4 sum = 0.0;
            UNITY_UNROLL for (int i = 0; i < 4; i++)
                sum += tex2D(tex, IN.neighbours[i]);
            sum *= 0.25;
            return sum;
        }

        float GetNoise (float2 uv)
        {
            float noise = tex2D(_NoiseTex, uv * _NoiseTexScale).a;
            noise = noise * 2.0 - 1.0;
            return noise / 255.0;
        }

        VertexOutput vert_base (VertexInput IN)
        {
            VertexOutput OUT;
            OUT.vertex = UnityObjectToClipPos(IN.vertex);
            OUT.uv = IN.uv;
            return OUT;
        }

        VertexBlurOutput vert_blur (VertexInput IN)
        {
            VertexBlurOutput OUT;
            OUT.vertex = UnityObjectToClipPos(IN.vertex);

            OUT.uv = IN.uv;
            OUT.neighbours[0] = IN.uv - float2(_TexelSize.x, 0.0);
            OUT.neighbours[1] = IN.uv + float2(_TexelSize.x, 0.0);
            OUT.neighbours[2] = IN.uv - float2(0.0, _TexelSize.y);
            OUT.neighbours[3] = IN.uv + float2(0.0, _TexelSize.y);

            return OUT;
        }

        half4 frag_prefilter (VertexOutput IN) : SV_Target
        {
            half4 c = tex2D(_MainTex, IN.uv);
            half br = max(c.r, max(c.g, c.b));
            half rq = clamp(br - _Curve.x, 0.0, _Curve.y);
            rq = _Curve.z * rq * rq;
            c.rgb *= max(rq, br - _Threshold) / max(br, 0.0001);
            return half4(c.rgb, c.a);
        }

        half4 frag_blur (VertexBlurOutput IN) : SV_Target
        {
            return BoxFilter(_MainTex, IN);
        }

        half4 frag_final (VertexBlurOutput IN) : SV_Target
        {
            half4 bloom = BoxFilter(_MainTex, IN);
            return bloom * _Intensity;
        }

        half4 frag_combine (VertexBlurOutput IN) : SV_Target
        {
            half3 bloom = tex2D(_MainTex, IN.uv).rgb;
            bloom += GetNoise(IN.uv);
            bloom = LinearToGammaSpace(bloom);
            half4 source = tex2D(_SourceTex, IN.uv);
            return half4(source.rgb + bloom, source.a);
        }

        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_base
            #pragma fragment frag_prefilter
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
            ENDCG
        }

        Pass
        {
            Blend One One
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_blur
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_final
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur
            #pragma fragment frag_combine
            ENDCG
        }
    }
}