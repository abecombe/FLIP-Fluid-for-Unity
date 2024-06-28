﻿#ifndef CS_SIMULATION_KERNEL_FUNC_HLSL
#define CS_SIMULATION_KERNEL_FUNC_HLSL

#if !defined(USE_LINEAR_KERNEL) && !defined(USE_QUADRATIC_KERNEL)
#define USE_LINEAR_KERNEL
#endif

inline float3 GetLinearWeight(float3 abs_x)
{
    return saturate(1.0f - abs_x);
}

inline float3 GetQuadraticWeight(float3 abs_x)
{
    return abs_x < 0.5f ? 0.75f - abs_x * abs_x : 0.5f * saturate(1.5f - abs_x) * saturate(1.5f - abs_x);
}

inline float GetWeight(float3 p_pos, float3 c_pos, float grid_inv_spacing)
{
    const float3 dist = abs((p_pos - c_pos) * grid_inv_spacing);

#if defined(USE_LINEAR_KERNEL)
    const float3 weight = GetLinearWeight(dist);
#elif defined(USE_QUADRATIC_KERNEL)
    const float3 weight = GetQuadraticWeight(dist);
#endif

    return weight.x * weight.y * weight.z;
}


#endif /* CS_SIMULATION_KERNEL_FUNC_HLSL */