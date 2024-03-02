using System;
using UnityEngine;

namespace Abecombe.GPUBufferOperators
{
    public class GPUFiltering : IDisposable
    {
        private const int NumGroupThreads = 128;
        private const int NumElementsPerGroup = NumGroupThreads;

        private const int MaxDispatchSize = 65535;

        protected ComputeShader FilteringCs;
        private int _kernelRadixSortLocal;
        private int _kernelGlobalShuffle;

        private GPUPrefixScan _prefixScan = new();

        // buffer to store the locally sorted input data
        // size: number of data
        private GraphicsBuffer _tempBuffer;
        // buffer to store the number of filtered elements within locally sorted groups
        // size: number of groups
        private GraphicsBuffer _groupSumBuffer;
        // buffer to store the global prefix sums of filtered elements within locally sorted groups
        // size: number of groups
        private GraphicsBuffer _globalPrefixSumBuffer;

        private uint _numFilteredElements = 0;

        private bool _inited = false;

        protected virtual void LoadComputeShader()
        {
            FilteringCs = Resources.Load<ComputeShader>("FilteringCS");
        }

        private void Init()
        {
            if (!FilteringCs) LoadComputeShader();
            _kernelRadixSortLocal = FilteringCs.FindKernel("RadixSortLocal");
            _kernelGlobalShuffle = FilteringCs.FindKernel("GlobalShuffle");

            _inited = true;
        }

        /// <summary>
        /// Gather elements that meet certain condition to the front of the buffer
        /// </summary>
        /// <param name="dataBuffer">data buffer to be filtered</param>
        public void Filter(GraphicsBuffer dataBuffer)
        {
            Filter(dataBuffer, null, 0, false);
        }

        /// <summary>
        /// Gather elements that meet certain condition to the front of the buffer
        /// </summary>
        /// <param name="dataBuffer">data buffer to be filtered</param>
        /// <param name="numFilteredElements">the number of filtered elements</param>
        public void Filter(GraphicsBuffer dataBuffer, out uint numFilteredElements)
        {
            Filter(dataBuffer, null, 0, true);
            numFilteredElements = _numFilteredElements;
        }

        /// <summary>
        /// Gather elements that meet certain condition to the front of the buffer
        /// </summary>
        /// <param name="dataBuffer">data buffer to be filtered</param>
        /// <param name="numBuffer">uint buffer to store the number of filtered elements</param>
        /// <param name="bufferOffset">index of the element in the numBuffer to store the number of filtered elements</param>
        public void Filter(GraphicsBuffer dataBuffer, GraphicsBuffer numBuffer, uint bufferOffset = 0)
        {
            Filter(dataBuffer, numBuffer, bufferOffset, false);
        }

        /// <summary>
        /// Gather elements that meet certain condition to the front of the buffer
        /// </summary>
        /// <param name="dataBuffer">data buffer to be filtered</param>
        /// <param name="numBuffer">uint buffer to store the number of filtered elements</param>
        /// <param name="bufferOffset">index of the element in the numBuffer to store the number of filtered elements</param>
        /// <param name="returnNumFilteredElements">whether this function should return the number of filtered elements</param>
        private void Filter(GraphicsBuffer dataBuffer, GraphicsBuffer numBuffer, uint bufferOffset, bool returnNumFilteredElements = false)
        {
            if (!_inited) Init();

            var cs = FilteringCs;
            var k_local = _kernelRadixSortLocal;
            var k_shuffle = _kernelGlobalShuffle;

            int numElements = dataBuffer.count;
            int numGroups = (numElements + NumElementsPerGroup - 1) / NumElementsPerGroup;

            CheckBufferSizeChanged(numElements, numGroups, dataBuffer.stride);

            cs.SetInt("num_elements", numElements);
            cs.SetInt("num_groups", numGroups);

            // sort input data locally and output the number of filtered elements within groups
            cs.SetBuffer(k_local, "data_in_buffer", dataBuffer);
            cs.SetBuffer(k_local, "data_out_buffer", _tempBuffer);
            cs.SetBuffer(k_local, "group_sum_buffer", _groupSumBuffer);
            cs.SetBuffer(k_local, "global_prefix_sum_buffer", _globalPrefixSumBuffer);
            for (int i = 0; i < numGroups; i += MaxDispatchSize)
            {
                cs.SetInt("group_offset", i);
                cs.Dispatch(k_local, Mathf.Min(numGroups - i, MaxDispatchSize), 1, 1);
            }

            // prefix scan global group sum data
            if (numBuffer is not null)
                _prefixScan.Scan(_globalPrefixSumBuffer, numBuffer, bufferOffset);
            else if (returnNumFilteredElements)
                _prefixScan.Scan(_globalPrefixSumBuffer, out _numFilteredElements);
            else
                _prefixScan.Scan(_globalPrefixSumBuffer);

            // copy input data to final position in global memory
            cs.SetBuffer(k_shuffle, "data_in_buffer", _tempBuffer);
            cs.SetBuffer(k_shuffle, "data_out_buffer", dataBuffer);
            cs.SetBuffer(k_shuffle, "group_sum_buffer", _groupSumBuffer);
            cs.SetBuffer(k_shuffle, "global_prefix_sum_buffer", _globalPrefixSumBuffer);
            for (int i = 0; i < numGroups; i += MaxDispatchSize)
            {
                cs.SetInt("group_offset", i);
                cs.Dispatch(k_shuffle, Mathf.Min(numGroups - i, MaxDispatchSize), 1, 1);
            }
        }

        private void CheckBufferSizeChanged(int numElements, int numGroups, int bufferStride)
        {
            if (_tempBuffer is null || _tempBuffer.count < numElements || _tempBuffer.stride != bufferStride)
            {
                _tempBuffer?.Release();
                _tempBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numElements, bufferStride);
            }
            if (_groupSumBuffer is null || _groupSumBuffer.count < numGroups)
            {
                _groupSumBuffer?.Release();
                _groupSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numGroups, sizeof(uint));
            }
            if (_globalPrefixSumBuffer is null || _globalPrefixSumBuffer.count != numGroups)
            {
                _globalPrefixSumBuffer?.Release();
                _globalPrefixSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numGroups, sizeof(uint));
            }
        }

        /// <summary>
        /// Release buffers
        /// </summary>
        public void Dispose()
        {
            if (_tempBuffer is not null) { _tempBuffer.Release(); _tempBuffer = null; }
            if (_groupSumBuffer is not null) { _groupSumBuffer.Release(); _groupSumBuffer = null; }
            if (_globalPrefixSumBuffer is not null) { _globalPrefixSumBuffer.Release(); _globalPrefixSumBuffer = null; }

            _prefixScan.Dispose();
        }
    }
}