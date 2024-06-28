#ifndef CS_HASH_HLSL
#define CS_HASH_HLSL

// Permuted Congruential Generator
// Original source code: https://www.shadertoy.com/view/XlGcRh

// uint => float[0, 1)
inline float UInt2Float01(uint value)
{
    return (value & 0x00ffffffu) * asfloat(0x33800000);
}
inline float2 UInt2Float01(uint2 value)
{
    return (value & 0x00ffffffu) * asfloat(0x33800000);
}
inline float3 UInt2Float01(uint3 value)
{
    return (value & 0x00ffffffu) * asfloat(0x33800000);
}
inline float4 UInt2Float01(uint4 value)
{
    return (value & 0x00ffffffu) * asfloat(0x33800000);
}

// uint => uint
inline uint Hash1Base(uint value)
{
    const uint state = value * 747796405u + 2891336453u;
    const uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

inline uint2 Hash2Base(uint2 value)
{
    uint2 v = value;

    v = v * 1664525u + 1013904223u;

    v.x += v.y * 1664525u;
    v.y += v.x * 1664525u;

    v ^= v >> 16u;

    v.x += v.y * 1664525u;
    v.y += v.x * 1664525u;

    v ^= v >> 16u;

    return v;
}

inline uint3 Hash3Base(uint3 value)
{
    uint3 v = value;

    v = v * 1664525u + 1013904223u;

    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;

    v ^= v >> 16u;

    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;

    return v;
}

inline uint4 Hash4Base(uint4 value)
{
    uint4 v = value;

    v = v * 1664525u + 1013904223u;

    v.x += v.y * v.w;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    v.w += v.y * v.z;

    v ^= v >> 16u;

    v.x += v.y * v.w;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    v.w += v.y * v.z;

    return v;
}

inline uint Hash1(uint value)
{
    return Hash1Base(value);
}
inline uint Hash1(uint2 value)
{
    return Hash2Base(value).y;
}
inline uint Hash1(uint3 value)
{
    return Hash3Base(value).z;
}
inline uint Hash1(uint4 value)
{
    return Hash4Base(value).w;
}

inline uint2 Hash2(uint value)
{
    return Hash2Base(uint2(value.x, ~value.x));
}
inline uint2 Hash2(uint2 value)
{
    return Hash2Base(value);
}
inline uint2 Hash2(uint3 value)
{
    return Hash3Base(value).yz;
}
inline uint2 Hash2(uint4 value)
{
    return Hash4Base(value).zw;
}

inline uint3 Hash3(uint value)
{
    return Hash3Base(uint3(value.x, ~value.x, value.x << 1));
}
inline uint3 Hash3(uint2 value)
{
    return Hash3Base(uint3(value.x, value.y, ~value.x));
}
inline uint3 Hash3(uint3 value)
{
    return Hash3Base(value);
}
inline uint3 Hash3(uint4 value)
{
    return Hash4Base(value).yzw;
}

inline uint4 Hash4(uint value)
{
    return Hash4Base(uint4(value.x, ~value.x, value.x << 1, ~value.x << 1));
}
inline uint4 Hash4(uint2 value)
{
    return Hash4Base(uint4(value.x, value.y, ~value.x, ~value.y));
}
inline uint4 Hash4(uint3 value)
{
    return Hash4Base(uint4(value.x, value.y, value.z, ~value.x));
}
inline uint4 Hash4(uint4 value)
{
    return Hash4Base(value);
}

// float => uint
inline uint Hash1(float value)
{
    return Hash1(asuint(value));
}
inline uint Hash1(float2 value)
{
    return Hash1(asuint(value));
}
inline uint Hash1(float3 value)
{
    return Hash1(asuint(value));
}
inline uint Hash1(float4 value)
{
    return Hash1(asuint(value));
}

inline uint2 Hash2(float value)
{
    return Hash2(asuint(value));
}
inline uint2 Hash2(float2 value)
{
    return Hash2(asuint(value));
}
inline uint2 Hash2(float3 value)
{
    return Hash2(asuint(value));
}
inline uint2 Hash2(float4 value)
{
    return Hash2(asuint(value));
}

inline uint3 Hash3(float value)
{
    return Hash3(asuint(value));
}
inline uint3 Hash3(float2 value)
{
    return Hash3(asuint(value));
}
inline uint3 Hash3(float3 value)
{
    return Hash3(asuint(value));
}
inline uint3 Hash3(float4 value)
{
    return Hash3(asuint(value));
}

inline uint4 Hash4(float value)
{
    return Hash4(asuint(value));
}
inline uint4 Hash4(float2 value)
{
    return Hash4(asuint(value));
}
inline uint4 Hash4(float3 value)
{
    return Hash4(asuint(value));
}
inline uint4 Hash4(float4 value)
{
    return Hash4(asuint(value));
}

// uint => float[0, 1)
inline float Hash1_01(uint value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(uint2 value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(uint3 value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(uint4 value)
{
    return UInt2Float01(Hash1(value));
}

inline float2 Hash2_01(uint value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(uint2 value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(uint3 value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(uint4 value)
{
    return UInt2Float01(Hash2(value));
}

inline float3 Hash3_01(uint value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(uint2 value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(uint3 value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(uint4 value)
{
    return UInt2Float01(Hash3(value));
}

inline float4 Hash4_01(uint value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(uint2 value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(uint3 value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(uint4 value)
{
    return UInt2Float01(Hash4(value));
}

// float => float[0, 1)
inline float Hash1_01(float value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(float2 value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(float3 value)
{
    return UInt2Float01(Hash1(value));
}
inline float Hash1_01(float4 value)
{
    return UInt2Float01(Hash1(value));
}

inline float2 Hash2_01(float value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(float2 value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(float3 value)
{
    return UInt2Float01(Hash2(value));
}
inline float2 Hash2_01(float4 value)
{
    return UInt2Float01(Hash2(value));
}

inline float3 Hash3_01(float value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(float2 value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(float3 value)
{
    return UInt2Float01(Hash3(value));
}
inline float3 Hash3_01(float4 value)
{
    return UInt2Float01(Hash3(value));
}

inline float4 Hash4_01(float value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(float2 value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(float3 value)
{
    return UInt2Float01(Hash4(value));
}
inline float4 Hash4_01(float4 value)
{
    return UInt2Float01(Hash4(value));
}

// uint => range[min, max)
inline float Hash1_Range(uint value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(uint2 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(uint3 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(uint4 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}

inline float2 Hash2_Range(uint value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(uint2 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(uint3 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(uint4 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}

inline float3 Hash3_Range(uint value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(uint2 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(uint3 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(uint4 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}

inline float4 Hash4_Range(uint value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(uint2 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(uint3 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(uint4 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}

// float => range[min, max)
inline float Hash1_Range(float value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(float2 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(float3 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}
inline float Hash1_Range(float4 value, float min, float max)
{
    return Hash1_01(value) * (max - min) + min;
}

inline float2 Hash2_Range(float value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(float2 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(float3 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}
inline float2 Hash2_Range(float4 value, float2 min, float2 max)
{
    return Hash2_01(value) * (max - min) + min;
}

inline float3 Hash3_Range(float value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(float2 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(float3 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}
inline float3 Hash3_Range(float4 value, float3 min, float3 max)
{
    return Hash3_01(value) * (max - min) + min;
}

inline float4 Hash4_Range(float value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(float2 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(float3 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}
inline float4 Hash4_Range(float4 value, float4 min, float4 max)
{
    return Hash4_01(value) * (max - min) + min;
}


#endif /* CS_HASH_HLSL */