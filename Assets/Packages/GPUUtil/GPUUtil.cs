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
        public GraphicsBuffer Data => _buffer;
        public int Size => _buffer.count;
        public int Stride => _buffer.stride;
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

    public class GPUDoubleBuffer<T> : IDisposable
    {
        public GPUBuffer<T> Read => _bufferRead;
        public GPUBuffer<T> Write => _bufferWrite;
        public int Size => _bufferRead.Size;
        public int Stride => _bufferRead.Stride;
        public int Bytes => _bufferRead.Bytes;

        private GPUBuffer<T> _bufferRead = new();
        private GPUBuffer<T> _bufferWrite = new();
        private GPUComputeShader _copyCs;
        private bool _inited = false;

        public void Init(int size)
        {
            Dispose();
            _bufferRead.Init(size);
            _bufferWrite.Init(size);
            _copyCs = new GPUComputeShader("GPUUtil");
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

        public void CopyReadToWrite()
        {
            var cs = _copyCs;
            var kernel = cs.FindKernel("CopyBuffer");
            cs.SetInt("_BufferSize", Size);
            cs.SetInt("_BufferBytes", Bytes);
            kernel.SetBuffer("_BufferRead", _bufferRead);
            kernel.SetBuffer("_BufferWrite", _bufferWrite);
            kernel.Dispatch(Size);
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

    public class GPUTexture2D : IDisposable
    {
        public RenderTexture Data => _tex;
        public int Width => _tex.width;
        public int Height => _tex.height;
        public RenderTextureFormat Format => _tex.format;
        public FilterMode FilterMode => _tex.filterMode;
        public TextureWrapMode WrapMode => _tex.wrapMode;

        private RenderTexture _tex;
        private bool _inited = false;

        public void Init(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            _tex = new RenderTexture(width, height, 0, format)
            {
                filterMode = filterMode,
                wrapMode = wrapMode,
                enableRandomWrite = true
            };
            _tex.Create();
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _tex.Release();
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
        public GPUTexture2D Read => _texRead;
        public GPUTexture2D Write => _texWrite;
        public int Width => _texRead.Width;
        public int Height => _texRead.Height;
        public RenderTextureFormat Format => _texRead.Format;
        public FilterMode FilterMode => _texRead.FilterMode;
        public TextureWrapMode WrapMode => _texRead.WrapMode;

        private GPUTexture2D _texRead = new();
        private GPUTexture2D _texWrite = new();
        private bool _inited = false;

        public void Init(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            _texRead.Init(width, height, format, filterMode, wrapMode);
            _texWrite.Init(width, height, format, filterMode, wrapMode);
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _texRead.Dispose();
            if (_inited) _texWrite.Dispose();
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

        public void Swap()
        {
            (_texRead, _texWrite) = (_texWrite, _texRead);
        }
    }

    public class GPUTexture3D : IDisposable
    {
        public RenderTexture Data => _tex;
        public int Width => _tex.width;
        public int Height => _tex.height;
        public int Depth => _tex.volumeDepth;
        public RenderTextureFormat Format => _tex.format;
        public FilterMode FilterMode => _tex.filterMode;
        public TextureWrapMode WrapMode => _tex.wrapMode;

        private RenderTexture _tex;
        private bool _inited = false;

        public void Init(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            _tex = new RenderTexture(width, height, 0, format)
            {
                filterMode = filterMode,
                wrapMode = wrapMode,
                dimension = TextureDimension.Tex3D,
                volumeDepth = depth,
                enableRandomWrite = true
            };
            _tex.Create();
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _tex.Release();
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
        public GPUTexture3D Read => _texRead;
        public GPUTexture3D Write => _texWrite;
        public int Width => _texRead.Width;
        public int Height => _texRead.Height;
        public int Depth => _texRead.Depth;
        public RenderTextureFormat Format => _texRead.Format;
        public FilterMode FilterMode => _texRead.FilterMode;
        public TextureWrapMode WrapMode => _texRead.WrapMode;

        private GPUTexture3D _texRead = new();
        private GPUTexture3D _texWrite = new();
        private bool _inited = false;

        public void Init(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, FilterMode filterMode = FilterMode.Bilinear, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Dispose();
            _texRead.Init(width, height, depth, format, filterMode, wrapMode);
            _texWrite.Init(width, height, depth, format, filterMode, wrapMode);
            _inited = true;
        }

        public void Dispose()
        {
            if (_inited) _texRead.Dispose();
            if (_inited) _texWrite.Dispose();
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

        public void Swap()
        {
            (_texRead, _texWrite) = (_texWrite, _texRead);
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