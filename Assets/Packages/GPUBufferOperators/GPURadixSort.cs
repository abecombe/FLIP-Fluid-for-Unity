using System;
using UnityEngine;

namespace Abecombe.GPUBufferOperators
{
    public class GPURadixSort : IDisposable
    {
        private const int NumGroupThreads = 128;
        private const int NumElementsPerGroup = NumGroupThreads;

        private const int MaxDispatchSize = 65535;

        protected ComputeShader RadixSortCs;
        private int _kernelRadixSortLocal;
        private int _kernelGlobalShuffle;

        private GPUPrefixScan _prefixScan = new();

        // buffer to store the locally sorted input data
        // size: number of data
        private GraphicsBuffer _tempBuffer;
        // buffer to store the first index of each 2bit key-value (0, 1, 2, 3) within locally sorted groups
        // size: 4 * number of groups
        private GraphicsBuffer _firstIndexBuffer;
        // buffer to store the sums of each 2bit key-value (0, 1, 2, 3) within locally sorted groups
        // size: 4 * number of groups
        private GraphicsBuffer _groupSumBuffer;

        private bool _inited = false;

        protected virtual void LoadComputeShader()
        {
            RadixSortCs = Resources.Load<ComputeShader>("RadixSortCS");
        }

        private void Init()
        {
            if (!RadixSortCs) LoadComputeShader();
            _kernelRadixSortLocal = RadixSortCs.FindKernel("RadixSortLocal");
            _kernelGlobalShuffle = RadixSortCs.FindKernel("GlobalShuffle");

            _inited = true;
        }

        // Implementation of Paper "Fast 4-way parallel radix sorting on GPUs"
        // https://vgc.poly.edu/~csilva/papers/cgf.pdf

        // GPURadixSort has O(n * s * w) complexity
        // n : number of data
        // s : size of data struct
        // w : number of bits to sort

        /// <summary>
        /// Sort data buffer in ascending order
        /// </summary>
        /// <param name="dataBuffer">data buffer to be sorted</param>
        /// <param name="maxValue">maximum key-value</param>
        public void Sort(GraphicsBuffer dataBuffer, uint maxValue = uint.MaxValue)
        {
            if (!_inited) Init();

            var cs = RadixSortCs;
            var k_local = _kernelRadixSortLocal;
            var k_shuffle = _kernelGlobalShuffle;

            int numElements = dataBuffer.count;
            int numGroups = (numElements + NumElementsPerGroup - 1) / NumElementsPerGroup;

            CheckBufferSizeChanged(numElements, numGroups, dataBuffer.stride);

            cs.SetInt("num_elements", numElements);
            cs.SetInt("num_groups", numGroups);

            cs.SetBuffer(k_local, "data_in_buffer", dataBuffer);
            cs.SetBuffer(k_local, "data_out_buffer", _tempBuffer);
            cs.SetBuffer(k_local, "first_index_buffer", _firstIndexBuffer);
            cs.SetBuffer(k_local, "group_sum_buffer", _groupSumBuffer);

            cs.SetBuffer(k_shuffle, "data_in_buffer", _tempBuffer);
            cs.SetBuffer(k_shuffle, "data_out_buffer", dataBuffer);
            cs.SetBuffer(k_shuffle, "first_index_buffer", _firstIndexBuffer);
            cs.SetBuffer(k_shuffle, "global_prefix_sum_buffer", _groupSumBuffer);

            int firstBitHigh = Convert.ToString(maxValue, 2).Length;
            for (int bitShift = 0; bitShift < firstBitHigh; bitShift += 2)
            {
                cs.SetInt("bit_shift", bitShift);

                // sort input data locally and output first-index / sums of each 2bit key-value within groups
                for (int i = 0; i < numGroups; i += MaxDispatchSize)
                {
                    cs.SetInt("group_offset", i);
                    cs.Dispatch(k_local, Mathf.Min(numGroups - i, MaxDispatchSize), 1, 1);
                }

                // prefix scan global group sum data
                _prefixScan.Scan(_groupSumBuffer);

                // copy input data to final position in global memory
                for (int i = 0; i < numGroups; i += MaxDispatchSize)
                {
                    cs.SetInt("group_offset", i);
                    cs.Dispatch(k_shuffle, Mathf.Min(numGroups - i, MaxDispatchSize), 1, 1);
                }
            }
        }

        private void CheckBufferSizeChanged(int numElements, int numGroups, int bufferStride)
        {
            if (_tempBuffer is null || _tempBuffer.count < numElements || _tempBuffer.stride != bufferStride)
            {
                _tempBuffer?.Release();
                _tempBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numElements, bufferStride);
            }
            if (_firstIndexBuffer is null || _firstIndexBuffer.count < 4 * numGroups)
            {
                _firstIndexBuffer?.Release();
                _firstIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4 * numGroups, sizeof(uint));
            }
            if (_groupSumBuffer is null || _groupSumBuffer.count != 4 * numGroups)
            {
                _groupSumBuffer?.Release();
                _groupSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4 * numGroups, sizeof(uint));
            }
        }

        /// <summary>
        /// Release buffers
        /// </summary>
        public void Dispose()
        {
            if (_tempBuffer is not null) { _tempBuffer.Release(); _tempBuffer = null; }
            if (_firstIndexBuffer is not null) { _firstIndexBuffer.Release(); _firstIndexBuffer = null; }
            if (_groupSumBuffer is not null) { _groupSumBuffer.Release(); _groupSumBuffer = null; }

            _prefixScan.Dispose();
        }
    }
}