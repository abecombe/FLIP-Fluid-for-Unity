using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abecombe.GPUUtil
{
    public class GPUBuffer<T> : IDisposable
    {
        public GraphicsBuffer Data { get; private set; }
        public int Size => Data.count;
        public int Stride => Data.stride;
        public int Bytes => Size * Stride;

        private bool _inited = false;

        public void Init(int size)
        {
            Dispose();
            Data = new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, Marshal.SizeOf(typeof(T)));
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) Data.Release();
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
            Data.SetData(data);
        }
        public void SetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            Data.SetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetData(T[] data)
        {
            Data.GetData(data);
        }
        public void GetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            Data.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }

        public static implicit operator GraphicsBuffer(GPUBuffer<T> buffer)
        {
            return buffer.Data;
        }
    }

    public class GPUDoubleBuffer<T> : IDisposable
    {
        public GPUBuffer<T> Read { get; private set; } = new();
        public GPUBuffer<T> Write { get; private set; } = new();
        public int Size => Read.Size;
        public int Stride => Read.Stride;
        public int Bytes => Read.Bytes;

        private GPUComputeShader _gpuUtilCs;
        private bool _inited = false;

        public void Init(int size)
        {
            Dispose();
            Read.Init(size);
            Write.Init(size);
            _gpuUtilCs = new GPUComputeShader("GPUUtil");
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) Read.Dispose();
            if (_inited) Write.Dispose();
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
            (Read, Write) = (Write, Read);
        }

        public void CopyReadToWrite()
        {
            var cs = _gpuUtilCs;
            var kernel = cs.FindKernel("CopyBuffer");
            cs.SetInt("_BufferSize", Size);
            cs.SetInt("_BufferBytes", Bytes);
            kernel.SetBuffer("_BufferRead", Read);
            kernel.SetBuffer("_BufferWrite", Write);
            kernel.Dispatch(Size);
        }

        public void SetData(T[] data)
        {
            Read.SetData(data);
        }
        public void SetData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            Read.SetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetReadData(T[] data)
        {
            Read.GetData(data);
        }
        public void GetReadData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            Read.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
        public void GetWriteData(T[] data)
        {
            Write.GetData(data);
        }
        public void GetWriteData(T[] data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
        {
            Write.GetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
        }
    }

    public class GPUTexture2D : IDisposable
    {
        public RenderTexture Data { get; private set; }
        public int Width => Data.width;
        public int Height => Data.height;
        public RenderTextureFormat Format => Data.format;
        public FilterMode FilterMode => Data.filterMode;
        public TextureWrapMode WrapMode => Data.wrapMode;

        private bool _inited = false;

        public void Init(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            Data = new RenderTexture(width, height, 0, format)
            {
                filterMode = filterMode,
                wrapMode = wrapMode,
                enableRandomWrite = true
            };
            Data.Create();
            _inited = true;
        }
        public void Init(int2 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Init(size.x, size.y, format, filterMode, wrapMode);
        }

        public void Dispose()
        {
            if (_inited) Data.Release();
            _inited = false;
        }

        public void CheckSizeChanged(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            if (!_inited)
            {
                Init(width, height, format, filterMode, wrapMode);
            }
            else if (Width != width || Height != height)
            {
                Init(width, height, Format, FilterMode, WrapMode);
            }
        }
        public void CheckSizeChanged(int2 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            CheckSizeChanged(size.x, size.y, format, filterMode, wrapMode);
        }

        public static implicit operator RenderTexture(GPUTexture2D tex)
        {
            return tex.Data;
        }

        public static implicit operator RenderTargetIdentifier(GPUTexture2D tex)
        {
            return tex.Data;
        }
    }

    public class GPUDoubleTexture2D : IDisposable
    {
        public GPUTexture2D Read { get; private set; } = new();
        public GPUTexture2D Write { get; private set; } = new();
        public int Width => Read.Width;
        public int Height => Read.Height;
        public int2 Size => new(Width, Height);
        public RenderTextureFormat Format => Read.Format;
        public FilterMode FilterMode => Read.FilterMode;
        public TextureWrapMode WrapMode => Read.WrapMode;

        private bool _inited = false;

        public void Init(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            Read.Init(width, height, format, filterMode, wrapMode);
            Write.Init(width, height, format, filterMode, wrapMode);
            _inited = true;
        }
        public void Init(int2 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Init(size.x, size.y, format, filterMode, wrapMode);
        }

        public void Dispose()
        {
            if (_inited) Read.Dispose();
            if (_inited) Write.Dispose();
            _inited = false;
        }

        public void CheckSizeChanged(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            if (!_inited)
            {
                Init(width, height, format, filterMode, wrapMode);
            }
            else if (Width != width || Height != height)
            {
                Init(width, height, Format, FilterMode, WrapMode);
            }
        }
        public void CheckSizeChanged(int2 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            CheckSizeChanged(size.x, size.y, format, filterMode, wrapMode);
        }

        public void Swap()
        {
            (Read, Write) = (Write, Read);
        }
    }

    public class GPUTexture3D : IDisposable
    {
        public RenderTexture Data { get; private set; }
        public int Width => Data.width;
        public int Height => Data.height;
        public int Depth => Data.volumeDepth;
        public int3 Size => new(Width, Height, Depth);
        public RenderTextureFormat Format => Data.format;
        public FilterMode FilterMode => Data.filterMode;
        public TextureWrapMode WrapMode => Data.wrapMode;

        private bool _inited = false;

        public void Init(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            Data = new RenderTexture(width, height, 0, format)
            {
                filterMode = filterMode,
                wrapMode = wrapMode,
                dimension = TextureDimension.Tex3D,
                volumeDepth = depth,
                enableRandomWrite = true
            };
            Data.Create();
            _inited = true;
        }
        public void Init(int3 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Init(size.x, size.y, size.z, format, filterMode, wrapMode);
        }

        public void Dispose()
        {
            if (_inited) Data.Release();
            _inited = false;
        }

        public void CheckSizeChanged(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            if (!_inited)
            {
                Init(width, height, depth, format, filterMode, wrapMode);
            }
            else if (Width != width || Height != height || Depth != depth)
            {
                Init(width, height, depth, Format, FilterMode, WrapMode);
            }
        }
        public void CheckSizeChanged(int3 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            CheckSizeChanged(size.x, size.y, size.z, format, filterMode, wrapMode);
        }

        public static implicit operator RenderTexture(GPUTexture3D tex)
        {
            return tex.Data;
        }

        public static implicit operator RenderTargetIdentifier(GPUTexture3D tex)
        {
            return tex.Data;
        }
    }

    public class GPUDoubleTexture3D : IDisposable
    {
        public GPUTexture3D Read { get; private set; } = new();
        public GPUTexture3D Write { get; private set; } = new();
        public int Width => Read.Width;
        public int Height => Read.Height;
        public int Depth => Read.Depth;
        public int3 Size => new(Width, Height, Depth);
        public RenderTextureFormat Format => Read.Format;
        public FilterMode FilterMode => Read.FilterMode;
        public TextureWrapMode WrapMode => Read.WrapMode;

        private bool _inited = false;

        public void Init(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            Read.Init(width, height, depth, format, filterMode, wrapMode);
            Write.Init(width, height, depth, format, filterMode, wrapMode);
            _inited = true;
        }
        public void Init(int3 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Init(size.x, size.y, size.z, format, filterMode, wrapMode);
        }

        public void Dispose()
        {
            if (_inited) Read.Dispose();
            if (_inited) Write.Dispose();
            _inited = false;
        }

        public void CheckSizeChanged(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            if (!_inited)
            {
                Init(width, height, depth, format, filterMode, wrapMode);
            }
            else if (Width != width || Height != height || Depth != depth)
            {
                Init(width, height, depth, Format, FilterMode, WrapMode);
            }
        }
        public void CheckSizeChanged(int3 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            CheckSizeChanged(size.x, size.y, size.z, format, filterMode, wrapMode);
        }

        public void Swap()
        {
            (Read, Write) = (Write, Read);
        }
    }

    public class GPUComputeShader
    {
        public ComputeShader Cs { get; }

        private Dictionary<string, GPUKernel> _kernels = new();

        public GPUComputeShader(ComputeShader cs)
        {
            Cs = cs;
        }
        public GPUComputeShader(string csName)
        {
            Cs = Resources.Load<ComputeShader>(csName);
        }

        public GPUKernel FindKernel(string name)
        {
            if (_kernels.TryGetValue(name, out var kernel))
                return kernel;

            kernel = new GPUKernel(Cs, name);
            _kernels.Add(name, kernel);
            return kernel;
        }

        #region SetInt
        public void SetInt(string name, int value)
        {
            Cs.SetInt(name, value);
        }
        public void SetInt(string name, uint value)
        {
            Cs.SetInt(name, (int)value);
        }
        #endregion

        #region SetInts
        public void SetInts(string name, int2 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, int3 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, int4 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, uint2 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, uint3 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, uint4 value)
        {
            Cs.SetInts(name, value.ToInts());
        }
        public void SetInts(string name, params int[] value)
        {
            Cs.SetInts(name, value);
        }
        #endregion

        #region SetFloat
        public void SetFloat(string name, int value)
        {
            Cs.SetFloat(name, value);
        }
        public void SetFloat(string name, uint value)
        {
            Cs.SetFloat(name, value);
        }
        public void SetFloat(string name, float value)
        {
            Cs.SetFloat(name, value);
        }
        #endregion

        #region SetVector
        public void SetVector(string name, int2 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, int3 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, int4 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, uint2 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, uint3 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, uint4 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float2 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float3 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, float4 value)
        {
            Cs.SetVector(name, value.ToVector());
        }
        public void SetVector(string name, Vector2 value)
        {
            Cs.SetVector(name, value);
        }
        public void SetVector(string name, Vector3 value)
        {
            Cs.SetVector(name, value);
        }
        public void SetVector(string name, Vector4 value)
        {
            Cs.SetVector(name, value);
        }
        #endregion

        #region SetMatrix
        public void SetMatrix(string name, Matrix4x4 matrix)
        {
            Cs.SetMatrix(name, matrix);
        }
        public void SetMatrix(string name, float4x4 matrix)
        {
            Cs.SetMatrix(name, matrix);
        }
        #endregion

        #region SetKeyword
        public void EnableKeyword(string keyword)
        {
            Cs.EnableKeyword(keyword);
        }
        public void DisableKeyword(string keyword)
        {
            Cs.DisableKeyword(keyword);
        }
        #endregion
    }

    public class GPUKernel
    {
        public ComputeShader Cs { get; }
        public string Name { get; }
        public int ID { get; }
        private uint _threadSizeX;
        private uint _threadSizeY;
        private uint _threadSizeZ;

        public GPUKernel(ComputeShader cs, string name)
        {
            Cs = cs;
            Name = name;
            ID = cs.FindKernel(name);
            cs.GetKernelThreadGroupSizes(ID, out _threadSizeX, out _threadSizeY, out _threadSizeZ);
        }

        #region SetBuffer
        public void SetBuffer(string name, GraphicsBuffer buffer)
        {
            Cs.SetBuffer(ID, name, buffer);
        }
        public void SetBuffer(string name, ComputeBuffer buffer)
        {
            Cs.SetBuffer(ID, name, buffer);
        }
        #endregion

        #region SetTexture
        public void SetTexture(string name, Texture tex)
        {
            Cs.SetTexture(ID, name, tex);
        }
        #endregion

        #region Dispatch
        public void Dispatch(int sizeX, int sizeY = 1, int sizeZ = 1)
        {
            int groupSizeX = (int)Mathf.Ceil(sizeX / (float)_threadSizeX);
            int groupSizeY = (int)Mathf.Ceil(sizeY / (float)_threadSizeY);
            int groupSizeZ = (int)Mathf.Ceil(sizeZ / (float)_threadSizeZ);
            Cs.SetInts("_NumThreads", sizeX, sizeY, sizeZ);
            Cs.SetInts("_NumGroups", groupSizeX, groupSizeY, groupSizeZ);
            Cs.Dispatch(ID, groupSizeX, groupSizeY, groupSizeZ);
        }
        public void Dispatch(int3 size)
        {
            Dispatch(size.x, size.y, size.z);
        }
        #endregion
    }

    public static class GPUCastTool
    {
        public static int[] ToInts(this int2 i2) => new[] { i2.x, i2.y };
        public static int[] ToInts(this int3 i3) => new[] { i3.x, i3.y, i3.z };
        public static int[] ToInts(this int4 i4) => new[] { i4.x, i4.y, i4.z, i4.w };
        public static int[] ToInts(this uint2 i2) => new[] { (int)i2.x, (int)i2.y };
        public static int[] ToInts(this uint3 i3) => new[] { (int)i3.x, (int)i3.y, (int)i3.z };
        public static int[] ToInts(this uint4 i4) => new[] { (int)i4.x, (int)i4.y, (int)i4.z, (int)i4.w };
        public static Vector4 ToVector(this int2 i2) => new(i2.x, i2.y);
        public static Vector4 ToVector(this int3 i3) => new(i3.x, i3.y, i3.z);
        public static Vector4 ToVector(this int4 i4) => new(i4.x, i4.y, i4.z, i4.w);
        public static Vector4 ToVector(this uint2 i2) => new(i2.x, i2.y);
        public static Vector4 ToVector(this uint3 i3) => new(i3.x, i3.y, i3.z);
        public static Vector4 ToVector(this uint4 i4) => new(i4.x, i4.y, i4.z, i4.w);
        public static Vector4 ToVector(this float2 f2) => new(f2.x, f2.y);
        public static Vector4 ToVector(this float3 f3) => new(f3.x, f3.y, f3.z);
        public static Vector4 ToVector(this float4 f4) => new(f4.x, f4.y, f4.z, f4.w);
    }
}