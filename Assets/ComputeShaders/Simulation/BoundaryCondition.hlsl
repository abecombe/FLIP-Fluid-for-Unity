#ifndef CS_SIMULATION_BOUNDARY_HLSL
#define CS_SIMULATION_BOUNDARY_HLSL

static const float POSITION_EPSILON = 1e-4;

inline void ClampPosition(inout float3 position, float3 grid_min, float3 grid_max)
{
    position = clamp(position, grid_min + POSITION_EPSILON, grid_max - POSITION_EPSILON);
}

inline void EnforceBoundaryCondition(inout float3 velocity, int3 c_index)
{
    velocity = c_index > 0 ? velocity : 0.0f;
}


#endif /* CS_SIMULATION_BOUNDARY_HLSL */