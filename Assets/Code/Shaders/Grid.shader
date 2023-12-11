Shader "Unlit/Grid"
{
    Properties
    {
        _Opacity("Opacity", Range(0.0, 1.0)) = 0.4
        _Width("Line Width", Range(0.0, 1.0)) = 0.1
        _Size("Grid Size", float) = 1.0
        _InnerRadius("Inner Radius", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : POSITION_WS;
                float3 normalWS : NORMAL_WS;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.positionWS = TransformObjectToWorld(input.vertex.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normal.xyz);
                output.uv = input.uv;

                return output;
            }

            float _Width, _Size;
            float _InnerRadius;

            float mask(float2 uv)
            {
                float v = 1.0 - length(uv * 2.0 - 1.0);
                v = saturate(v / (1.0 - _InnerRadius));
                v = smoothstep(0.0, 1.0, v);
                return saturate(v);
            }

            float _Opacity;
            
            half4 frag(Varyings input) : SV_Target
            {
                input.normalWS = normalize(input.normalWS);

                float a = _Opacity;
                a *= mask(input.uv);

                float3 lines = input.positionWS * _Size;
                lines = (lines % 1.0 + 1.0) % 1.0;
                lines = abs(lines * 2.0 - 1.0);

                float3 weights = 1.0 - pow(abs(input.normalWS), 128.0);
                a *= saturate
                (
                    (lines.x < _Width) * weights.x +
                    (lines.y < _Width) * weights.y +
                    (lines.z < _Width) * weights.z
                );

                return float4(1.0, 1.0, 1.0, a);
            }
            ENDHLSL
        }
    }
}