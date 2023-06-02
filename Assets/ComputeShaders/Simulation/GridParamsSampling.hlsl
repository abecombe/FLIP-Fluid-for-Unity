#ifndef CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL
#define CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

#if defined(USE_LINEAR_KERNEL)
// sample grid param using linear interpolation
#define S0(S) (1.0f - S)
#define S1(S) (S)

#define INTERPOLATE_X(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, STRUCT) {\
const uint c_id_0 = CellIndexToCellID(C_INDEX + int3(0, 0, 0));\
const uint c_id_1 = CellIndexToCellID(C_INDEX + int3(1, 0, 0));\
STRUCT sum_x = (STRUCT)0;\
sum_x += GRID_BUFFER[c_id_0] * S0(S.x);\
sum_x += GRID_BUFFER[c_id_1] * S1(S.x);\
sum_x *= WEIGHT;\
SUM += sum_x;\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, STRUCT) {\
STRUCT sum_y = (STRUCT)0;\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 0, 0), S, S0(S.y), GRID_BUFFER, STRUCT)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 1, 0), S, S1(S.y), GRID_BUFFER, STRUCT)\
sum_y *= WEIGHT;\
SUM += sum_y;\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, STRUCT) {\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 0), S, S0(S.z), GRID_BUFFER, STRUCT)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 1), S, S1(S.z), GRID_BUFFER, STRUCT)\
}\

#define INTERPOLATE(VALUE, C_INDEX, S, GRID_BUFFER, STRUCT) {\
INTERPOLATE_Z(VALUE, C_INDEX, S, GRID_BUFFER, STRUCT)\
}\

#define SAMPLE_GRID_PARAM(POS, GRID_BUFFER, STRUCT) \
STRUCT ret = 0;\
const float3 g_pos = WorldPosToGridPos(POS);\
const int3 c_index = floor(g_pos - 0.5f);\
const float3 s = frac(g_pos - 0.5f);\
INTERPOLATE(ret, c_index, s, GRID_BUFFER, STRUCT)\
return ret;\

#elif defined(USE_QUADRATIC_KERNEL)
// sample grid param using quadratic interpolation
#define S0(S) (0.5f * (0.5f - S) * (0.5f - S))
#define S1(S) (0.75f - S * S)
#define S2(S) (0.5f * (0.5f + S) * (0.5f + S))

#define INTERPOLATE_X(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, STRUCT) {\
const uint c_id_0 = CellIndexToCellID(C_INDEX + int3(-1, 0, 0));\
const uint c_id_1 = CellIndexToCellID(C_INDEX + int3(0, 0, 0));\
const uint c_id_2 = CellIndexToCellID(C_INDEX + int3(1, 0, 0));\
STRUCT sum_x = (STRUCT)0;\
sum_x += GRID_BUFFER[c_id_0] * S0(S.x);\
sum_x += GRID_BUFFER[c_id_1] * S1(S.x);\
sum_x += GRID_BUFFER[c_id_2] * S2(S.x);\
sum_x *= WEIGHT;\
SUM += sum_x;\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, WEIGHT, GRID_BUFFER, STRUCT) {\
STRUCT sum_y = (STRUCT)0;\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, -1, 0), S, S0(S.y), GRID_BUFFER, STRUCT)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 0, 0), S, S1(S.y), GRID_BUFFER, STRUCT)\
INTERPOLATE_X(sum_y, C_INDEX + int3(0, 1, 0), S, S2(S.y), GRID_BUFFER, STRUCT)\
sum_y *= WEIGHT;\
SUM += sum_y;\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, STRUCT) {\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, -1), S, S0(S.z), GRID_BUFFER, STRUCT)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 0), S, S1(S.z), GRID_BUFFER, STRUCT)\
INTERPOLATE_Y(SUM, C_INDEX + int3(0, 0, 1), S, S2(S.z), GRID_BUFFER, STRUCT)\
}\

#define INTERPOLATE(VALUE, C_INDEX, S, GRID_BUFFER, STRUCT) {\
INTERPOLATE_Z(VALUE, C_INDEX, S, GRID_BUFFER, STRUCT)\
}\

#define SAMPLE_GRID_PARAM(POS, GRID_BUFFER, STRUCT) \
STRUCT ret = 0;\
const float3 g_pos = WorldPosToGridPos(POS);\
const int3 c_index = round(g_pos - 0.5f);\
const float3 s = (g_pos - 0.5f) - round(g_pos - 0.5f);\
INTERPOLATE(ret, c_index, s, GRID_BUFFER, STRUCT)\
return ret;\

#else
// default
#define SAMPLE_GRID_PARAM(POS, GRID_BUFFER, STRUCT) \
return (STRUCT)0;\

#endif

inline float SampleGridParam(float3 position, StructuredBuffer<float> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float)
}
inline float SampleGridParam(float3 position, RWStructuredBuffer<float> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float)
}
inline float2 SampleGridParam(float3 position, StructuredBuffer<float2> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float2)
}
inline float2 SampleGridParam(float3 position, RWStructuredBuffer<float2> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float2)
}
inline float3 SampleGridParam(float3 position, StructuredBuffer<float3> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float3)
}
inline float3 SampleGridParam(float3 position, RWStructuredBuffer<float3> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float3)
}
inline float4 SampleGridParam(float3 position, StructuredBuffer<float4> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float4)
}
inline float4 SampleGridParam(float3 position, RWStructuredBuffer<float4> grid_buffer)
{
    SAMPLE_GRID_PARAM(position, grid_buffer, float4)
}


#endif /* CS_SIMULATION_GRID_PARAMS_SAMPLING_HLSL */