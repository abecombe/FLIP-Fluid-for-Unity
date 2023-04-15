#ifndef CS_PIC_PIC_KERNEL_HLSL
#define CS_PIC_PIC_KERNEL_HLSL

inline float3 LinearKernel(float3 absx)
{
    return saturate(1.0f - absx);
}

inline float3 QuadraticKernel(float3 absx)
{
    return absx < 0.5f ? 0.75f - absx * absx : 0.5f * saturate(1.5f - absx) * saturate(1.5f - absx);
}

inline float GetWeight(float3 p_pos, float3 c_pos, float3 invH)
{
    const float3 dis = abs((p_pos - c_pos) * invH);

#if defined(USE_LINEAR_KERNEL)
    const float3 weight = LinearKernel(dis);

#elif defined(USE_QUADRATIC_KERNEL)
    const float3 weight = QuadraticKernel(dis);

#else
    const float3 weight = (float3)0;

#endif

    return weight.x * weight.y * weight.z;
}


#endif /* CS_PIC_PIC_KERNEL_HLSL */
