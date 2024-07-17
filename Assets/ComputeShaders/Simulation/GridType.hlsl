#ifndef CS_SIMULATION_GRID_TYPE_HLSL
#define CS_SIMULATION_GRID_TYPE_HLSL

#include "../Bit.hlsl"

// GridType
static const uint GT_AIR = 0;
static const uint GT_FLUID = 1;
static const uint GT_SOLID = 2;

// Mask
static const uint MyTypeMask         = 0x00000003u;
static const uint MyTypeMaskShift    = 0;
static const uint XPrevTypeMask      = 0x0000000cu;
static const uint XPrevTypeMaskShift = 2;
static const uint XNextTypeMask      = 0x00000030u;
static const uint XNextTypeMaskShift = 4;
static const uint YPrevTypeMask      = 0x000000c0u;
static const uint YPrevTypeMaskShift = 6;
static const uint YNextTypeMask      = 0x00000300u;
static const uint YNextTypeMaskShift = 8;
static const uint ZPrevTypeMask      = 0x00000c00u;
static const uint ZPrevTypeMaskShift = 10;
static const uint ZNextTypeMask      = 0x00003000u;
static const uint ZNextTypeMaskShift = 12;

inline void SetMyType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, MyTypeMask, MyTypeMaskShift);
}
inline uint GetMyType(uint grid_types)
{
    return GET_VALUE(grid_types, MyTypeMask, MyTypeMaskShift);
}
inline void SetXPrevType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, XPrevTypeMask, XPrevTypeMaskShift);
}
inline uint GetXPrevType(uint grid_types)
{
    return GET_VALUE(grid_types, XPrevTypeMask, XPrevTypeMaskShift);
}
inline void SetXNextType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, XNextTypeMask, XNextTypeMaskShift);
}
inline uint GetXNextType(uint grid_types)
{
    return GET_VALUE(grid_types, XNextTypeMask, XNextTypeMaskShift);
}
inline void SetYPrevType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, YPrevTypeMask, YPrevTypeMaskShift);
}
inline uint GetYPrevType(uint grid_types)
{
    return GET_VALUE(grid_types, YPrevTypeMask, YPrevTypeMaskShift);
}
inline void SetYNextType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, YNextTypeMask, YNextTypeMaskShift);
}
inline uint GetYNextType(uint grid_types)
{
    return GET_VALUE(grid_types, YNextTypeMask, YNextTypeMaskShift);
}
inline void SetZPrevType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, ZPrevTypeMask, ZPrevTypeMaskShift);
}
inline uint GetZPrevType(uint grid_types)
{
    return GET_VALUE(grid_types, ZPrevTypeMask, ZPrevTypeMaskShift);
}
inline void SetZNextType(inout uint grid_types, uint grid_type)
{
    SET_VALUE(grid_types, grid_type, ZNextTypeMask, ZNextTypeMaskShift);
}
inline uint GetZNextType(uint grid_types)
{
    return GET_VALUE(grid_types, ZNextTypeMask, ZNextTypeMaskShift);
}

inline void SetMyType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, MyTypeMask, MyTypeMaskShift);
}
inline uint3 GetMyType(uint3 grid_types)
{
    return GET_VALUE(grid_types, MyTypeMask, MyTypeMaskShift);
}
inline void SetXPrevType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, XPrevTypeMask, XPrevTypeMaskShift);
}
inline uint3 GetXPrevType(uint3 grid_types)
{
    return GET_VALUE(grid_types, XPrevTypeMask, XPrevTypeMaskShift);
}
inline void SetXNextType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, XNextTypeMask, XNextTypeMaskShift);
}
inline uint3 GetXNextType(uint3 grid_types)
{
    return GET_VALUE(grid_types, XNextTypeMask, XNextTypeMaskShift);
}
inline void SetYPrevType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, YPrevTypeMask, YPrevTypeMaskShift);
}
inline uint3 GetYPrevType(uint3 grid_types)
{
    return GET_VALUE(grid_types, YPrevTypeMask, YPrevTypeMaskShift);
}
inline void SetYNextType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, YNextTypeMask, YNextTypeMaskShift);
}
inline uint3 GetYNextType(uint3 grid_types)
{
    return GET_VALUE(grid_types, YNextTypeMask, YNextTypeMaskShift);
}
inline void SetZPrevType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, ZPrevTypeMask, ZPrevTypeMaskShift);
}
inline uint3 GetZPrevType(uint3 grid_types)
{
    return GET_VALUE(grid_types, ZPrevTypeMask, ZPrevTypeMaskShift);
}
inline void SetZNextType(inout uint3 grid_types, uint3 grid_type)
{
    SET_VALUE(grid_types, grid_type, ZNextTypeMask, ZNextTypeMaskShift);
}
inline uint3 GetZNextType(uint3 grid_types)
{
    return GET_VALUE(grid_types, ZNextTypeMask, ZNextTypeMaskShift);
}

inline bool IsAirCell(uint grid_type)
{
    return grid_type == GT_AIR;
}
inline bool IsFluidCell(uint grid_type)
{
    return grid_type == GT_FLUID;
}
inline bool IsSolidCell(uint grid_type)
{
    return grid_type == GT_SOLID;
}

inline bool3 IsAirCell(uint3 grid_type)
{
    return grid_type == (uint3)GT_AIR;
}
inline bool3 IsFluidCell(uint3 grid_type)
{
    return grid_type == (uint3)GT_FLUID;
}
inline bool3 IsSolidCell(uint3 grid_type)
{
    return grid_type == (uint3)GT_SOLID;
}


#endif /* CS_SIMULATION_GRID_TYPE_HLSL */