Shader "Custom/SilhouetteCapture"
{
    // Renders the object as pure black on a white background.
    // Used by the shadow capture camera via RenderWithShader().
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Object pixels = black
                return half4(0, 0, 0, 1);
            }
            ENDHLSL
        }
    }

    // Fallback so the camera clear colour (white) shows through
    // for anything NOT using this RenderType tag.
    FallBack Off
}
