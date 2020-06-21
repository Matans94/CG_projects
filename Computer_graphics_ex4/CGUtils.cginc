﻿#ifndef CG_UTILS_INCLUDED
#define CG_UTILS_INCLUDED

#define PI 3.141592653

// A struct containing all the data needed for bump-mapping
struct bumpMapData
{ 
    float3 normal;       // Mesh surface normal at the point
    float3 tangent;      // Mesh surface tangent at the point
    float2 uv;           // UV coordinates of the point
    sampler2D heightMap; // Heightmap texture to use for bump mapping
    float du;            // Increment size for u partial derivative approximation
    float dv;            // Increment size for v partial derivative approximation
    float bumpScale;     // Bump scaling factor
};


// Receives pos in 3D cartesian coordinates (x, y, z)
// Returns UV coordinates corresponding to pos using spherical texture mapping
float2 getSphericalUV(float3 pos)
{
    float r = sqrt(pow(pos.x, 2) + pow(pos.y, 2) + pow(pos.z, 2));
    float theta = atan2(pos.z, pos.x);
    float phi = acos(pos.y / r);
    
    return float2((0.5 + theta) / (2 * PI), 1 - (phi / PI));
}

// Implements an adjusted version of the Blinn-Phong lighting model
fixed3 blinnPhong(float3 n, float3 v, float3 l, float shininess, fixed4 albedo, fixed4 specularity, float ambientIntensity)
{
    float3 h = normalize(l + v);
    fixed4 ambient = ambientIntensity * albedo;
    fixed4 diffuse = max(0, dot(n,l)) * albedo;
    fixed4 specular = pow(max(0,dot(n,h)), shininess) * specularity;
    return (ambient+diffuse+specular); 
}

// Returns the world-space bump-mapped normal for the given bumpMapData
float3 getBumpMappedNormal(bumpMapData i)
{
    float3 wsbmNormal;

    float deriv_v = tex2D(i.heightMap, float2(i.uv.x, i.uv.y + i.dv)) - tex2D(i.heightMap, i.uv);
    deriv_v /= i.dv;
    float deriv_u = (tex2D(i.heightMap, float2(i.uv.x + i.du, i.uv.y)) - tex2D(i.heightMap, i.uv));
    deriv_u /= i.du;
    
    float3 nh = normalize(float3(i.bumpScale * (-deriv_u), i.bumpScale * (-deriv_v), 1));
    float3 b = i.tangent * i.normal;
    
    return normalize(i.tangent*nh.x + i.normal*nh.z + b*nh.y);
}


#endif // CG_UTILS_INCLUDED
