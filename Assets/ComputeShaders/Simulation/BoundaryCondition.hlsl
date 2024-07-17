#ifndef CS_SIMULATION_BOUNDARY_HLSL
#define CS_SIMULATION_BOUNDARY_HLSL

static const float POSITION_EPSILON = 1e-4;

#include "GridType.hlsl"

inline void ClampPosition(inout float3 position, float3 grid_min, float3 grid_max)
{
    position = clamp(position, grid_min + POSITION_EPSILON, grid_max - POSITION_EPSILON);
    const float3 dir_from_center = position;
    const float dist_from_center = length(dir_from_center);
    position = normalize(dir_from_center) * min(dist_from_center, 9.95);
}

inline void EnforceBoundaryCondition(inout float3 velocity, uint grid_types)
{
    const bool3 is_solid_cell =
        (bool3)IsSolidCell(GetMyType(grid_types)) ||
        bool3(IsSolidCell(GetXPrevType(grid_types)), IsSolidCell(GetYPrevType(grid_types)), IsSolidCell(GetZPrevType(grid_types)));
    velocity = is_solid_cell ? 0.0f : velocity;
}


#endif /* CS_SIMULATION_BOUNDARY_HLSL */