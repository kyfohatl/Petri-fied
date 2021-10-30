Shader "Unlit/SurfaceNoise"
{
    Properties
    {
        _OffsetX ("X offset", Range(0.0, 100)) = 0.5
        _OffsetY ("Y offset", Range(0.0, 100)) = 0.5
        _Scale ("Noise scale", Range(0.0, 100)) = 3.5
        _AdditionalOffset ("Additional Vertex Offset", Range(0.5, 3.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "SurfaceNoise.cginc"
            ENDCG
        }
    }
}
