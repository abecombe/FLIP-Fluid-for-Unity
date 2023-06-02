#ifndef CS_SIMULATION_BOUNDARY_HLSL
#define CS_SIMULATION_BOUNDARY_HLSL

bool3 _BoundaryPositive;
bool3 _BoundaryNegative;

static const float POSITION_EPSILON = 1e-4;

inline void ClampPosition(inout float3 position, float3 grid_min, float3 grid_max)
{
    position = clamp(position, grid_min + POSITION_EPSILON, grid_max - POSITION_EPSILON);
}

inline void EnforceBoundaryCondition(inout float3 velocity, int3 c_index, int3 grid_size)
{
    velocity = (c_index == 0 && _BoundaryNegative) || (c_index == grid_size - 1 && _BoundaryPositive) ? 0.0f : velocity;
}


#endif /* CS_SIMULATION_BOUNDARY_HLSL */
