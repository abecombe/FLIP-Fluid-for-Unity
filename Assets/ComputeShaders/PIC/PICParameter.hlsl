#ifndef _CS_PIC_PICPARAMETER_HLSL_
#define _CS_PIC_PICPARAMETER_HLSL_

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

static const float AverageWeight = 8.0f;
static const float InvAverageWeight = 1.0f / AverageWeight;
float4 _DensityProjectionParameter1;
float3 _DensityProjectionParameter2;


#endif /* _CS_PIC_PICPARAMETER_HLSL_ */
