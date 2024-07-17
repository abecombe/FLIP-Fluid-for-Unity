#ifndef CS_SIMULATION_OBSTACLE_HLSL
#define CS_SIMULATION_OBSTACLE_HLSL

inline bool IsSolidCell(float3 c_pos)
{
    //return false;
    return length(c_pos) > 10.0;
}


#endif /* CS_SIMULATION_OBSTACLE_HLSL */