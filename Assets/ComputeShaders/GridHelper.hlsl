#ifndef CS_GRID_HELPER_HLSL
#define CS_GRID_HELPER_HLSL

#include "GridData.hlsl"

inline uint CellIndexToCellID(int3 index)
{
    uint3 clamped_index = clamp(index, (int3)0, _GridSize - 1);
    return clamped_index.x + clamped_index.y * _GridSize.x + clamped_index.z * _GridSize.x * _GridSize.y;
}

inline int3 CellIDToCellIndex(uint id)
{
    int x = id % _GridSize.x;
    int y = id / _GridSize.x % _GridSize.y;
    int z = id / (_GridSize.x * _GridSize.y);
    return int3(x, y, z);
}

inline float3 WorldPosToGridPos(float3 position)
{
    return (position - _GridMin) * _GridInvSpacing;
}

inline int3 WorldPosToCellIndex(float3 position)
{
    return floor(WorldPosToGridPos(position));
}

inline uint WorldPosToCellID(float3 position)
{
    return CellIndexToCellID(WorldPosToCellIndex(position));
}

inline float3 CellIndexToWorldPos(int3 index)
{
    return _GridMin + (index + 0.5f) * _GridSpacing;
}

#define FOR_EACH_NEIGHBOR_CELL_START(C_INDEX, NC_INDEX, NC_ID, RANGE) {\
for (int i = max((int)C_INDEX.x + RANGE[0], 0); i <= min((int)C_INDEX.x + RANGE[1], _GridSize.x - 1); ++i)\
for (int j = max((int)C_INDEX.y + RANGE[2], 0); j <= min((int)C_INDEX.y + RANGE[3], _GridSize.y - 1); ++j)\
for (int k = max((int)C_INDEX.z + RANGE[4], 0); k <= min((int)C_INDEX.z + RANGE[5], _GridSize.z - 1); ++k) {\
    const int3 NC_INDEX = int3(i, j, k);\
    const uint NC_ID = CellIndexToCellID(NC_INDEX);\

#define FOR_EACH_NEIGHBOR_CELL_END }}

#define FOR_EACH_NEIGHBOR_CELL_PARTICLE_START(C_INDEX, P_ID, P_ID_BUFFER, RANGE) {\
for (int i = max((int)C_INDEX.x + RANGE[0], 0); i <= min((int)C_INDEX.x + RANGE[1], _GridSize.x - 1); ++i)\
for (int j = max((int)C_INDEX.y + RANGE[2], 0); j <= min((int)C_INDEX.y + RANGE[3], _GridSize.y - 1); ++j)\
for (int k = max((int)C_INDEX.z + RANGE[4], 0); k <= min((int)C_INDEX.z + RANGE[5], _GridSize.z - 1); ++k) {\
    const uint2 index = P_ID_BUFFER[CellIndexToCellID(int3(i, j, k))];\
    for (uint P_ID = index.x; P_ID < index.y; ++P_ID) {\

#define FOR_EACH_NEIGHBOR_CELL_PARTICLE_END }}}


#endif /* CS_GRID_HELPER_HLSL */