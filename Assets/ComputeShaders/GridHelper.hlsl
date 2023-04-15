#ifndef CS_GRID_HELPER_HLSL
#define CS_GRID_HELPER_HLSL

#include "GridData.hlsl"

inline float3 PosToCellPos(float3 pos)
{
    return (pos - _GridMin) * _GridInvSpacing;
}

inline int3 PosToCellIndex(float3 pos)
{
    return floor(PosToCellPos(pos));
}

inline float3 CellIndexToCellPos(uint3 index)
{
    return _GridMin + (index + 0.5f) * _GridSpacing;
}

inline uint CellIndexToCellID(int3 index)
{
    uint3 clamped_index = clamp(index, (int3)0, _GridSize - 1);
    return clamped_index.x + clamped_index.y * _GridSize.x + clamped_index.z * _GridSize.x * _GridSize.y;
}

inline uint PosToCellID(float3 pos)
{
    return CellIndexToCellID(PosToCellIndex(pos));
}

inline uint3 CellIDToCellIndex(uint id)
{
    uint x = id % _GridSize.x;
    uint y = (id / _GridSize.x) % _GridSize.y;
    uint z = id / (_GridSize.x * _GridSize.y);
    return uint3(x, y, z);
}

#define FOR_EACH_NEIGHBOR_CELL_START(C_INDEX, NC_INDEX, NC_ID, RANGE) {\
for (int i = max((int)C_INDEX.x + RANGE[0], 0); i <= min((int)C_INDEX.x + RANGE[1], _GridSize.x - 1); ++i)\
for (int j = max((int)C_INDEX.y + RANGE[2], 0); j <= min((int)C_INDEX.y + RANGE[3], _GridSize.y - 1); ++j)\
for (int k = max((int)C_INDEX.z + RANGE[4], 0); k <= min((int)C_INDEX.z + RANGE[5], _GridSize.z - 1); ++k) {\
    const uint3 NC_INDEX = uint3(i, j, k);\
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
