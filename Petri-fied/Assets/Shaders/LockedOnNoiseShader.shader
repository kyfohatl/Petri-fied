Shader "Unlit/LockedOnNoiseShader"
{
     Properties
    {
        _OffsetX ("X offset", Range(0.0, 100)) = 0.5
        _OffsetY ("Y offset", Range(0.0, 100)) = 0.5
        _Scale ("Noise scale", Range(0.0, 100)) = 3.5
        _AdditionalOffset ("Additional Vertex Offset", Range(0.5, 3.0)) = 1.0
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0.5,0.5,0.5,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _InsideColor ("Inside Color", Color) = (0,0,0,1)
        _MinOutlineThickness ("Min Outline Thickness", Range(1.05, 3.0)) = 1.1
        _MaxOutlineThickness ("Max Outline Thickness", Range(1.05, 3.0)) = 1.2
        _IsInvincible ("Invicibility Effect", Float) = 0.0
        _IsSpeed ("Speed Powerup Effect", Float) = 0.0
        _IsMagnet ("Magnet Powerup Effect", Float) = 0.0
        _DefaultColorMult ("The Multiplier For Each RGB Of The Default Color", Vector) = (0.666, 0.666, 0.666, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // The outline pass
        Pass
        {
            // We do not want to store z-buffer depth information because we want to have actual object
            // render over the outline
            ZWrite off
            // Ensure that the fragments are always drawn, even if they are behind other fragments.
            // This will be overwritten by the next pass where the actual object is rendered over the 
            // outline. But, when the actual object is obstrcuted, it will not be rendered while the 
            // outline will be
            ZTest always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "PerlinNoise.cginc"

            // For the outline, we do not care what color the incoming vertex is, since we will just
            // override with our outline color
            struct vertIn
            {
                float4 vertex : POSITION;
            };


            struct vertOut
            {
                float4 vertex : POSITION;
                // This sets the fogcoord which is needed for the fog effect
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float _MinOutlineThickness;
            float _MaxOutlineThickness;
            float4 _OutlineColor;
            float _OffsetX;
            float _OffsetY;
            float _Scale;
            float _AdditionalOffset;

            vertOut vert (vertIn v)
            {
                float perlinNoise = perlin3d(_Scale * v.vertex + float3(_Time.y + _OffsetX, _OffsetY, 0.0f));

                // Multiply incoming vertex by some value between min and maximum outline thickness to 
                // scale the outline to be larger than the object itself, and also to make the outline
                // pulsate
                v.vertex.xyz *= ((perlinNoise + _AdditionalOffset) * (_MinOutlineThickness + ((_MaxOutlineThickness - _MinOutlineThickness) * (_SinTime.w + 1.0) / 2.0)));

                vertOut o;
                // Now translate the vertex into world space
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Fog effect
                UNITY_TRANSFER_FOG(o,o.vertex);

                return o;
            }

            fixed4 frag (vertOut i) : COLOR
            {
                // Set the color to be the outline color
                fixed4 col = _OutlineColor * 10;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // The surface noise pass
        Pass
        {
            ZWrite on
            Lighting on

            CGPROGRAM
            #include "SurfaceNoise.cginc"
            ENDCG
        }
    }
}
