#ifndef CS_PIC_PIC_PARAMETER_HLSL
#define CS_PIC_PIC_PARAMETER_HLSL

float  _DeltaTime;

float3 _ParticleInitRangeMin;
float3 _ParticleInitRangeMax;

float3 _Gravity;
float3 _RayOrigin;
float3 _RayDirection;
float4 _MouseForceParameter; // x,y,z: Force w: Range

float4 _DiffusionParameter;

float3 _DivergenceParameter;
float4 _PressureProjectionParameter1;
float3 _PressureProjectionParameter2;

float  _Flipness;

float3 _GhostWeight;
float  _InvAverageWeight;
float4 _DensityProjectionParameter1;
float3 _DensityProjectionParameter2;


#endif /* CS_PIC_PIC_PARAMETER_HLSL */
