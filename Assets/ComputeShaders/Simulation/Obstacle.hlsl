#ifndef CS_SIMULATION_OBSTACLE_HLSL
#define CS_SIMULATION_OBSTACLE_HLSL

#if !defined(IS_CUBE_AREA_SIMULATION) && !defined(IS_SPHERE_AREA_SIMULATION)
#define IS_CUBE_AREA_SIMULATION
#endif

inline bool IsSolidCell(float3 c_pos)
{
#if defined(IS_CUBE_AREA_SIMULATION)
    return false;
#elif defined(IS_SPHERE_AREA_SIMULATION)
    return length(c_pos) > 12.0;
#endif
}

inline void ClampPositionByObstacles(inout float3 position)
{
#if defined(IS_CUBE_AREA_SIMULATION)
    // do nothing
#elif defined(IS_SPHERE_AREA_SIMULATION)
    const float3 dir_from_center = position;
    const float dist_from_center = length(dir_from_center);
    position = normalize(dir_from_center) * min(dist_from_center, 11.9);
#endif
}


#endif /* CS_SIMULATION_OBSTACLE_HLSL */