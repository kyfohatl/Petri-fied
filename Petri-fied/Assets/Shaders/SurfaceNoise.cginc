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
float _IsInvincible;
float _IsSpeed;
float _IsMagnet;
Vector _DefaultColorMult;

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
    fixed4 color = fixed4(i.noise.x * _DefaultColorMult.x, i.noise.x * _DefaultColorMult.y , i.noise.x * _DefaultColorMult.z, 1);
    //fixed4(i.noise.x / 1.5f, i.noise.x / 1.5f , i.noise.x / 1.5f, 1);

    if (_IsInvincible > 0.1) {
        color = fixed4(i.noise.x / 1.5f, (sin(_Time.w * 1.7f) + 1.0f) / 2.5f  , i.noise.x / 1.5f, 1);
    }
    if (_IsSpeed > 0.1) {
        color = fixed4((sin(_Time.w * 1.7f) + 1.0f) / 2.5f, i.noise.x / 1.5f , i.noise.x / 1.5f, 1);
    }
    if (_IsMagnet > 0.1) {
        color = fixed4(i.noise.x / 1.3f, i.noise.x / 1.3f , (_SinTime.w + 1.0f) / 2.0f, 1);
    }

    // Other experimental colors
    //fixed4((cos(_Time.y + 1.5f) + 1.0f) / 2.0f, (_SinTime.w + 1.0f) / 2.0f, i.noise.x / 2.0, 1);
    //fixed4((_SinTime.w + 1.0f) / 3.0f, (_SinTime.w + 1.0f) / 3.0f, i.noise.x / 3.0, 1);

    // apply fog
    UNITY_APPLY_FOG(i.fogCoord, color);

    return color;
}