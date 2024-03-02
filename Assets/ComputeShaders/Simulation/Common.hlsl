#ifndef CS_SIMULATION_COMMON_HLSL
#define CS_SIMULATION_COMMON_HLSL

float _DeltaTime;

#include "Assets/Packages/GPUUtil/DispatchHelper.hlsl"

#include "../Constant.hlsl"
#include "../PCG.hlsl"

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

#include "FLIPParticle.hlsl"
#include "FreeSurface.hlsl"
#include "BoundaryCondition.hlsl"
#include "KernelFunc.hlsl"


#endif /* CS_SIMULATION_COMMON_HLSL */