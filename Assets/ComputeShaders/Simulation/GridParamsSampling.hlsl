#ifndef CS_PIC_GRID_PARAMS_SAMPLING_HLSL
#define CS_PIC_GRID_PARAMS_SAMPLING_HLSL

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

#if defined(USE_LINEAR_KERNEL)
// sample grid param using linear interpolation
#define S0(S) (1.0f - S)
#define S1(S) (S)

#define INTERPOLATE_X(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
const uint c_id_0 = CellIndexToCellID(C_INDEX + int3(0, 0, 0));\
const uint c_id_1 = CellIndexToCellID(C_INDEX + int3(1, 0, 0));\
SUM += GRID_BUFFER[c_id_0].AXIS * S0(S.x);\
SUM += GRID_BUFFER[c_id_1].AXIS * S1(S.x);\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
float2 sum_x = (float2)0;\
INTERPOLATE_X(sum_x[0], C_INDEX + int3(0, 0, 0), S, GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_x[1], C_INDEX + int3(0, 1, 0), S, GRID_BUFFER, AXIS)\
SUM += sum_x[0] * S0(S.y);\
SUM += sum_x[1] * S1(S.y);\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
float2 sum_y = (float2)0;\
INTERPOLATE_Y(sum_y[0], C_INDEX + int3(0, 0, 0), S, GRID_BUFFER, AXIS)\
INTERPOLATE_Y(sum_y[1], C_INDEX + int3(0, 0, 1), S, GRID_BUFFER, AXIS)\
SUM += sum_y[0] * S0(S.z);\
SUM += sum_y[1] * S1(S.z);\
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

#define INTERPOLATE_X(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
const uint c_id_0 = CellIndexToCellID(C_INDEX + int3(-1, 0, 0));\
const uint c_id_1 = CellIndexToCellID(C_INDEX + int3(0, 0, 0));\
const uint c_id_2 = CellIndexToCellID(C_INDEX + int3(1, 0, 0));\
SUM += GRID_BUFFER[c_id_0].AXIS * S0(S.x);\
SUM += GRID_BUFFER[c_id_1].AXIS * S1(S.x);\
SUM += GRID_BUFFER[c_id_2].AXIS * S2(S.x);\
}\

#define INTERPOLATE_Y(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
float3 sum_x = (float3)0;\
INTERPOLATE_X(sum_x[0], C_INDEX + int3(0, -1, 0), S, GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_x[1], C_INDEX + int3(0, 0, 0), S, GRID_BUFFER, AXIS)\
INTERPOLATE_X(sum_x[2], C_INDEX + int3(0, 1, 0), S, GRID_BUFFER, AXIS)\
SUM += sum_x[0] * S0(S.y);\
SUM += sum_x[1] * S1(S.y);\
SUM += sum_x[2] * S2(S.y);\
}\

#define INTERPOLATE_Z(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
float3 sum_y = (float3)0;\
INTERPOLATE_Y(sum_y[0], C_INDEX + int3(0, 0, -1), S, GRID_BUFFER, AXIS)\
INTERPOLATE_Y(sum_y[1], C_INDEX + int3(0, 0, 0), S, GRID_BUFFER, AXIS)\
INTERPOLATE_Y(sum_y[2], C_INDEX + int3(0, 0, 1), S, GRID_BUFFER, AXIS)\
SUM += sum_y[0] * S0(S.z);\
SUM += sum_y[1] * S1(S.z);\
SUM += sum_y[2] * S2(S.z);\
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

#define SAMPLE_GRID_SCALAR(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing;\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, x)\
}\

#define SAMPLE_GRID_VECTOR_X(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing;\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, x)\
}\

#define SAMPLE_GRID_VECTOR_Y(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing;\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, y)\
}\

#define SAMPLE_GRID_VECTOR_Z(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing;\
SAMPLE_GRID_PARAM(VALUE, g_pos, GRID_BUFFER, z)\
}\

inline float SampleGridParam(float3 pos, StructuredBuffer<float> grid_buffer)
{
    float value = 0;
    SAMPLE_GRID_SCALAR(value, pos, grid_buffer)
    return value;
}

inline float3 SampleGridParam(float3 pos, StructuredBuffer<float3> grid_buffer)
{
    float3 value = 0;
    SAMPLE_GRID_VECTOR_X(value.x, pos, grid_buffer)
    SAMPLE_GRID_VECTOR_Y(value.y, pos, grid_buffer)
    SAMPLE_GRID_VECTOR_Z(value.z, pos, grid_buffer)
    return value;
}


#endif /* CS_PIC_GRID_PARAMS_SAMPLING_HLSL */