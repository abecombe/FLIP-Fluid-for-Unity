#ifndef CS_SORT_GRID_SORT_HELPER_HLSL
#define CS_SORT_GRID_SORT_HELPER_HLSL

#include "Assets/Packages/GPUUtil/DispatchHelper.hlsl"

#include "../GridData.hlsl"
#include "../GridHelper.hlsl"

uint _NumObjects;

StructuredBuffer<Object> _ObjectBufferRead;
RWStructuredBuffer<Object> _ObjectBufferWrite;

StructuredBuffer<uint2> _ObjectCellIDPairBufferRead;
RWStructuredBuffer<uint2> _ObjectCellIDPairBufferWrite;

RWStructuredBuffer<uint2> _GridObjectIDBufferWrite;

[numthreads(128, 1, 1)]
void MakeObjectCellIDPair(uint3 thread_id : SV_DispatchThreadID)
{
    RETURN_IF_INVALID(thread_id);

    const uint o_id = thread_id.x;
    const uint c_id = WorldPosToCellID(_ObjectBufferRead[o_id].Pos);

    _ObjectCellIDPairBufferWrite[o_id] = uint2(c_id, o_id);
}

[numthreads(128, 1, 1)]
void ClearGridObjectID(uint3 thread_id : SV_DispatchThreadID)
{
    RETURN_IF_INVALID(thread_id);

    const uint c_id = thread_id.x;

    _GridObjectIDBufferWrite[c_id] = (uint2)0;
}

[numthreads(128, 1, 1)]
void SetGridObjectID(uint3 thread_id : SV_DispatchThreadID)
{
    RETURN_IF_INVALID(thread_id);

    const uint curr_o_id = thread_id.x;

    const uint prev_o_id = curr_o_id == 0 ? _NumObjects - 1 : curr_o_id - 1;
    const uint next_o_id = curr_o_id == _NumObjects - 1 ? 0 : curr_o_id + 1;
    const uint curr_c_id = _ObjectCellIDPairBufferRead[curr_o_id].x;
    const uint prev_c_id = _ObjectCellIDPairBufferRead[prev_o_id].x;
    const uint next_c_id = _ObjectCellIDPairBufferRead[next_o_id].x;

    if (curr_c_id != prev_c_id) _GridObjectIDBufferWrite[curr_c_id].x = curr_o_id;
    if (curr_c_id != next_c_id) _GridObjectIDBufferWrite[curr_c_id].y = curr_o_id + 1;
}

[numthreads(128, 1, 1)]
void RearrangeObject(uint3 thread_id : SV_DispatchThreadID)
{
    RETURN_IF_INVALID(thread_id);

    const uint o_id = thread_id.x;

    const uint past_o_id = _ObjectCellIDPairBufferRead[o_id].y;

    _ObjectBufferWrite[o_id] = _ObjectBufferRead[past_o_id];
}


#endif /* CS_SORT_GRID_SORT_HELPER_HLSL */