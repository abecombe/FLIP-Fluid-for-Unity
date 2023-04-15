using GPUUtil;
using System;
using RosettaUI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public struct Particle
{
    public float3 Position;
    public float3 Velocity;
}

public enum KernelFunction
{
    Linear,
    Quadratic
}
public enum AdvectionMethod
{
    ForwardEuler,
    SecondOrderRungeKutta,
    ThirdOrderRungeKutta
}

public enum Quality
{
    Low,
    Medium,
    High,
    Ultra
}

public class FLIPSimulation : MonoBehaviour
{
    #region Properties
    private const float DeltaTime = 1f / 60f;

    private const float NumParticleInCell = 8f;
    private int NumParticles => (int)(ParticleInitGridSize.x * ParticleInitGridSize.y * ParticleInitGridSize.z * NumParticleInCell);
    private int NumGrids => GridSize.x * GridSize.y * GridSize.z;

    // Quality
    [SerializeField] private Quality quality = Quality.Medium;
    private readonly float[] _qualityToGridSpacing = { 0.5f, 0.4f, 0.3f, 0.2f };
    private float3 TempGridSpacing => _qualityToGridSpacing[(int)quality];

    // Particle Params
    [SerializeField] private float3 particleInitRangeMin;
    [SerializeField] private float3 particleInitRangeMax;
    private float3 ParticleInitRangeMin => particleInitRangeMin;
    private float3 ParticleInitRangeMax => particleInitRangeMax;
    private float3 ParticleInitGridSize => (ParticleInitRangeMax - ParticleInitRangeMin) / GridSpacing;

    // Grid Params
    private float3 GridMin => -transform.localScale / 2f;
    private float3 GridMax => transform.localScale / 2f;
    private int3 GridSize => (int3)math.ceil((GridMax - GridMin) / TempGridSpacing);
    private float3 GridSpacing => (GridMax - GridMin) / GridSize;
    private float3 GridInvSpacing => 1f / GridSpacing;

    // Particle Data Buffers
    private readonly GPUDoubleBuffer<Particle> _particleBuffer = new();
    private readonly GPUBuffer<float4> _particleRenderingBuffer = new(); // xyz: position, w: speed

    // Grid Data Buffers
    private readonly GPUBuffer<uint2> _gridParticleIDBuffer = new();
    private readonly GPUBuffer<uint> _gridTypeBuffer = new();
    private readonly GPUBuffer<float3> _gridVelocityBuffer = new();
    private readonly GPUBuffer<float3> _gridOriginalVelocityBuffer = new();
    private readonly GPUDoubleBuffer<float3> _gridDiffusionBuffer = new();
    private readonly GPUBuffer<float> _gridDivergenceBuffer = new();
    private readonly GPUDoubleBuffer<float> _gridPressureBuffer = new();
    private readonly GPUBuffer<float> _gridWeightBuffer = new();
    private readonly GPUBuffer<float> _gridGhostWeightBuffer = new();
    private readonly GPUBuffer<uint> _gridUIntWeightBuffer = new();
    private readonly GPUDoubleBuffer<float> _gridDensityPressureBuffer = new();
    private readonly GPUBuffer<float3> _gridPositionModifyBuffer = new();
    private readonly GPUBuffer<float> _gridFloatZeroBuffer = new();

    // Compute Shaders
    private GPUComputeShader _particleInitCS;
    private GPUComputeShader _particleToGridCS;
    private GPUComputeShader _externalForceCS;
    private GPUComputeShader _diffusionCS;
    private GPUComputeShader _pressureProjectionCS;
    private GPUComputeShader _gridToParticleCS;
    private GPUComputeShader _advectionCS;
    private GPUComputeShader _densityProjectionCS;
    private GPUComputeShader _renderingCS;

    // Grid Sort Helper
    private readonly GridSortHelper<Particle> _gridSortHelper = new();

    // Simulation Params
    [SerializeField] [Tooltip("0 is full PIC, 1 is full FLIP")] [Range(0f, 1f)] private float flipness = 0.99f;
    [SerializeField] private Vector3 gravity = Vector3.down * 9.8f;
    [SerializeField] [Range(0f, 10f)] private float viscosity = 0f;
    [SerializeField] [Range(0f, 5f)] private float mouseForce = 1.32f;
    [SerializeField] [Range(0f, 5f)] private float mouseForceRange = 2.25f;
    [SerializeField] private KernelFunction kernelFunction = KernelFunction.Linear;
    [SerializeField] private AdvectionMethod advectionMethod = AdvectionMethod.ForwardEuler;
    [SerializeField] private bool activeDensityProjection = true;
    [SerializeField] [Range(1, 30)] private uint diffusionJacobiIteration = 15;
    [SerializeField] [Range(1, 30)] private uint pressureProjectionJacobiIteration = 15;
    [SerializeField] [Range(1, 60)] private uint densityProjectionJacobiIteration = 30;

    [SerializeField] private bool showFps = true;
    #endregion

    #region Initialize Functions
    private void InitComputeShaders()
    {
        _particleInitCS = new GPUComputeShader(Resources.Load<ComputeShader>("ParticleInitCS"), "InitParticle");
        _particleToGridCS = new GPUComputeShader(Resources.Load<ComputeShader>("ParticleToGridCS"), "ParticleToGrid");
        _externalForceCS = new GPUComputeShader(Resources.Load<ComputeShader>("ExternalForceCS"), "AddExternalForce");
        _diffusionCS = new GPUComputeShader(Resources.Load<ComputeShader>("DiffusionCS"), "Diffuse", "UpdateVelocity");
        _pressureProjectionCS = new GPUComputeShader(Resources.Load<ComputeShader>("PressureProjectionCS"), "CalcDivergence", "Project", "UpdateVelocity");
        _gridToParticleCS = new GPUComputeShader(Resources.Load<ComputeShader>("GridToParticleCS"), "GridToParticle");
        _advectionCS = new GPUComputeShader(Resources.Load<ComputeShader>("AdvectionCS"), "Advect");
        _densityProjectionCS = new GPUComputeShader(Resources.Load<ComputeShader>("DensityProjectionCS"), "BuildGhostWeight", "InitBuffer", "InterlockedAddWeight", "CalcGridWeight", "Project", "CalcPositionModify", "UpdatePosition");
        _renderingCS = new GPUComputeShader(Resources.Load<ComputeShader>("RenderingCS"), "PrepareRendering");
    }

    private void InitParticleBuffers()
    {
        _particleBuffer.Init(NumParticles);
        _particleRenderingBuffer.Init(NumParticles);

        // init particle
        var cs = _particleInitCS;
        var k = cs.Kernel[0];
        SetConstants(cs);
        cs.SetVector("_ParticleInitRangeMin", ParticleInitRangeMin);
        cs.SetVector("_ParticleInitRangeMax", ParticleInitRangeMax);
        k.SetBuffer("_ParticleBufferWrite", _particleBuffer.Read);
        k.Dispatch(NumParticles);

        // init vfx
        var vfx = FindObjectOfType<VisualEffect>();
        vfx.Reinit();
        vfx.SetFloat("NumInstance", NumParticles);
        vfx.SetFloat("Size", GridSpacing.x * 0.8f);
        vfx.SetGraphicsBuffer("ParticleBuffer", _particleRenderingBuffer);
    }

    private void InitGridBuffers()
    {
        _gridParticleIDBuffer.Init(NumGrids);
        _gridTypeBuffer.Init(NumGrids);
        _gridVelocityBuffer.Init(NumGrids);
        _gridOriginalVelocityBuffer.Init(NumGrids);
        _gridDiffusionBuffer.Init(NumGrids);
        _gridDivergenceBuffer.Init(NumGrids);
        _gridPressureBuffer.Init(NumGrids);
        _gridWeightBuffer.Init(NumGrids);
        _gridGhostWeightBuffer.Init(NumGrids);
        _gridUIntWeightBuffer.Init(NumGrids);
        _gridDensityPressureBuffer.Init(NumGrids);
        _gridPositionModifyBuffer.Init(NumGrids);
        _gridFloatZeroBuffer.Init(NumGrids);

        // build ghost weight
        var cs = _densityProjectionCS;
        var k = cs.Kernel[0];
        SetConstants(cs);
        cs.SetVector("_GhostWeight", new float3(0.125f, 0.234375f, 0.330078125f) * NumParticleInCell);
        k.SetBuffer("_GridGhostWeightBufferWrite", _gridGhostWeightBuffer);
        k.Dispatch(NumGrids);
    }

    private void InitGPUBuffers()
    {
        InitParticleBuffers();
        InitGridBuffers();
    }
    #endregion

    #region Update Functions
    private void SetConstants(GPUComputeShader cs)
    {
        cs.SetFloat("_DeltaTime", DeltaTime);

        cs.SetVector("_GridMin", GridMin);
        cs.SetVector("_GridMax", GridMax);
        cs.SetInts("_GridSize", GridSize);
        cs.SetVector("_GridSpacing", GridSpacing);
        cs.SetVector("_GridInvSpacing", GridInvSpacing);

        switch (kernelFunction)
        {
            case KernelFunction.Linear:
                cs.EnableKeyword("USE_LINEAR_KERNEL");
                cs.DisableKeyword("USE_QUADRATIC_KERNEL");
                break;
            case KernelFunction.Quadratic:
                cs.DisableKeyword("USE_LINEAR_KERNEL");
                cs.EnableKeyword("USE_QUADRATIC_KERNEL");
                break;
        }
    }

    // transferring velocity from particle to grid
    private void DispatchParticleToGrid()
    {
        _gridSortHelper.Sort(_particleBuffer, _gridParticleIDBuffer, GridMin, GridMax, GridSize, GridSpacing);

        var cs = _particleToGridCS;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_ParticleBufferRead", _particleBuffer.Read);
        k.SetBuffer("_GridParticleIDBufferRead", _gridParticleIDBuffer);
        k.SetBuffer("_GridTypeBufferWrite", _gridTypeBuffer);
        k.SetBuffer("_GridVelocityBufferWrite", _gridVelocityBuffer);
        k.SetBuffer("_GridOriginalVelocityBufferWrite", _gridOriginalVelocityBuffer);

        k.Dispatch(NumGrids);
    }

    // external force term with reference to https://github.com/dli/fluid
    private float2 _lastMousePlane = float2.zero;
    private void DispatchExternalForce()
    {
        var cs = _externalForceCS;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer);

        cs.SetVector("_Gravity", gravity);

        var cam = Camera.main;
        var mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        cs.SetVector("_RayOrigin", mouseRay.origin);
        cs.SetVector("_RayDirection", mouseRay.direction);

        var height = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f;
        var width = height * Screen.width / Screen.height;
        var mousePlane = ((float3)Input.mousePosition).xy / new float2(Screen.width, Screen.height) - 0.5f;
        mousePlane *= new float2(width, height);
        mousePlane *= cam.GetComponent<OrbitCamera>().Distance;
        var cameraViewMatrix = cam.worldToCameraMatrix;
        var cameraRight = new float3(cameraViewMatrix[0], cameraViewMatrix[4], cameraViewMatrix[8]);
        var cameraUp = new float3(cameraViewMatrix[1], cameraViewMatrix[5], cameraViewMatrix[9]);
        var mouseVelocity = (mousePlane - _lastMousePlane) / DeltaTime;
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Time.frameCount <= 1)
            mouseVelocity = float2.zero;
        _lastMousePlane = mousePlane;
        var mouseAxisVelocity = mouseVelocity.x * cameraRight + mouseVelocity.y * cameraUp;
        cs.SetVector("_MouseForceParameter", new float4(mouseAxisVelocity * mouseForce, mouseForceRange));

        k.Dispatch(NumGrids);
    }

    // viscous diffusion term
    private void DispatchDiffusion()
    {
        if (viscosity <= 0f) return;

        var cs = _diffusionCS;
        var k_diff = cs.Kernel[0];
        var k_vel = cs.Kernel[1];

        SetConstants(cs);

        // diffuse
        float temp1 = viscosity * DeltaTime;
        float3 temp2 = 1f / GridSpacing / GridSpacing;
        float temp3 = 1f / (1f + 2f * (temp2.x + temp2.y + temp2.z) * temp1);
        float4 diffusionParameter = new(temp1 * temp2 * temp3, temp3);
        cs.SetVector("_DiffusionParameter", diffusionParameter);
        k_diff.SetBuffer("_GridTypeBufferRead", _gridTypeBuffer);
        k_diff.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer);
        for (uint i = 0; i < diffusionJacobiIteration; i++)
        {
            k_diff.SetBuffer("_GridDiffusionBufferRead", _gridDiffusionBuffer.Read);
            k_diff.SetBuffer("_GridDiffusionBufferWrite", _gridDiffusionBuffer.Write);
            k_diff.Dispatch(NumGrids);
            _gridDiffusionBuffer.Swap();
        }

        // update velocity
        k_vel.SetBuffer("_GridVelocityBufferWrite", _gridVelocityBuffer);
        k_vel.SetBuffer("_GridDiffusionBufferRead", _gridDiffusionBuffer.Read);
        k_vel.Dispatch(NumGrids);
    }

    // pressure projection term
    private void DispatchPressureProjection()
    {
        var cs = _pressureProjectionCS;
        var k_div = cs.Kernel[0];
        var k_proj = cs.Kernel[1];
        var k_vel = cs.Kernel[2];

        SetConstants(cs);

        // calc divergence
        float3 divergenceParameter = 1f / GridSpacing;
        cs.SetVector("_DivergenceParameter", divergenceParameter);
        k_div.SetBuffer("_GridTypeBufferRead", _gridTypeBuffer);
        k_div.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer);
        k_div.SetBuffer("_GridDivergenceBufferWrite", _gridDivergenceBuffer);
        k_div.Dispatch(NumGrids);

        // project
        float3 temp1 = 1f / GridSpacing / GridSpacing;
        float temp2 = 1f / (2f * (temp1.x + temp1.y + temp1.z));
        float4 projectionParameter1 = new(temp2 / GridSpacing / GridSpacing, -temp2);
        cs.SetVector("_PressureProjectionParameter1", projectionParameter1);
        k_proj.SetBuffer("_GridTypeBufferRead", _gridTypeBuffer);
        k_proj.SetBuffer("_GridDivergenceBufferRead", _gridDivergenceBuffer);
        for (uint i = 0; i < pressureProjectionJacobiIteration; i++)
        {
            k_proj.SetBuffer("_GridPressureBufferRead", _gridPressureBuffer.Read);
            k_proj.SetBuffer("_GridPressureBufferWrite", _gridPressureBuffer.Write);
            k_proj.Dispatch(NumGrids);
            _gridPressureBuffer.Swap();
        }

        // update velocity
        float3 projectionParameter2 = 1f / GridSpacing;
        cs.SetVector("_PressureProjectionParameter2", projectionParameter2);
        k_vel.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer);
        k_vel.SetBuffer("_GridPressureBufferRead", _gridPressureBuffer.Read);
        k_vel.Dispatch(NumGrids);
    }

    // transferring velocity from grid to particle
    private void DispatchGridToParticle()
    {
        var cs = _gridToParticleCS;
        var k = cs.Kernel[0];

        SetConstants(cs);

        cs.SetFloat("_Flipness", math.saturate(flipness));
        k.SetBuffer("_ParticleBufferRW", _particleBuffer.Read);
        k.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer);
        k.SetBuffer("_GridOriginalVelocityBufferRead", _gridOriginalVelocityBuffer);

        k.Dispatch(NumParticles);
    }

    // particle advection term
    private void DispatchAdvection()
    {
        var cs = _advectionCS;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_ParticleBufferRW", _particleBuffer.Read);
        k.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer);
        switch (advectionMethod)
        {
            case AdvectionMethod.ForwardEuler:
                cs.EnableKeyword("USE_RK1");
                cs.DisableKeyword("USE_RK2");
                cs.DisableKeyword("USE_RK3");
                break;
            case AdvectionMethod.SecondOrderRungeKutta:
                cs.DisableKeyword("USE_RK1");
                cs.EnableKeyword("USE_RK2");
                cs.DisableKeyword("USE_RK3");
                break;
            case AdvectionMethod.ThirdOrderRungeKutta:
                cs.DisableKeyword("USE_RK1");
                cs.DisableKeyword("USE_RK2");
                cs.EnableKeyword("USE_RK3");
                break;
        }

        k.Dispatch(NumParticles);
    }

    // density projection term with reference to
    // https://animation.rwth-aachen.de/media/papers/66/2019-TVCG-ImplicitDensityProjection.pdf
    private void DispatchDensityProjection()
    {
        var cs = _densityProjectionCS;
        var k_init = cs.Kernel[1];
        var k_add = cs.Kernel[2];
        var k_weight = cs.Kernel[3];
        var k_proj = cs.Kernel[4];
        var k_delpos = cs.Kernel[5];
        var k_update = cs.Kernel[6];

        SetConstants(cs);
        cs.SetFloat("_InvAverageWeight", 1f / NumParticleInCell);

        // init buffers
        k_init.SetBuffer("_GridTypeBufferWrite", _gridTypeBuffer);
        k_init.SetBuffer("_GridUIntWeightBufferWrite", _gridUIntWeightBuffer);
        k_init.Dispatch(NumGrids);

        // interlocked add weight
        k_add.SetBuffer("_ParticleBufferRead", _particleBuffer.Read);
        k_add.SetBuffer("_GridTypeBufferWrite", _gridTypeBuffer);
        k_add.SetBuffer("_GridUIntWeightBufferWrite", _gridUIntWeightBuffer);
        k_add.Dispatch(NumParticles);

        // calc grid weight
        k_weight.SetBuffer("_GridTypeBufferRead", _gridTypeBuffer);
        k_weight.SetBuffer("_GridUIntWeightBufferRead", _gridUIntWeightBuffer);
        k_weight.SetBuffer("_GridGhostWeightBufferRead", _gridGhostWeightBuffer);
        k_weight.SetBuffer("_GridWeightBufferWrite", _gridWeightBuffer);
        k_weight.Dispatch(NumGrids);

        // project
        float3 temp1 = 1f / GridSpacing / GridSpacing;
        float temp2 = 1f / (2f * (temp1.x + temp1.y + temp1.z));
        float4 projectionParameter1 = new(temp2 / GridSpacing / GridSpacing, -temp2);
        cs.SetVector("_DensityProjectionParameter1", projectionParameter1);
        k_proj.SetBuffer("_GridTypeBufferRead", _gridTypeBuffer);
        k_proj.SetBuffer("_GridWeightBufferRead", _gridWeightBuffer);
        for (uint i = 0; i < densityProjectionJacobiIteration; i++)
        {
            k_proj.SetBuffer("_GridDensityPressureBufferRead", i == 0 ? _gridFloatZeroBuffer : _gridDensityPressureBuffer.Read);
            k_proj.SetBuffer("_GridDensityPressureBufferWrite", _gridDensityPressureBuffer.Write);
            k_proj.Dispatch(NumGrids);
            _gridDensityPressureBuffer.Swap();
        }

        // calc grid delta position
        float3 projectionParameter2 = 1f / GridSpacing;
        cs.SetVector("_DensityProjectionParameter2", projectionParameter2);
        k_delpos.SetBuffer("_GridDensityPressureBufferRead", _gridDensityPressureBuffer.Read);
        k_delpos.SetBuffer("_GridPositionModifyBufferWrite", _gridPositionModifyBuffer);
        k_delpos.Dispatch(NumGrids);

        // update particle position
        k_update.SetBuffer("_ParticleBufferRW", _particleBuffer.Read);
        k_update.SetBuffer("_GridPositionModifyBufferRead", _gridPositionModifyBuffer);
        k_update.Dispatch(NumParticles);
    }

    private void RenderParticles()
    {
        var cs = _renderingCS;
        var k = cs.Kernel[0];

        k.SetBuffer("_ParticleBufferRead", _particleBuffer.Read);
        k.SetBuffer("_ParticleRenderingBufferWrite", _particleRenderingBuffer);

        k.Dispatch(NumParticles);
    }
    #endregion

    #region Release Buffers
    private void ReleaseBuffers()
    {
        _particleBuffer.Release();
        _particleRenderingBuffer.Release();

        _gridParticleIDBuffer.Release();
        _gridTypeBuffer.Release();
        _gridVelocityBuffer.Release();
        _gridOriginalVelocityBuffer.Release();
        _gridDiffusionBuffer.Release();
        _gridDivergenceBuffer.Release();
        _gridPressureBuffer.Release();
        _gridWeightBuffer.Release();
        _gridGhostWeightBuffer.Release();
        _gridUIntWeightBuffer.Release();
        _gridDensityPressureBuffer.Release();
        _gridPositionModifyBuffer.Release();
        _gridFloatZeroBuffer.Release();

        _gridSortHelper.ReleaseBuffers();
    }
    #endregion

    #region RosettaUI
    public Element CreateElement()
    {
        return UI.Window(
            "Settings ( press U to open / close )",
            UI.Indent(UI.Box(UI.Indent(
            UI.Space().SetHeight(5f),
            UI.Field("Quality", () => quality)
            .RegisterValueChangeCallback(InitGPUBuffers),
            UI.Indent(
                UI.FieldReadOnly("Num Particles", () => NumParticles),
                UI.FieldReadOnly("Num Grids", () => NumGrids)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Parameter"),
            UI.Indent(
                UI.Slider("Flipness", () => flipness),
                UI.Field("Gravity", () => gravity),
                UI.Slider("Viscosity", () => viscosity)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Numerical Method"),
            UI.Indent(
                UI.Field("Kernel Function", () => kernelFunction),
                UI.Field("Advection Method", () => advectionMethod),
                UI.Field("Density Projection", () => activeDensityProjection)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Interaction"),
            UI.Indent(
                UI.Slider("Mouse Force", () => mouseForce),
                UI.Slider("Mouse Force Range", () => mouseForceRange)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Jacobi Iteration"),
            UI.Indent(
                UI.Slider("Diffusion", () => diffusionJacobiIteration),
                UI.Slider("Pressure Projection", () => pressureProjectionJacobiIteration),
                UI.Slider("Density Projection", () => densityProjectionJacobiIteration)
            ),
            UI.Space().SetHeight(10f),
            UI.Field("Show FPS", () => showFps)
            .RegisterValueChangeCallback(() => {
                FindObjectOfType<FPSCounter>().enabled = showFps;
            }),
            UI.Space().SetHeight(5f)
            )))
        );
    }
    #endregion

    #region MonoBehaviour
    private void OnEnable()
    {
        InitComputeShaders();
        InitGPUBuffers();
    }

    private void Update()
    {
        DispatchParticleToGrid();
        DispatchExternalForce();
        DispatchDiffusion();
        DispatchPressureProjection();
        DispatchGridToParticle();
        DispatchAdvection();
        if (activeDensityProjection) DispatchDensityProjection();
        RenderParticles();
    }

    private void OnDisable()
    {
        ReleaseBuffers();
    }
    #endregion
}
