Shader "Unlit/HueSlider"
{
    Properties {}
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float alpha : ALPHA;
                float4 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.uv = input.uv;
                output.alpha = input.color.a;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 hsv;
                hsv.r = input.uv.x;
                hsv.g = 1.0;
                hsv.b = 1.0;
                
                return float4(HsvToRgb(hsv), input.alpha);
            }
            ENDHLSL
        }
    }
}