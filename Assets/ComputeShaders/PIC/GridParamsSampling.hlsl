#ifndef _CS_PIC_GRIDPARAMSSAMPLING_HLSL_
#define _CS_PIC_GRIDPARAMSSAMPLING_HLSL_

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

#if defined(USE_LINEAR_KERNEL)
// sample grid param using linear interpolation
#define S0(S) (1.0f - S)
#define S1(S) (S)

#define INTERPOLATE_X(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
const int3 c_index_0 = C_INDEX + int3(0, 0, 0);\
const int3 c_index_1 = C_INDEX + int3(1, 0, 0);\
const uint c_id_0 = CellIndexToCellID(c_index_0);\
const uint c_id_1 = CellIndexToCellID(c_index_1);\
SUM += c_index_0.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_0].AXIS * S0(S.x) : 0.0f;\
SUM += c_index_1.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_1].AXIS * S1(S.x) : 0.0f;\
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

#define SAMPLE_GRID_PARAMS(VALUE, G_POS, GRID_BUFFER, AXIS) \
const int3 c_index = floor(G_POS - 0.5f);\
const float3 s = frac(G_POS - 0.5f);\
INTERPOLATE(VALUE, c_index, s, GRID_BUFFER, AXIS)\

#elif defined(USE_QUADRATIC_KERNEL)
// sample grid param using quadratic interpolation
#define S0(S) (0.5f * (0.5f - S) * (0.5f - S))
#define S1(S) (0.75f - S * S)
#define S2(S) (0.5f * (0.5f + S) * (0.5f + S))

#define INTERPOLATE_X(SUM, C_INDEX, S, GRID_BUFFER, AXIS) {\
const int3 c_index_0 = C_INDEX + int3(-1, 0, 0);\
const int3 c_index_1 = C_INDEX + int3(0, 0, 0);\
const int3 c_index_2 = C_INDEX + int3(1, 0, 0);\
const uint c_id_0 = CellIndexToCellID(c_index_0);\
const uint c_id_1 = CellIndexToCellID(c_index_1);\
const uint c_id_2 = CellIndexToCellID(c_index_2);\
SUM += c_index_0.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_0].AXIS * S0(S.x) : 0.0f;\
SUM += c_index_1.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_1].AXIS * S1(S.x) : 0.0f;\
SUM += c_index_2.AXIS < _GridSize.AXIS ? GRID_BUFFER[c_id_2].AXIS * S2(S.x) : 0.0f;\
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

#define SAMPLE_GRID_PARAMS(VALUE, G_POS, GRID_BUFFER, AXIS) \
const int3 c_index = round(G_POS - 0.5f);\
const float3 s = (G_POS - 0.5f) - round(G_POS - 0.5f);\
INTERPOLATE(VALUE, c_index, s, GRID_BUFFER, AXIS)\

#else
// default
#define SAMPLE_GRID_PARAMS(VALUE, G_POS, GRID_BUFFER, AXIS) \
VALUE = 0;\

#endif

#define SAMPLE_GRID_PARAMS_X(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing + float3(0.5f, 0.0f, 0.0f);\
SAMPLE_GRID_PARAMS(VALUE, g_pos, GRID_BUFFER, x)\
}\

#define SAMPLE_GRID_PARAMS_Y(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing + float3(0.0, 0.5f, 0.0f);\
SAMPLE_GRID_PARAMS(VALUE, g_pos, GRID_BUFFER, y)\
}\

#define SAMPLE_GRID_PARAMS_Z(VALUE, POS, GRID_BUFFER) {\
const float3 g_pos = (POS - _GridMin) * _GridInvSpacing + float3(0.0f, 0.0f, 0.5f);\
SAMPLE_GRID_PARAMS(VALUE, g_pos, GRID_BUFFER, z)\
}\

#define SAMPLE_GRID_PARAMS_MASTER(POS, GRID_BUFFER) \
float3 value = (float3)0;\
SAMPLE_GRID_PARAMS_X(value.x, POS, GRID_BUFFER)\
SAMPLE_GRID_PARAMS_Y(value.y, POS, GRID_BUFFER)\
SAMPLE_GRID_PARAMS_Z(value.z, POS, GRID_BUFFER)\
return value;\

inline float3 SampleGridParams(float3 pos, StructuredBuffer<float3> gridBuffer)
{
    SAMPLE_GRID_PARAMS_MASTER(pos, gridBuffer)
}


#endif /* _CS_PIC_GRIDPARAMSSAMPLING_HLSL_ */