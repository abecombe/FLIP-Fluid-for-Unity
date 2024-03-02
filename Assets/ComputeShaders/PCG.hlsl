#ifndef CS_PCG_HLSL
#define CS_PCG_HLSL

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
inline uint PCG1Base(uint value)
{
    const uint state = value * 747796405u + 2891336453u;
    const uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

inline uint2 PCG2Base(uint2 value)
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

inline uint3 PCG3Base(uint3 value)
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

inline uint4 PCG4Base(uint4 value)
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

inline uint PCG1(uint value)
{
    return PCG1Base(value);
}
inline uint PCG1(uint2 value)
{
    return PCG2Base(value).y;
}
inline uint PCG1(uint3 value)
{
    return PCG3Base(value).z;
}
inline uint PCG1(uint4 value)
{
    return PCG4Base(value).w;
}

inline uint2 PCG2(uint value)
{
    return PCG2Base(uint2(value.x, ~value.x));
}
inline uint2 PCG2(uint2 value)
{
    return PCG2Base(value);
}
inline uint2 PCG2(uint3 value)
{
    return PCG3Base(value).yz;
}
inline uint2 PCG2(uint4 value)
{
    return PCG4Base(value).zw;
}

inline uint3 PCG3(uint value)
{
    return PCG3Base(uint3(value.x, ~value.x, value.x << 1));
}
inline uint3 PCG3(uint2 value)
{
    return PCG3Base(uint3(value.x, value.y, ~value.x));
}
inline uint3 PCG3(uint3 value)
{
    return PCG3Base(value);
}
inline uint3 PCG3(uint4 value)
{
    return PCG4Base(value).yzw;
}

inline uint4 PCG4(uint value)
{
    return PCG4Base(uint4(value.x, ~value.x, value.x << 1, ~value.x << 1));
}
inline uint4 PCG4(uint2 value)
{
    return PCG4Base(uint4(value.x, value.y, ~value.x, ~value.y));
}
inline uint4 PCG4(uint3 value)
{
    return PCG4Base(uint4(value.x, value.y, value.z, ~value.x));
}
inline uint4 PCG4(uint4 value)
{
    return PCG4Base(value);
}

// float => uint
inline uint PCG1(float value)
{
    return PCG1(asuint(value));
}
inline uint PCG1(float2 value)
{
    return PCG1(asuint(value));
}
inline uint PCG1(float3 value)
{
    return PCG1(asuint(value));
}
inline uint PCG1(float4 value)
{
    return PCG1(asuint(value));
}

inline uint2 PCG2(float value)
{
    return PCG2(asuint(value));
}
inline uint2 PCG2(float2 value)
{
    return PCG2(asuint(value));
}
inline uint2 PCG2(float3 value)
{
    return PCG2(asuint(value));
}
inline uint2 PCG2(float4 value)
{
    return PCG2(asuint(value));
}

inline uint3 PCG3(float value)
{
    return PCG3(asuint(value));
}
inline uint3 PCG3(float2 value)
{
    return PCG3(asuint(value));
}
inline uint3 PCG3(float3 value)
{
    return PCG3(asuint(value));
}
inline uint3 PCG3(float4 value)
{
    return PCG3(asuint(value));
}

inline uint4 PCG4(float value)
{
    return PCG4(asuint(value));
}
inline uint4 PCG4(float2 value)
{
    return PCG4(asuint(value));
}
inline uint4 PCG4(float3 value)
{
    return PCG4(asuint(value));
}
inline uint4 PCG4(float4 value)
{
    return PCG4(asuint(value));
}

// uint => float[0, 1)
inline float PCG1_01(uint value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(uint2 value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(uint3 value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(uint4 value)
{
    return UInt2Float01(PCG1(value));
}

inline float2 PCG2_01(uint value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(uint2 value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(uint3 value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(uint4 value)
{
    return UInt2Float01(PCG2(value));
}

inline float3 PCG3_01(uint value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(uint2 value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(uint3 value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(uint4 value)
{
    return UInt2Float01(PCG3(value));
}

inline float4 PCG4_01(uint value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(uint2 value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(uint3 value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(uint4 value)
{
    return UInt2Float01(PCG4(value));
}

// float => float[0, 1)
inline float PCG1_01(float value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(float2 value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(float3 value)
{
    return UInt2Float01(PCG1(value));
}
inline float PCG1_01(float4 value)
{
    return UInt2Float01(PCG1(value));
}

inline float2 PCG2_01(float value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(float2 value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(float3 value)
{
    return UInt2Float01(PCG2(value));
}
inline float2 PCG2_01(float4 value)
{
    return UInt2Float01(PCG2(value));
}

inline float3 PCG3_01(float value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(float2 value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(float3 value)
{
    return UInt2Float01(PCG3(value));
}
inline float3 PCG3_01(float4 value)
{
    return UInt2Float01(PCG3(value));
}

inline float4 PCG4_01(float value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(float2 value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(float3 value)
{
    return UInt2Float01(PCG4(value));
}
inline float4 PCG4_01(float4 value)
{
    return UInt2Float01(PCG4(value));
}

// uint => range[min, max)
inline float PCG1_Range(uint value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(uint2 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(uint3 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(uint4 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}

inline float2 PCG2_Range(uint value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(uint2 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(uint3 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(uint4 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}

inline float3 PCG3_Range(uint value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(uint2 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(uint3 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(uint4 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}

inline float4 PCG4_Range(uint value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(uint2 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(uint3 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(uint4 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}

// float => range[min, max)
inline float PCG1_Range(float value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(float2 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(float3 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}
inline float PCG1_Range(float4 value, float min, float max)
{
    return PCG1_01(value) * (max - min) + min;
}

inline float2 PCG2_Range(float value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(float2 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(float3 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}
inline float2 PCG2_Range(float4 value, float2 min, float2 max)
{
    return PCG2_01(value) * (max - min) + min;
}

inline float3 PCG3_Range(float value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(float2 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(float3 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}
inline float3 PCG3_Range(float4 value, float3 min, float3 max)
{
    return PCG3_01(value) * (max - min) + min;
}

inline float4 PCG4_Range(float value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(float2 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(float3 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}
inline float4 PCG4_Range(float4 value, float4 min, float4 max)
{
    return PCG4_01(value) * (max - min) + min;
}


#endif /* CS_PCG_HLSL */