#ifndef CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL
#define CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

#if defined(USE_LINEAR_KERNEL)
// sample grid param using linear interpolation
#define S0(S) (1.0f - S)
#define S1(S) (S)

#define INTERPOLATE_X(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, AXIS) {\
const int3 c_index_0 = C_INDEX + int3(0, 0, 0);\
const int3 c_index_1 = C_INDEX + int3(1, 0, 0);\
const uint c_id_0 = CellIndexToCellID(c_index_0);\
const uint c_id_1 = CellIndexToCellID(c_index_1);\
float sum_x = 0;\
sum_x += c_index_0.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_0].AXIS * S0(S.x) : 0.0f;\
sum_x += c_index_1.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_1].AXIS * S1(S.x) : 0.0f;\
sum_x *= WEIGHT;\
SUM += sum_x;\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, AXIS) {\
float sum_y = 0;\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 0, 0), S, S0(S.y), GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 1, 0), S, S1(S.y), GRID_BUFFER, AXIS)\
sum_y *= WEIGHT;\
SUM += sum_y;\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 0), S, S0(S.z), GRID_BUFFER, AXIS)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 1), S, S1(S.z), GRID_BUFFER, AXIS)\
}\

#define INTERPOLATE(VALUE, C_INDEX, S, GRID_BUFFER, AXIS) \
INTERPOLATE_Z(VALUE, C_INDEX, S, GRID_BUFFER, AXIS)\

#define SAMPLE_GRID_PARAM(VALUE, G_POS, GRID_BUFFER, AXIS) \
const int3 c_index = floor(G_POS - 0.5f);\
const float3 s = frac(G_POS - 0.5f);\
INTERPOLATE(VALUE, c_index, s, GRID_BUFFER, AXIS)\

#elif defined(USE_QUADRATIC_KERNEL)
// sample grid param using quadratic interpolation
#define S0(S) (0.5f * (0.5f - S) * (0.5f - S))
#define S1(S) (0.75f - S * S)
#define S2(S) (0.5f * (0.5f + S) * (0.5f + S))

#define INTERPOLATE_X(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, AXIS) {\
const int3 c_index_0 = C_INDEX + int3(-1, 0, 0);\
const int3 c_index_1 = C_INDEX + int3(0, 0, 0);\
const int3 c_index_2 = C_INDEX + int3(1, 0, 0);\
const uint c_id_0 = CellIndexToCellID(c_index_0);\
const uint c_id_1 = CellIndexToCellID(c_index_1);\
const uint c_id_2 = CellIndexToCellID(c_index_2);\
float sum_x = 0;\
sum_x += c_index_0.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_0].AXIS * S0(S.x) : 0.0f;\
sum_x += c_index_1.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_1].AXIS * S1(S.x) : 0.0f;\
sum_x += c_index_2.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_2].AXIS * S2(S.x) : 0.0f;\
sum_x *= WEIGHT;\
SUM += sum_x;\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, AXIS) {\
float sum_y = 0;\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, -1, 0), S, S0(S.y), GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 0, 0), S, S1(S.y), GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 1, 0), S, S2(S.y), GRID_BUFFER, AXIS)\
sum_y *= WEIGHT;\
SUM += sum_y;\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, -1), S, S0(S.z), GRID_BUFFER, AXIS)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 0), S, S1(S.z), GRID_BUFFER, AXIS)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 1), S, S2(S.z), GRID_BUFFER, AXIS)\
}\

#define INTERPOLATE(VALUE, C_INDEX, S, GRID_BUFFER, AXIS) \
INTERPOLATE_Z(VALUE, C_INDEX, S, GRID_BUFFER, AXIS)\

#define SAMPLE_GRID_PARAM(VALUE, G_POS, GRID_BUFFER, AXIS) \
const int3 c_index = round(G_POS - 0.5f);\
const float3 s = (G_POS - 0.5f) - round(G_POS - 0.5f);\
INTERPOLATE(VALUE, c_index, s, GRID_BUFFER, AXIS)\

#else
// default
#define SAMPLE_GRID_PARAM(VALUE, G_POS, GRID_BUFFER, AXIS) \
VALUE = 0;\

#endif

#define SAMPLE_GRID_PARAM_X(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = WorldPosToGridPos(POS) + float3(0.5f, 0.0f, 0.0f);\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, x)\
}\

#define SAMPLE_GRID_PARAM_Y(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = WorldPosToGridPos(POS) + float3(0.0, 0.5f, 0.0f);\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, y)\
}\

#define SAMPLE_GRID_PARAM_Z(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = WorldPosToGridPos(POS) + float3(0.0f, 0.0f, 0.5f);\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, z)\
}\

#define SAMPLE_GRID_PARAM_MASTER(POS, GRID_BUFFER) \
float3 ret = (float3)0;\
SAMPLE_GRID_PARAM_X(ret.x, POS, GRID_BUFFER)\
SAMPLE_GRID_PARAM_Y(ret.y, POS, GRID_BUFFER)\
SAMPLE_GRID_PARAM_Z(ret.z, POS, GRID_BUFFER)\
return ret;\

inline float3 SampleGridParam(float3 pos, StructuredBuffer<float3> grid_buffer)
{
    SAMPLE_GRID_PARAM_MASTER(pos, grid_buffer)
}

inline float3 SampleGridParam(float3 pos, RWStructuredBuffer<float3> grid_buffer)
{
    SAMPLE_GRID_PARAM_MASTER(pos, grid_buffer)
}

#endif /* CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL */