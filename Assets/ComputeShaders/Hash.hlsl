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
inline uint Hash_UInt1Base(uint value)
{
    const uint state = value * 747796405u + 2891336453u;
    const uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

inline uint2 Hash_UInt2Base(uint2 value)
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

inline uint3 Hash_UInt3Base(uint3 value)
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

inline uint4 Hash_UInt4Base(uint4 value)
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

inline uint Hash_UInt1(uint value)
{
    return Hash_UInt1Base(value);
}
inline uint Hash_UInt1(uint2 value)
{
    return Hash_UInt2Base(value).y;
}
inline uint Hash_UInt1(uint3 value)
{
    return Hash_UInt3Base(value).z;
}
inline uint Hash_UInt1(uint4 value)
{
    return Hash_UInt4Base(value).w;
}

inline uint2 Hash_UInt2(uint value)
{
    return Hash_UInt2Base(uint2(value.x, ~value.x));
}
inline uint2 Hash_UInt2(uint2 value)
{
    return Hash_UInt2Base(value);
}
inline uint2 Hash_UInt2(uint3 value)
{
    return Hash_UInt3Base(value).yz;
}
inline uint2 Hash_UInt2(uint4 value)
{
    return Hash_UInt4Base(value).zw;
}

inline uint3 Hash_UInt3(uint value)
{
    return Hash_UInt3Base(uint3(value.x, ~value.x, value.x << 1));
}
inline uint3 Hash_UInt3(uint2 value)
{
    return Hash_UInt3Base(uint3(value.x, value.y, ~value.x));
}
inline uint3 Hash_UInt3(uint3 value)
{
    return Hash_UInt3Base(value);
}
inline uint3 Hash_UInt3(uint4 value)
{
    return Hash_UInt4Base(value).yzw;
}

inline uint4 Hash_UInt4(uint value)
{
    return Hash_UInt4Base(uint4(value.x, ~value.x, value.x << 1, ~value.x << 1));
}
inline uint4 Hash_UInt4(uint2 value)
{
    return Hash_UInt4Base(uint4(value.x, value.y, ~value.x, ~value.y));
}
inline uint4 Hash_UInt4(uint3 value)
{
    return Hash_UInt4Base(uint4(value.x, value.y, value.z, ~value.x));
}
inline uint4 Hash_UInt4(uint4 value)
{
    return Hash_UInt4Base(value);
}

// float => uint
inline uint Hash_UInt1(float value)
{
    return Hash_UInt1(asuint(value));
}
inline uint Hash_UInt1(float2 value)
{
    return Hash_UInt1(asuint(value));
}
inline uint Hash_UInt1(float3 value)
{
    return Hash_UInt1(asuint(value));
}
inline uint Hash_UInt1(float4 value)
{
    return Hash_UInt1(asuint(value));
}

inline uint2 Hash_UInt2(float value)
{
    return Hash_UInt2(asuint(value));
}
inline uint2 Hash_UInt2(float2 value)
{
    return Hash_UInt2(asuint(value));
}
inline uint2 Hash_UInt2(float3 value)
{
    return Hash_UInt2(asuint(value));
}
inline uint2 Hash_UInt2(float4 value)
{
    return Hash_UInt2(asuint(value));
}

inline uint3 Hash_UInt3(float value)
{
    return Hash_UInt3(asuint(value));
}
inline uint3 Hash_UInt3(float2 value)
{
    return Hash_UInt3(asuint(value));
}
inline uint3 Hash_UInt3(float3 value)
{
    return Hash_UInt3(asuint(value));
}
inline uint3 Hash_UInt3(float4 value)
{
    return Hash_UInt3(asuint(value));
}

inline uint4 Hash_UInt4(float value)
{
    return Hash_UInt4(asuint(value));
}
inline uint4 Hash_UInt4(float2 value)
{
    return Hash_UInt4(asuint(value));
}
inline uint4 Hash_UInt4(float3 value)
{
    return Hash_UInt4(asuint(value));
}
inline uint4 Hash_UInt4(float4 value)
{
    return Hash_UInt4(asuint(value));
}

// uint => float[0, 1)
inline float Hash_Float1(uint value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(uint2 value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(uint3 value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(uint4 value)
{
    return UInt2Float01(Hash_UInt1(value));
}

inline float2 Hash_Float2(uint value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(uint2 value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(uint3 value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(uint4 value)
{
    return UInt2Float01(Hash_UInt2(value));
}

inline float3 Hash_Float3(uint value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(uint2 value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(uint3 value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(uint4 value)
{
    return UInt2Float01(Hash_UInt3(value));
}

inline float4 Hash_Float4(uint value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(uint2 value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(uint3 value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(uint4 value)
{
    return UInt2Float01(Hash_UInt4(value));
}

// float => float[0, 1)
inline float Hash_Float1(float value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(float2 value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(float3 value)
{
    return UInt2Float01(Hash_UInt1(value));
}
inline float Hash_Float1(float4 value)
{
    return UInt2Float01(Hash_UInt1(value));
}

inline float2 Hash_Float2(float value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(float2 value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(float3 value)
{
    return UInt2Float01(Hash_UInt2(value));
}
inline float2 Hash_Float2(float4 value)
{
    return UInt2Float01(Hash_UInt2(value));
}

inline float3 Hash_Float3(float value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(float2 value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(float3 value)
{
    return UInt2Float01(Hash_UInt3(value));
}
inline float3 Hash_Float3(float4 value)
{
    return UInt2Float01(Hash_UInt3(value));
}

inline float4 Hash_Float4(float value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(float2 value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(float3 value)
{
    return UInt2Float01(Hash_UInt4(value));
}
inline float4 Hash_Float4(float4 value)
{
    return UInt2Float01(Hash_UInt4(value));
}

// uint => range[min, max)
inline float Hash_Range1(uint value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(uint2 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(uint3 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(uint4 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}

inline float2 Hash_Range2(uint value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(uint2 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(uint3 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(uint4 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}

inline float3 Hash_Range3(uint value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(uint2 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(uint3 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(uint4 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}

inline float4 Hash_Range4(uint value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(uint2 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(uint3 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(uint4 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}

// float => range[min, max)
inline float Hash_Range1(float value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(float2 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(float3 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}
inline float Hash_Range1(float4 value, float min, float max)
{
    return Hash_Float1(value) * (max - min) + min;
}

inline float2 Hash_Range2(float value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(float2 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(float3 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}
inline float2 Hash_Range2(float4 value, float2 min, float2 max)
{
    return Hash_Float2(value) * (max - min) + min;
}

inline float3 Hash_Range3(float value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(float2 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(float3 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}
inline float3 Hash_Range3(float4 value, float3 min, float3 max)
{
    return Hash_Float3(value) * (max - min) + min;
}

inline float4 Hash_Range4(float value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(float2 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(float3 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}
inline float4 Hash_Range4(float4 value, float4 min, float4 max)
{
    return Hash_Float4(value) * (max - min) + min;
}


#endif /* CS_HASH_HLSL */