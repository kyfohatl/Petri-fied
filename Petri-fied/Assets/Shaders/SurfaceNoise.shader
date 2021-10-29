

Shader "Unlit/SurfaceNoise"
{
    Properties
    {
        _OffsetX ("X offset", Range(0.0, 100)) = 0.5
        _OffsetY ("Y offset", Range(0.0, 100)) = 0.5
        _Scale ("Noise scale", Range(0.0, 100)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            static const int gradCount = 12;

            // Define an array of gradients we can assign to each point on the grid
            // We use gradient vectors with 1's because they do not require multipication
            // to calculate dot products with
            static const float3 gradients[] = {
                float3(1,1,0),float3(-1,1,0),float3(1,-1,0),float3(-1,-1,0),
                float3(1,0,1),float3(-1,0,1),float3(1,0,-1),float3(-1,0,-1),
                float3(0,1,1),float3(0,-1,1),float3(0,1,-1),float3(0,-1,-1)
            };

            // The numbers from 0 to 255, shuffled in an array, and then duplicated.
            // These values will be used to assing a psuedo-random value to each grid point
            // in the 3D space
            // The duplication is to prevent index overflow when accessing the array using our 
            // hashing function, as we will see later
            // This is the original permutations array used by Ken Perlin, and is still commonly
            // used in many implementations of the algorithm
            static const int permutations[] = {
                151,160,137,91,90,15,
                131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
                
                151,160,137,91,90,15,
                131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
                190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
                88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
                77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
                102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
                135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
                5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
                223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
                129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
                251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
                49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
                138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

            // Smooths the given value using 5th degree polynomial function
            float fade(float t) {
                return t*t*t*(t*(t*6.0f-15.0f)+10.0f);
            }
                
            float perlin3d(float3 inputPoint) {
                // Find the base point: The bottom corner point on the grid cube which
                // contains our point
                int X = int(floor(inputPoint.x));
                int Y = int(floor(inputPoint.y));
                int Z = int(floor(inputPoint.z));
                
                // Next find the position of the point within the grid cube
                float x = inputPoint.x - float(X);
                float y = inputPoint.y - float(Y);
                float z = inputPoint.z - float(Z);
                
                // We will use coordinates of the 8 points of the cube containing out point
                // to pick a number from the permutations array for each point
                // Since the size of the permutations array is 255 * 2, we must wrap the 
                // base point coordinates to prevent index overflow
                X %= 255;
                Y %= 255;
                Z %= 255;
                
                // Pick a number from the permutations array for each corner point
                int gradientIndex000 = permutations[X + permutations[Y + permutations[Z]]] % gradCount;
                int gradientIndex001 = permutations[X + permutations[Y + permutations[Z + 1]]] % gradCount;
                int gradientIndex010 = permutations[X + permutations[Y + 1 + permutations[Z]]] % gradCount;
                int gradientIndex011 = permutations[X + permutations[Y + 1 + permutations[Z + 1]]] % gradCount;
                int gradientIndex100 = permutations[X + 1 + permutations[Y + permutations[Z]]] % gradCount;
                int gradientIndex101 = permutations[X + 1 + permutations[Y + permutations[Z + 1]]] % gradCount;
                int gradientIndex110 = permutations[X + 1 + permutations[Y + 1 + permutations[Z]]] % gradCount;
                int gradientIndex111 = permutations[X + 1 + permutations[Y + 1 + permutations[Z + 1]]] % gradCount;
                
                // Use the number for each corner point to pick a gradient for it from the 
                // gradients array
                float3 gradient000 = gradients[gradientIndex000];
                float3 gradient001 = gradients[gradientIndex001];
                float3 gradient010 = gradients[gradientIndex010];
                float3 gradient011 = gradients[gradientIndex011];
                float3 gradient100 = gradients[gradientIndex100];
                float3 gradient101 = gradients[gradientIndex101];
                float3 gradient110 = gradients[gradientIndex110];
                float3 gradient111 = gradients[gradientIndex111];
                
                // Find the vector from each corner point to our input point
                float3 corner000ToPoint = float3(x, y, z);
                float3 corner001ToPoint = float3(x, y, z-1.0f);
                float3 corner010ToPoint = float3(x, y-1.0f, z);
                float3 corner011ToPoint = float3(x, y-1.0f, z-1.0f);
                float3 corner100ToPoint = float3(x-1.0f, y, z);
                float3 corner101ToPoint = float3(x-1.0f, y, z-1.0f);
                float3 corner110ToPoint = float3(x-1.0f, y-1.0f, z);
                float3 corner111ToPoint = float3(x-1.0f, y-1.0f, z-1.0f);
                
                // For each corner point of the cube, calculate the dot product between the 
                // gradient vector and the vector from the point to our input point, to get 
                // a value for each corner point
                float dotValue000 = dot(gradient000, corner000ToPoint);
                float dotValue001 = dot(gradient001, corner001ToPoint);
                float dotValue010 = dot(gradient010, corner010ToPoint);
                float dotValue011 = dot(gradient011, corner011ToPoint);
                float dotValue100 = dot(gradient100, corner100ToPoint);
                float dotValue101 = dot(gradient101, corner101ToPoint);
                float dotValue110 = dot(gradient110, corner110ToPoint);
                float dotValue111 = dot(gradient111, corner111ToPoint);
            
                // Smooth out the relative coordinates of our point before interpolating
                float u = fade(x);
                float v = fade(y);
                float w = fade(z);

                // Now interpolate pairs of vlues using the smoothed relative x
                float bottomFrontX = lerp(dotValue000, dotValue100, u);
                float bottomBackX = lerp(dotValue001, dotValue101, u);
                float topFrontX = lerp(dotValue010, dotValue110, u);
                float topBackX = lerp(dotValue011, dotValue111, u);

                // Then interpolate the resulting vlues using the smoothed relative y
                float frontY = lerp(bottomFrontX, topFrontX, v);
                float backY = lerp(bottomBackX, topBackX, v);

                // Finally interpolate the resulting vlues using the smoothed relative z to 
                // get the final noise value
                float finalNoiseValue = lerp(frontY, backY, w);
                
                return finalNoiseValue;
            }

            float _OffsetX;
            float _OffsetY;
            float _Scale;

            struct vertIn
            {
                float4 vertex : POSITION;
            };

            struct vertOut
            {
                float4 vertex : SV_POSITION;
                float2 noise : TEXCOORD0;
            };

            vertOut vert (vertIn v)
            {
                vertOut o;
                float4 scaledPoint = _Scale * v.vertex;
                float perlinNoise = perlin3d(scaledPoint.xyz + float3(_Time.y + _OffsetX, _OffsetY, 0.0f)) + 1.0f;
                v.vertex *= perlinNoise;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.noise.x = perlinNoise;
                return o;
            }

            fixed4 frag (vertOut i) : SV_Target
            {
                return fixed4((_SinTime.w + 1.0f) / 3.0f, i.noise.x , i.noise.x, 1);
            }
            ENDCG
        }
    }
}
