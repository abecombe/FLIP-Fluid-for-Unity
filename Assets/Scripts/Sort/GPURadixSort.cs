﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace Abecombe.GPUBufferOperators
{
    public class GPURadixSort<T>
    {
        private static readonly int _numGroupThreads = 128;
        private static readonly int _numElementsPerGroup = _numGroupThreads;

        private static readonly int _max_dispatch_size = 65535;

        protected ComputeShader _radixSortCS;
        private int _kernelRadixSortLocal;
        private int _kernelGlobalShuffle;

        private readonly GPUPrefixScan _prefixScan = new();

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
            _radixSortCS = Resources.Load<ComputeShader>("RadixSortCS");
        }

        private void Init()
        {
            if (!_radixSortCS) LoadComputeShader();
            _kernelRadixSortLocal = _radixSortCS.FindKernel("RadixSortLocal");
            _kernelGlobalShuffle = _radixSortCS.FindKernel("GlobalShuffle");

            _inited = true;
        }

        // Implementation of Paper "Fast 4-way parallel radix sorting on GPUs"
        // https://vgc.poly.edu/~csilva/papers/cgf.pdf

        // GPURadixSort has O(n * s * w) complexity
        // n : number of data
        // s : size of data struct
        // w : number of bits

        // dataBuffer
        // : data<T> buffer to be sorted
        // : please define the data struct & how to get the key-values in "CustomDefinition.hlsl".
        // maxValue
        // : maximum key-value
        // : since this variable directly related to the complexity,
        // : passing this argument will reduce the cost of sorting.
        public void Sort(GraphicsBuffer dataBuffer, uint maxValue = uint.MaxValue)
        {
            if (!_inited) Init();

            var cs = _radixSortCS;
            var k_local = _kernelRadixSortLocal;
            var k_shuffle = _kernelGlobalShuffle;

            int numElements = dataBuffer.count;
            int numGroups = (numElements + _numElementsPerGroup - 1) / _numElementsPerGroup;

            CheckBufferSizeChanged(numElements, numGroups);

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

            int firstBitHigh = System.Convert.ToString(maxValue, 2).Length;
            for (int bitShift = 0; bitShift < firstBitHigh; bitShift += 2)
            {
                cs.SetInt("bit_shift", bitShift);

                // sort input data locally and output first-index / sums of each 2bit key-value within groups
                for (int i = 0; i < numGroups; i += _max_dispatch_size)
                {
                    cs.SetInt("group_offset", i);
                    cs.Dispatch(k_local, Mathf.Min(numGroups - i, _max_dispatch_size), 1, 1);
                }

                // prefix scan global group sum data
                _prefixScan.Scan(_groupSumBuffer);

                // copy input data to final position in global memory
                for (int i = 0; i < numGroups; i += _max_dispatch_size)
                {
                    cs.SetInt("group_offset", i);
                    cs.Dispatch(k_shuffle, Mathf.Min(numGroups - i, _max_dispatch_size), 1, 1);
                }
            }
        }

        private void CheckBufferSizeChanged(int numElements, int numGroups)
        {
            if (_tempBuffer == null || _tempBuffer.count != numElements)
            {
                _tempBuffer?.Release();
                _tempBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numElements, Marshal.SizeOf(typeof(T)));
            }
            if (_firstIndexBuffer == null || _firstIndexBuffer.count != 4 * numGroups)
            {
                _firstIndexBuffer?.Release();
                _firstIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4 * numGroups, sizeof(uint));
            }
            if (_groupSumBuffer == null || _groupSumBuffer.count != 4 * numGroups)
            {
                _groupSumBuffer?.Release();
                _groupSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 4 * numGroups, sizeof(uint));
            }
        }

        public void ReleaseBuffers()
        {
            _tempBuffer?.Release();
            _firstIndexBuffer?.Release();
            _groupSumBuffer?.Release();

            _prefixScan?.ReleaseBuffers();
        }
    }
}