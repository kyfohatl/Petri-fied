Shader "Unlit/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0.5,0.5,0.5,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(1.05, 7.0)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent+1" }
        LOD 100

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
            float4 _MainTex_ST;
            float _OutlineThickness;
            float4 _OutlineColor;

            vertOut vert (vertIn v)
            {

                // Multiply incoming vertex by outline tickness to scale the outline to be larger than the
                // object itself
                v.vertex.xyz *= _OutlineThickness;

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
                fixed4 col = _OutlineColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        Pass {
            // We need to store z-buffer depth information so that the actual object renders over the 
            // outline object
            ZWrite on

            Lighting on

            Material {
                Diffuse[_MainColor]
                Ambient[_MainColor]
            }

            SetTexture[_MainTex] {
                ConstantColor[_MainColor]
            }

            SetTexture[_MainTex] {
                Combine previous * primary DOUBLE
            }
        }
    }
}
