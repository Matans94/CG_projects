Shader "CG/Earth"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(1, 100)) = 30
        [NoScaleOffset] _CloudMap ("Cloud Map", 2D) = "black" {}
        _AtmosphereColor ("Atmosphere Color", Color) = (0.8, 0.85, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"

                // Declare used properties
                uniform sampler2D _AlbedoMap;
                uniform float _Ambient;
                uniform sampler2D _SpecularMap;
                uniform float _Shininess;
                uniform sampler2D _HeightMap;
                uniform float4 _HeightMap_TexelSize;
                uniform float _BumpScale;
                uniform sampler2D _CloudMap;
                uniform fixed4 _AtmosphereColor;

                struct appdata
                { 
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 vertex : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                   // float2 uv : TEXCOORD2;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.vertex = input.vertex;
                    output.worldPos = mul(unity_ObjectToWorld, input.vertex);
                    return output;
                }

                fixed4 frag(v2f input) : SV_Target
                {
                    float2 uv = getSphericalUV(input.vertex); 
                    
                    float3 v = normalize(_WorldSpaceCameraPos - input.worldPos);
                    float3 l = normalize(_WorldSpaceLightPos0);
                    fixed4 albedo = tex2D(_AlbedoMap, uv);
                    fixed4 specularity = tex2D(_SpecularMap, uv);
                    
                    bumpMapData i; 
                    i.normal = input.worldPos;
                    i.tangent = i.normal * float3(0,1,0); 
                    i.uv = uv;
                    i.heightMap = _HeightMap;
                    i.du = _HeightMap_TexelSize.x;
                    i.dv = _HeightMap_TexelSize.y;
                    i.bumpScale = _BumpScale / 10000;
                    
                    float3 bumpNormal = getBumpMappedNormal(i);
                    float3 finalNormal = (1-tex2D(_SpecularMap, uv).x) * bumpNormal + tex2D(_SpecularMap, uv).x * input.worldPos;
                    finalNormal = normalize(finalNormal);
                    
                    float lambert = max(0, dot(input.worldPos, l));
                    
                    fixed4 atmosphere = (1 - max(0, dot(input.worldPos, v)))*sqrt(lambert)*_AtmosphereColor;
                    fixed4 clouds = tex2D(_CloudMap, uv)*(sqrt(lambert)+_Ambient);
                    fixed3 finalColor = blinnPhong(finalNormal, v, l, _Shininess, albedo, specularity, _Ambient) + atmosphere+clouds ;
                    
                    
                    return fixed4(finalColor ,1);
   
                 
                }

            ENDCG
        }
    }
}
