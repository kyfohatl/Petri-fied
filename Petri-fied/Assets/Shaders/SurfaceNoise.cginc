#pragma vertex vert
#pragma fragment frag
// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"
#include "PerlinNoise.cginc"

float _OffsetX;
float _OffsetY;
float _Scale;
float _AdditionalOffset;

struct vertIn
{
    float4 vertex : POSITION;
};

struct vertOut
{
    float4 vertex : SV_POSITION;
    float2 noise : TEXCOORD0;
    // This sets the fogcoord which is needed for the fog effect
    UNITY_FOG_COORDS(1)
};

vertOut vert (vertIn v)
{
    vertOut o;
    float perlinNoise = perlin3d(_Scale * v.vertex + float3(_Time.y + _OffsetX, _OffsetY, 0.0f));
    v.vertex *= perlinNoise + _AdditionalOffset;
    o.noise.x = (perlinNoise + 1.0f) / 2.0f;
    o.vertex = UnityObjectToClipPos(v.vertex);
    // Fog effect
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}

fixed4 frag (vertOut i) : SV_Target
{
    fixed4 color = fixed4((_SinTime.w + 1.0f) / 2.0f, i.noise.x , i.noise.x, 1);

    // apply fog
    UNITY_APPLY_FOG(i.fogCoord, color);

    return color;
}