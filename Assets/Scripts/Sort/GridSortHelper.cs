using System;
using Abecombe.GPUBufferOperators;
using Abecombe.GPUUtil;
using Unity.Mathematics;

public class GridSortHelper<Object> : IDisposable
{
    #region Properties
    private GPUComputeShader _gridSortHelperCs;
    private GPUBuffer<uint2> _objectCellIDPairBuffer = new();
    private GPURadixSort _radixSort = new();
    #endregion

    #region Sort Functions
    public void Sort(GPUDoubleBuffer<Object> objectBuffer, GPUBuffer<uint2> gridObjectIDBuffer, float3 gridMin, float3 gridMax, int3 gridSize, float3 gridSpacing)
    {
        _gridSortHelperCs ??= new GPUComputeShader("GridSortHelperCS");

        var cs = _gridSortHelperCs;
        var k_make = cs.FindKernel("MakeObjectCellIDPair");
        var k_clear = cs.FindKernel("ClearGridObjectID");
        var k_set = cs.FindKernel("SetGridObjectID");
        var k_rearrange = cs.FindKernel("RearrangeObject");

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