Shader "CG/Water"
{
    Properties
    {
        _CubeMap("Reflection Cube Map", Cube) = "" {}
        _NoiseScale("Texture Scale", Range(1, 100)) = 10 
        _TimeScale("Time Scale", Range(0.1, 5)) = 3 
        _BumpScale("Bump Scale", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"
                #include "CGRandom.cginc"

                #define DELTA 0.01

                // Declare used properties
                uniform samplerCUBE _CubeMap;
                uniform float _NoiseScale;
                uniform float _TimeScale;
                uniform float _BumpScale;

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos      : SV_POSITION;
                    float3 normal   : NORMAL;
                    float2 uv       : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    float4 tangent  : TANGENT;
                };

                // Returns the value of a noise function simulating water, at coordinates uv and time t
                float waterNoise(float2 uv, float t)
                {
                    float v1 = perlin3d(float3(0.5 * uv.x, 0.5 * uv.y, 0.5 * t));
                    float v2 = perlin3d(float3(uv.x, uv.y, t));
                    float v3 = perlin3d(float3(2 * uv.x, 2 * uv.y, 2 * t));
                    return (v1 + 0.5 * v2 + 0.2 * v3);
                }


                bumpMapData createBumpMapData(v2f input)
                {
                    bumpMapData i;
                    i.normal = input.normal;
                    i.tangent = input.tangent;
                    i.uv = input.uv;
                    i.du = DELTA;
                    i.dv = DELTA;
                    i.bumpScale = _BumpScale;
                    return i;
                }


                // Returns the world-space bump-mapped normal for the given bumpMapData and time t
                float3 getWaterBumpMappedNormal(bumpMapData i, float t)
                {

                    float deriv_v = waterNoise(float2(i.uv.x, i.uv.y + i.dv), _Time.y * _TimeScale)- waterNoise(i.uv, _Time.y * _TimeScale);
                    deriv_v /= i.dv;
                    float deriv_u = waterNoise(float2(i.uv.x + i.du, i.uv.y), _Time.y * _TimeScale) - waterNoise(i.uv, _Time.y * _TimeScale);
                    deriv_u /= i.du;

                    float3 nh = normalize(float3(i.bumpScale * (-deriv_u), i.bumpScale * (-deriv_v), 1));
                    float3 b = i.tangent * i.normal;

                    return normalize(i.tangent * nh.x + i.normal * nh.z + b * nh.y);
                }


                v2f vert (appdata input)
                {
                    v2f output;

                    float noiseVal = waterNoise(_NoiseScale * input.uv, _Time.y * _TimeScale) * _BumpScale;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.pos.y -= noiseVal;
                    
                    output.worldPos = mul(unity_ObjectToWorld, input.vertex);
                    
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    
                    output.uv = _NoiseScale * input.uv;

                    output.tangent = mul(unity_ObjectToWorld, input.tangent);
                    
                    return output;
                }

                fixed4 frag(v2f input) : SV_Target
                {
                    
                    float noiseVal = waterNoise(_NoiseScale * input.uv, _Time.y * _TimeScale);
                    noiseVal = noiseVal * 0.5 + 0.5;

                    float3 n = getWaterBumpMappedNormal(createBumpMapData(input), 0);
                    
                    float3 v = normalize(_WorldSpaceCameraPos - input.worldPos);

                    float nvDotPro = dot(n, v);
                    
                    float3 reflectedVector = 2 * nvDotPro * n - v;
                    
                    half4 reflectedColor = texCUBE(_CubeMap, reflectedVector);

                    fixed4 finalColor = (1 - max(0, nvDotPro) + 0.2) * reflectedColor;
                    return finalColor;
                }

            ENDCG
        }
    }
}
