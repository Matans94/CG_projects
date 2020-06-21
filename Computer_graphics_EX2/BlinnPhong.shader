Shader "CG/BlinnPhong"
{
    Properties
    {
        _DiffuseColor ("Diffuse Color", Color) = (0.14, 0.43, 0.84, 1)
        _SpecularColor ("Specular Color", Color) = (0.7, 0.7, 0.7, 1)
        _AmbientColor ("Ambient Color", Color) = (0.05, 0.13, 0.25, 1)
        _Shininess ("Shininess", Range(0.1, 50)) = 10
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

                // From UnityCG
                uniform fixed4 _LightColor0; 

                // Declare used properties
                uniform fixed4 _DiffuseColor;
                uniform fixed4 _SpecularColor;
                uniform fixed4 _AmbientColor;
                uniform float _Shininess;

                struct appdata
                { 
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 normal : NORMAL;
                    float3 posWorld : TEXCOORD1;

                   // fixed4 color : COLOR;
                   // float4 texture : TEXCOORD0;
                };


                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.posWorld = normalize(mul(unity_ObjectToWorld, input.vertex).xyz);
                    output.normal = input.normal;
                    return output;
                }


                fixed4 frag(v2f input) : SV_Target
                {
                    input.normal = normalize(mul(unity_ObjectToWorld, input.normal));
                    float3 light = normalize((_WorldSpaceLightPos0).xyz);
                    fixed4 color_a = _AmbientColor * _LightColor0;

                    float cos_tetha = dot(light, input.normal);
                    fixed4 color_d = (cos_tetha > 0) ? cos_tetha * _DiffuseColor * _LightColor0 : fixed4(0,0,0,0);

                    float3 v = normalize(_WorldSpaceCameraPos - input.posWorld);
                    float3 h = normalize(light + v);
                    float cos_beta = dot(h, input.normal);
                    fixed4 color_s = (cos_beta > 0) ? pow(cos_beta, _Shininess) * _SpecularColor * _LightColor0 : fixed4(0, 0, 0, 0);
                    fixed4 final = color_a + color_d + color_s;
                    //final.w = 1;
                    return (final);
                }

            ENDCG
        }
    }
}
