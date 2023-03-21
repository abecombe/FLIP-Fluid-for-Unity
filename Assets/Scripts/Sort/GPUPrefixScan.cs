using System.Collections.Generic;
using UnityEngine;

namespace Abecombe.GPUBufferOperators
{
    public class GPUPrefixScan
    {
        private static readonly int _max_dispatch_size = 65535;

        protected ComputeShader _prefixScanCS;
        private int _kernelPrefixScan;
        private int _kernelAddGroupSum;

        // buffers to store the sum of values within local groups
        // size: number of groups
        private List<GraphicsBuffer> _groupSumBufferList = new();
        // buffer to store the total sum of values
        // size: 1
        private GraphicsBuffer _totalSumBuffer;

        private uint _totalSum = 0;

        private bool _inited = false;

        protected virtual void LoadComputeShader()
        {
            _prefixScanCS = Resources.Load<ComputeShader>("PrefixScanCS");
        }

        private void Init()
        {
            if (!_prefixScanCS) LoadComputeShader();
            _kernelPrefixScan = _prefixScanCS.FindKernel("PrefixScan");
            _kernelAddGroupSum = _prefixScanCS.FindKernel("AddGroupSum");

            _totalSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(uint));

            _inited = true;
        }

        // Implementation of Article "Chapter 39. Parallel Prefix Sum (Scan) with CUDA"
        // https://developer.nvidia.com/gpugems/gpugems3/part-vi-gpu-computing/chapter-39-parallel-prefix-sum-scan-cuda

        // dataBuffer
        // : data<uint> buffer to be scaned
        // returnTotalSum
        // : whether this function should return the total sum of values
        // return value
        // : the total sum of values (only when returnTotalSum is true)
        public uint Scan(GraphicsBuffer dataBuffer, bool returnTotalSum = false)
        {
            Scan(dataBuffer, null, 0, returnTotalSum, 0);

            return _totalSum;
        }

        // dataBuffer
        // : data<uint> buffer to be scaned
        // totalSumBuffer
        // : data<uint> buffer to store the total sum
        // bufferOffset
        // : index of the element in the totalSumBuffer to store the total sum
        // returnTotalSum
        // : whether this function should return the total sum of values
        // return value
        // : the total sum of values (only when returnTotalSum is true)
        public uint Scan(GraphicsBuffer dataBuffer, GraphicsBuffer totalSumBuffer, uint bufferOffset, bool returnTotalSum = false)
        {
            Scan(dataBuffer, totalSumBuffer, bufferOffset, returnTotalSum, 0);

            return _totalSum;
        }

        private void Scan(GraphicsBuffer dataBuffer, GraphicsBuffer totalSumBuffer, uint bufferOffset, bool returnTotalSum, int bufferIndex)
        {
            if (!_inited) Init();

            if (totalSumBuffer == null) totalSumBuffer = _totalSumBuffer;

            var cs = _prefixScanCS;
            var k_scan = _kernelPrefixScan;
            var k_add = _kernelAddGroupSum;

            int numElements = dataBuffer.count;

            int numGroupThreads = SetNumGroupThreads(cs, numElements);
            int numElementsPerGroup = 2 * numGroupThreads;

            int numGroups = (numElements + numElementsPerGroup - 1) / numElementsPerGroup;

            GraphicsBuffer groupSumBuffer;
            if (_groupSumBufferList.Count <= bufferIndex)
            {
                groupSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numGroups, sizeof(uint));
                _groupSumBufferList.Add(groupSumBuffer);
            }
            else
            {
                groupSumBuffer = _groupSumBufferList[bufferIndex];
                if (groupSumBuffer == null || groupSumBuffer.count != numGroups)
                {
                    groupSumBuffer?.Release();
                    groupSumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numGroups, sizeof(uint));
                    _groupSumBufferList[bufferIndex] = groupSumBuffer;
                }
            }

            // scan input data locally and output total sums within groups
            cs.SetInt("num_elements", numElements);
            cs.SetBuffer(k_scan, "data_buffer", dataBuffer);
            cs.SetBuffer(k_scan, "group_sum_buffer", groupSumBuffer);
            cs.SetInt("group_sum_offset", 0);
            for (int i = 0; i < numGroups; i += _max_dispatch_size)
            {
                cs.SetInt("group_offset", i);
                cs.Dispatch(k_scan, Mathf.Min(numGroups - i, _max_dispatch_size), 1, 1);
            }

            // scan group total sums
            if (numGroups <= numElementsPerGroup)
            {
                cs.SetInt("num_elements", numGroups);
                cs.SetInt("group_offset", 0);
                cs.SetBuffer(k_scan, "data_buffer", groupSumBuffer);
                cs.SetBuffer(k_scan, "group_sum_buffer", totalSumBuffer);
                cs.SetInt("group_sum_offset", (int)bufferOffset);
                cs.Dispatch(k_scan, 1, 1, 1);

                if (returnTotalSum)
                {
                    uint[] totalSumArr = new uint[1];
                    totalSumBuffer.GetData(totalSumArr, 0, (int)bufferOffset, 1);
                    _totalSum = totalSumArr[0];
                }
            }
            // execute this function recursively
            else
            {
                Scan(groupSumBuffer, totalSumBuffer, bufferOffset, returnTotalSum, bufferIndex + 1);
            }

            // add each group's total sum to its scan output
            SetNumGroupThreads(cs, numElements);
            cs.SetInt("num_elements", numElements);
            cs.SetBuffer(k_add, "data_buffer", dataBuffer);
            cs.SetBuffer(k_add, "group_sum_buffer", groupSumBuffer);
            for (int i = 0; i < numGroups; i += _max_dispatch_size)
            {
                cs.SetInt("group_offset", i);
                cs.Dispatch(k_add, Mathf.Min(numGroups - i, _max_dispatch_size), 1, 1);
            }
        }

        // changing the number of group threads according to the number of data to reduce the number of nests
        private int SetNumGroupThreads(ComputeShader cs, int numElements)
        {
            if (numElements <= 65536)
            {
                cs.EnableKeyword("NUM_GROUP_THREADS_128");
                cs.DisableKeyword("NUM_GROUP_THREADS_256");
                cs.DisableKeyword("NUM_GROUP_THREADS_512");
                return 128;
            }
            else if (numElements <= 262144)
            {
                cs.DisableKeyword("NUM_GROUP_THREADS_128");
                cs.EnableKeyword("NUM_GROUP_THREADS_256");
                cs.DisableKeyword("NUM_GROUP_THREADS_512");
                return 256;
            }
            else
            {
                cs.DisableKeyword("NUM_GROUP_THREADS_128");
                cs.DisableKeyword("NUM_GROUP_THREADS_256");
                cs.EnableKeyword("NUM_GROUP_THREADS_512");
                return 512;
            }
        }

        public void ReleaseBuffers()
        {
            foreach (var groupSumBuffer in _groupSumBufferList)
                groupSumBuffer?.Release();
            _totalSumBuffer?.Release();
        }
    }
}
