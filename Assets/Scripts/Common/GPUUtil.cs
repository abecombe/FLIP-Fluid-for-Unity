using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace GPUUtil
{
    public class GPUDoubleBuffer<T> : IDisposable
    {
        public GPUBuffer<T> Read => _bufferRead;
        public GPUBuffer<T> Write => _bufferWrite;
        public int Size => _bufferRead.Size;

        private GPUBuffer<T> _bufferRead = new();
        private GPUBuffer<T> _bufferWrite = new();
        private bool _inited = false;

        public void Init(int size)
        {
            Dispose();
            _bufferRead.Init(size);
            _bufferWrite.Init(size);
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _bufferRead.Dispose();
            if (_inited) _bufferWrite.Dispose();
            _inited = false;
        }

        public void CheckSizeChanged(int size)
        {
            if (!_inited || Size != size)
            {
                Init(size);
            }
        }

        public void Swap()
        {
            (_bufferRead, _bufferWrite) = (_bufferWrite, _bufferRead);
        }

        public void SetData(T[] data)
        {
            _bufferRead.SetData(data);
        }
        public void SetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            _bufferRead.SetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetReadData(T[] data)
        {
            _bufferRead.GetData(data);
        }
        public void GetReadData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            _bufferRead.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetWriteData(T[] data)
        {
            _bufferWrite.GetData(data);
        }
        public void GetWriteData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            _bufferWrite.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
    }

    public class GPUBuffer<T> : IDisposable
    {
        public GraphicsBuffer Data => _buffer;
        public int Size => _buffer.count;
        public int Bytes => _buffer.count * _buffer.stride;

        private GraphicsBuffer _buffer;
        private bool _inited = false;

        public void Init(int size)
        {
            Dispose();
            _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, Marshal.SizeOf(typeof(T)));
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _buffer.Release();
            _inited = false;
        }

        public void CheckSizeChanged(int size)
        {
            if (!_inited || Size != size)
            {
                Init(size);
            }
        }

        public void SetData(T[] data)
        {
            _buffer.SetData(data);
        }
        public void SetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            _buffer.SetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetData(T[] data)
        {
            _buffer.GetData(data);
        }
        public void GetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            _buffer.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }

        public static implicit operator GraphicsBuffer(GPUBuffer<T> buffer)
        {
            return buffer.Data;
        }
    }

    public class GPUComputeShader
    {
        private ComputeShader _cs;
        private GPUKernel[] _kernel;
        public GPUKernel[] Kernel => _kernel;

        public GPUComputeShader(ComputeShader cs, params string[] names)
        {
            _cs = cs;
            _kernel = names.Select(name => new GPUKernel(cs, name)).ToArray();
        }

        #region SetInt
        public void SetInt(string name, int value)
        {
            _cs.SetInt(name, value);
        }
        public void SetInt(string name, uint value)
        {
            _cs.SetInt(name, (int)value);
        }
        #endregion

        #region SetInts
        public void SetInts(string name, int2 value)
        {
            _cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, int3 value)
        {
            _cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, int4 value)
        {
            _cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, params int[] value)
        {
            _cs.SetInts(name, value);
        }
        #endregion

        #region SetFloat
        public void SetFloat(string name, int value)
        {
            _cs.SetFloat(name, value);
        }
        public void SetFloat(string name, float value)
        {
            _cs.SetFloat(name, value);
        }
        #endregion

        #region SetVector
        public void SetVector(string name, int2 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, int3 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, int4 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float2 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float3 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float4 value)
        {
            _cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, Vector2 value)
        {
            _cs.SetVector(name, value);
        }
        public void SetVector(string name, Vector3 value)
        {
            _cs.SetVector(name, value);
        }
        public void SetVector(string name, Vector4 value)
        {
            _cs.SetVector(name, value);
        }
        #endregion

        #region SetMatrix
        public void SetMatrix(string name, Matrix4x4 matrix)
        {
            _cs.SetMatrix(name, matrix);
        }
        #endregion

        #region SetKeyword
        public void EnableKeyword(string keyword)
        {
            _cs.EnableKeyword(keyword);
        }
        public void DisableKeyword(string keyword)
        {
            _cs.DisableKeyword(keyword);
        }
        #endregion
    }

    public class GPUKernel
    {
        private ComputeShader _cs;
        private string _name;
        private int _id;
        private uint3 _threadSize;

        public GPUKernel(ComputeShader cs, string name)
        {
            _cs = cs;
            _name = name;
            _id = cs.FindKernel(name);
            cs.GetKernelThreadGroupSizes(_id, out _threadSize.x, out _threadSize.y, out _threadSize.z);
        }

        #region SetBuffer
        public void SetBuffer(string name, GraphicsBuffer buffer)
        {
            _cs.SetBuffer(_id, name, buffer);
        }
        public void SetBuffer(string name, ComputeBuffer buffer)
        {
            _cs.SetBuffer(_id, name, buffer);
        }
        #endregion

        #region SetTexture
        public void SetTexture(string name, Texture tex)
        {
            _cs.SetTexture(_id, name, tex);
        }
        #endregion

        #region Dispatch
        public void Dispatch(int sizeX)
        {
            Dispatch(new int3(sizeX, 1, 1));
        }
        public void Dispatch(int sizeX, int sizeY, int sizeZ)
        {
            Dispatch(new int3(sizeX, sizeY, sizeZ));
        }
        public void Dispatch(int3 size)
        {
            var groupSize = (int3)math.ceil(size / (float3)_threadSize);
            _cs.SetInts("_NumThreads", size.ToInts());
            _cs.SetInts("_NumGroups", groupSize.ToInts());
            _cs.Dispatch(_id, groupSize.x, groupSize.y, groupSize.z);
        }

        public void DispatchGroup(int sizeX)
        {
            DispatchGroup(new int3(sizeX, 1, 1));
        }
        public void DispatchGroup(int sizeX, int sizeY, int sizeZ)
        {
            DispatchGroup(new int3(sizeX, sizeY, sizeZ));
        }
        public void DispatchGroup(int3 size)
        {
            _cs.SetInts("_NumGroups", size.ToInts());
            _cs.Dispatch(_id, size.x, size.y, size.z);
        }
        #endregion
    }

    public static class GPUCastTool
    {
        public static int[] ToInts(this int2 i2) => new int[] { i2.x, i2.y };
        public static int[] ToInts(this int3 i3) => new int[] { i3.x, i3.y, i3.z };
        public static int[] ToInts(this int4 i4) => new int[] { i4.x, i4.y, i4.z, i4.w };
        public static Vector4 ToVector(this int2 i2) => new Vector4(i2.x, i2.y);
        public static Vector4 ToVector(this int3 i3) => new Vector4(i3.x, i3.y, i3.z);
        public static Vector4 ToVector(this int4 i4) => new Vector4(i4.x, i4.y, i4.z, i4.w);
        public static Vector4 ToVector(this float2 f2) => new Vector4(f2.x, f2.y);
        public static Vector4 ToVector(this float3 f3) => new Vector4(f3.x, f3.y, f3.z);
        public static Vector4 ToVector(this float4 f4) => new Vector4(f4.x, f4.y, f4.z, f4.w);
    }
}