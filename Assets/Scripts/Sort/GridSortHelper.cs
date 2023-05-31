using System;
using Abecombe.GPUBufferOperators;
using GPUUtil;
using Unity.Mathematics;
using UnityEngine;

public class GridSortHelper<Object> : IDisposable
{
    #region Properties
    private GPUComputeShader _gridSortHelperCS;
    private GPUBuffer<uint2> _objectCellIDPairBuffer = new();
    private GPURadixSort _radixSort = new();
    #endregion

    #region Sort Functions
    public void Sort(GPUDoubleBuffer<Object> objectBuffer, GPUBuffer<uint2> gridObjectIDBuffer, float3 gridMin, float3 gridMax, int3 gridSize, float3 gridSpacing)
    {
        _gridSortHelperCS ??= new GPUComputeShader(Resources.Load<ComputeShader>("GridSortHelperCS"), "MakeObjectCellIDPair", "ClearGridObjectID", "SetGridObjectID", "RearrangeObject");

        var cs = _gridSortHelperCS;
        var k_make = cs.Kernel[0];
        var k_clear = cs.Kernel[1];
        var k_set = cs.Kernel[2];
        var k_rearrange = cs.Kernel[3];

        int numObjects = objectBuffer.Size;
        // check for updates
        _objectCellIDPairBuffer.CheckSizeChanged(numObjects);
        int numGrids = gridObjectIDBuffer.Size;

        cs.SetInt("_NumObjects", numObjects);

        cs.SetVector("_GridMin", gridMin);
        cs.SetVector("_GridMax", gridMax);
        cs.SetInts("_GridSize", gridSize);
        cs.SetVector("_GridSpacing", gridSpacing);
        cs.SetVector("_GridInvSpacing", 1f / gridSpacing);

        // make <cellID, objectID> pair
        k_make.SetBuffer("_ObjectBufferRead", objectBuffer.Read);
        k_make.SetBuffer("_ObjectCellIDPairBufferWrite", _objectCellIDPairBuffer);
        k_make.Dispatch(numObjects);

        // sort
        _radixSort.Sort(_objectCellIDPairBuffer, (uint)numGrids - 1);

        // clear grid objectID
        k_clear.SetBuffer("_GridObjectIDBufferWrite", gridObjectIDBuffer);
        k_clear.Dispatch(numGrids);

        // set grid objectID
        k_set.SetBuffer("_ObjectCellIDPairBufferRead", _objectCellIDPairBuffer);
        k_set.SetBuffer("_GridObjectIDBufferWrite", gridObjectIDBuffer);
        k_set.Dispatch(numObjects);

        // rearrange object
        k_rearrange.SetBuffer("_ObjectCellIDPairBufferRead", _objectCellIDPairBuffer);
        k_rearrange.SetBuffer("_ObjectBufferRead", objectBuffer.Read);
        k_rearrange.SetBuffer("_ObjectBufferWrite", objectBuffer.Write);
        k_rearrange.Dispatch(numObjects);

        // swap object buffer
        objectBuffer.Swap();
    }
    #endregion

    #region Release Buffers
    public void Dispose()
    {
        _objectCellIDPairBuffer.Dispose();
        _radixSort.Dispose();
    }
    #endregion
}
