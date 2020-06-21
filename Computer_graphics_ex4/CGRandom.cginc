#ifndef CG_RANDOM_INCLUDED
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define CG_RANDOM_INCLUDED

// Returns a psuedo-random float between -1 and 1 for a given float c
float random(float c)
{
    return -1.0 + 2.0 * frac(43758.5453123 * sin(c));
}

// Returns a psuedo-random float2 with componenets between -1 and 1 for a given float2 c 
float2 random2(float2 c)
{
    c = float2(dot(c, float2(127.1, 311.7)), dot(c, float2(269.5, 183.3)));

    float2 v = -1.0 + 2.0 * frac(43758.5453123 * sin(c));
    return v;
}

// Returns a psuedo-random float3 with componenets between -1 and 1 for a given float3 c 
float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0*j);
    j *= .125;
    r.x = frac(512.0*j);
    j *= .125;
    r.y = frac(512.0*j);
    r = -1.0 + 2.0 * r;
    return r.yzx;
}

// Interpolates a given array v of 4 float2 values using bicubic interpolation
// at the given ratio t (a float2 with components between 0 and 1)
//
// [0]=====o==[1]
//         |
//         t
//         |
// [2]=====o==[3]
//
float bicubicInterpolation(float2 v[4], float2 t)
{
    float2 u = t * t * (3.0 - 2.0 * t); // Cubic interpolation

    // Interpolate in the x direction
    float x1 = lerp(v[0], v[1], u.x);
    float x2 = lerp(v[2], v[3], u.x);

    // Interpolate in the y direction and return
    return lerp(x1, x2, u.y);
}

// Interpolates a given array v of 4 float2 values using biquintic interpolation
// at the given ratio t (a float2 with components between 0 and 1)
float biquinticInterpolation(float2 v[4], float2 t)
{
    float2 u = t * t * t * ((6.0 * t - 15.0) * t + 10.0); // Cubic interpolation

    // Interpolate in the x direction
    float x1 = lerp(v[0], v[1], u.x);
    float x2 = lerp(v[2], v[3], u.x);

    // Interpolate in the y direction and return
    return lerp(x1, x2, u.y);
}

// Interpolates a given array v of 8 float3 values using triquintic interpolation
// at the given ratio t (a float3 with components between 0 and 1)
float triquinticInterpolation(float3 v[8], float3 t)
{
    float3 u = t * t * t * ((6.0 * t - 15.0) * t + 10.0); // Cubic interpolation

    // Interpolate in the x direction
    float x1 = lerp(v[0], v[1], u.x);
    float x2 = lerp(v[2], v[3], u.x);
    float x3 = lerp(v[4], v[5], u.x);
    float x4 = lerp(v[6], v[7], u.x);


    // Interpolate in the y direction
    float y1 = lerp(x1, x2, u.y);
    float y2 = lerp(x3, x4, u.y);

    // Interpolate in the z direction and return
    return lerp(y1, y2, u.z);
}

// Returns the value of a 2D value noise function at the given coordinates c
float value2d(float2 c)
{
    float2 x0 = float2(floor(c.x), ceil(c.y));
    float2 x1 = float2(ceil(c.x), ceil(c.y));
    float2 x2 = float2(floor(c.x), floor(c.y));
    float2 x3 = float2(ceil(c.x), floor(c.y));
    
    float2 t = float2(c.x-floor(c.x), ceil(c.y)-c.y);
    
    float2 rand0 = random2(x0);
    rand0.y = rand0.x;
    float2 rand1 = random2(x1);
    rand1.y = rand1.x;
    float2 rand2 = random2(x2);
    rand2.y = rand2.x;
    float2 rand3 = random2(x3);
    rand3.y = rand3.x;
    
    float2 v[4] = {rand0, rand1, rand2, rand3};
    
    return bicubicInterpolation(v,t);
}

// Returns the value of a 2D Perlin noise function at the given coordinates c
float perlin2d(float2 c)
{
    float2 x0 = float2(floor(c.x), ceil(c.y));
    float2 x1 = float2(ceil(c.x), ceil(c.y));
    float2 x2 = float2(floor(c.x), floor(c.y));
    float2 x3 = float2(ceil(c.x), floor(c.y));
    
    float2 randVec0 = random2(x0);
    float2 randVec1 = random2(x1);
    float2 randVec2 = random2(x2);
    float2 randVec3 = random2(x3);
    
    float2 distVec0 = c - x0;
    float2 distVec1 = c - x1;
    float2 distVec2 = c - x2;
    float2 distVec3 = c - x3;
    
    float2 value0 = float2(dot(distVec0, randVec0), dot(distVec0, randVec0));
    float2 value1 = float2(dot(distVec1, randVec1), dot(distVec1, randVec1));
    float2 value2 = float2(dot(distVec2, randVec2), dot(distVec2, randVec2));
    float2 value3 = float2(dot(distVec3, randVec3), dot(distVec3, randVec3));
    
    float2 t = float2(c.x-floor(c.x), ceil(c.y)-c.y);

    float2 v[4] = {value0, value1, value2, value3};
    
    return biquinticInterpolation(v,t);
}

// Returns the value of a 3D Perlin noise function at the given coordinates c
float perlin3d(float3 c)
{                    
    float3 x0 = float3(floor(c.x), ceil(c.y), floor(c.z));
    float3 x1 = float3(ceil(c.x), ceil(c.y), floor(c.z));
    float3 x2 = float3(floor(c.x), floor(c.y), floor(c.z));
    float3 x3 = float3(ceil(c.x), floor(c.y), floor(c.z));
    float3 x4 = float3(floor(c.x), ceil(c.y), ceil(c.z));
    float3 x5 = float3(ceil(c.x), ceil(c.y), ceil(c.z));
    float3 x6 = float3(floor(c.x), floor(c.y), ceil(c.z));
    float3 x7 = float3(ceil(c.x), floor(c.y), ceil(c.z));


    float3 randVec0 = random3(x0);
    float3 randVec1 = random3(x1);
    float3 randVec2 = random3(x2);
    float3 randVec3 = random3(x3);
    float3 randVec4 = random3(x4);
    float3 randVec5 = random3(x5);
    float3 randVec6 = random3(x6);
    float3 randVec7 = random3(x7);

    float3 distVec0 = c - x0;
    float3 distVec1 = c - x1;
    float3 distVec2 = c - x2;
    float3 distVec3 = c - x3;
    float3 distVec4 = c - x4;
    float3 distVec5 = c - x5;
    float3 distVec6 = c - x6;
    float3 distVec7 = c - x7;

    float3 dot0 = float3(dot(distVec0, randVec0), dot(distVec0, randVec0), dot(distVec0, randVec0));
    float3 dot1 = float3(dot(distVec1, randVec1), dot(distVec1, randVec1), dot(distVec1, randVec1));
    float3 dot2 = float3(dot(distVec2, randVec2), dot(distVec2, randVec2), dot(distVec2, randVec2));
    float3 dot3 = float3(dot(distVec3, randVec3), dot(distVec3, randVec3), dot(distVec3, randVec3));
    float3 dot4 = float3(dot(distVec4, randVec4), dot(distVec4, randVec4), dot(distVec4, randVec4));
    float3 dot5 = float3(dot(distVec5, randVec5), dot(distVec5, randVec5), dot(distVec5, randVec5));
    float3 dot6 = float3(dot(distVec6, randVec6), dot(distVec6, randVec6), dot(distVec6, randVec6));
    float3 dot7 = float3(dot(distVec7, randVec7), dot(distVec7, randVec7), dot(distVec7, randVec7));

    float3 t = float3(c.x - floor(c.x), ceil(c.y) - c.y, c.z - floor(c.z));

    float3 v[8] = { dot0, dot1, dot2, dot3 , dot4, dot5, dot6, dot7};

    return triquinticInterpolation(v, t);
}


#endif // CG_RANDOM_INCLUDED
