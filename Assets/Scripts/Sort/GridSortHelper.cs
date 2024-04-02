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
    public void Sort(GPUDoubleBuffer<Object> objectBuffer, GPUBuffer<uint2> gridObjectIDBuffer, float3 gridMin, float3 gridMax, int3 gridSize, float gridSpacing)
    {
        _gridSortHelperCs ??= new GPUComputeShader("GridSortHelperCS");

        int numObjects = objectBuffer.Size;
        // check for updates
        _objectCellIDPairBuffer.CheckSizeChanged(numObjects);
        int numGrids = gridObjectIDBuffer.Size;

        var cs = _gridSortHelperCs;

        cs.SetInt("_NumObjects", numObjects);

        cs.SetVector("_GridMin", gridMin);
        cs.SetVector("_GridMax", gridMax);
        cs.SetInts("_GridSize", gridSize);
        cs.SetFloat("_GridSpacing", gridSpacing);
        cs.SetFloat("_GridInvSpacing", 1f / gridSpacing);

        // make <cellID, objectID> pair
        var k = cs.FindKernel("MakeObjectCellIDPair");
        k.SetBuffer("_ObjectBufferRead", objectBuffer.Read);
        k.SetBuffer("_ObjectCellIDPairBufferWrite", _objectCellIDPairBuffer);
        k.Dispatch(numObjects);

        // sort
        _radixSort.Sort(_objectCellIDPairBuffer, (uint)numGrids - 1);

        // clear grid objectID
        k = cs.FindKernel("ClearGridObjectID");
        k.SetBuffer("_GridObjectIDBufferWrite", gridObjectIDBuffer);
        k.Dispatch(numGrids);

        // set grid objectID
        k = cs.FindKernel("SetGridObjectID");
        k.SetBuffer("_ObjectCellIDPairBufferRead", _objectCellIDPairBuffer);
        k.SetBuffer("_GridObjectIDBufferWrite", gridObjectIDBuffer);
        k.Dispatch(numObjects);

        // rearrange object
        k = cs.FindKernel("RearrangeObject");
        k.SetBuffer("_ObjectCellIDPairBufferRead", _objectCellIDPairBuffer);
        k.SetBuffer("_ObjectBufferRead", objectBuffer.Read);
        k.SetBuffer("_ObjectBufferWrite", objectBuffer.Write);
        k.Dispatch(numObjects);

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