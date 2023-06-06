using GPUUtil;
using System;
using RosettaUI;
using Unity.Mathematics;
using UnityEngine;

public class SmokeSimulation : MonoBehaviour, IDisposable
{
    #region Structs & Enums
    private enum Quality
    {
        Low,
        Medium,
        High,
        Ultra
    }
    #endregion
    
    #region Properties
    private const float DeltaTime = 1f / 60f;
    
    private int NumGrids => GridSize.x * GridSize.y * GridSize.z;

    // Quality
    [SerializeField] private Quality _quality = Quality.Medium;
    private readonly float[] _qualityToGridSpacing = { 0.4f, 0.3f, 0.2f, 0.1f };
    private float3 TempGridSpacing => _qualityToGridSpacing[(int)_quality];

    // Grid Params
    private float3 GridMin => -transform.localScale / 2f;
    private float3 GridMax => transform.localScale / 2f;
    private int3 GridSize => (int3)math.ceil((GridMax - GridMin) / TempGridSpacing);
    private float3 GridSpacing => (GridMax - GridMin) / GridSize;
    private float3 GridInvSpacing => 1f / GridSpacing;

    // Grid Data Buffers
    private readonly GPUDoubleBuffer<float3> _gridVelocityBuffer = new();
    private readonly GPUDoubleBuffer<float> _gridDensityBuffer = new();
    private readonly GPUDoubleBuffer<float> _gridTemperatureBuffer = new();
    private readonly GPUBuffer<float4> _gridVorticityBuffer = new();
    private readonly GPUDoubleBuffer<float3> _gridDiffusionBuffer = new();
    private readonly GPUBuffer<float> _gridDivergenceBuffer = new();
    private readonly GPUDoubleBuffer<float> _gridPressureBuffer = new();
    private readonly GPUTexture3D _gridDensityTexture = new();

    // Compute Shaders
    private GPUComputeShader _emitterCs;
    private GPUComputeShader _gridAdvectionCs;
    private GPUComputeShader _externalForceCs;
    private GPUComputeShader _buoyancyCs;
    private GPUComputeShader _vorticityCs;
    private GPUComputeShader _diffusionCs;
    private GPUComputeShader _boundaryCs;
    private GPUComputeShader _pressureProjectionCs;
    private GPUComputeShader _renderingCs;

    [SerializeField] private Material _volumeRendering;

    // Simulation Params
    [SerializeField] [Range(0f, 5f)] private float _mouseForce = 2f;
    [SerializeField] [Range(0f, 5f)] private float _mouseForceRange = 2f;
    [SerializeField] [Range(0f, 2f)] private float _densityDissipation = 1f;
    [SerializeField] [Range(0f, 2f)] private float _temperatureDissipation = 1f;
    [SerializeField] private float _buoyancyAlpha = 0.08f;
    [SerializeField] private float _buoyancyBeta = 0.97f;
    [SerializeField] private float _vorticityEpsilon = 0.1f;
    [SerializeField] [Range(0f, 10f)] private float _viscosity = 0f;
    [SerializeField] [Range(1, 30)] private uint _diffusionJacobiIteration = 15;
    [SerializeField] [Range(1, 30)] private uint _pressureProjectionJacobiIteration = 15;
    [SerializeField] private bool3 _boundaryPositive = true;
    [SerializeField] private bool3 _boundaryNegative = true;

    // Rendering Params
    [SerializeField] private Color _color = Color.red;
    [SerializeField] private float2 _densityVisibleRange = new(0f, 1f);
    [SerializeField] [Range(0.001f, 0.1f)] private float _samplingDistance = 0.01f;

    [SerializeField] private bool _showFps = true;
    #endregion

    #region Initialize Functions
    private void InitComputeShaders()
    {
        _emitterCs = new GPUComputeShader(Resources.Load<ComputeShader>("EmitterCS"), "Emit");
        _gridAdvectionCs = new GPUComputeShader(Resources.Load<ComputeShader>("GridAdvectionCS"), "AdvectVelocity", "AdvectScalar");
        _externalForceCs = new GPUComputeShader(Resources.Load<ComputeShader>("ExternalForceCS"), "AddExternalForce");
        _buoyancyCs = new GPUComputeShader(Resources.Load<ComputeShader>("BuoyancyCS"), "AddBuoyancy");
        _vorticityCs = new GPUComputeShader(Resources.Load<ComputeShader>("VorticityCS"), "CalcVorticity", "ConfineVorticity");
        _diffusionCs = new GPUComputeShader(Resources.Load<ComputeShader>("DiffusionCS"), "Diffuse", "UpdateVelocity");
        _boundaryCs = new GPUComputeShader(Resources.Load<ComputeShader>("BoundaryCS"), "EnforceBoundaryCondition");
        _pressureProjectionCs = new GPUComputeShader(Resources.Load<ComputeShader>("PressureProjectionCS"), "CalcDivergence", "Project", "UpdateVelocity");
        _renderingCs = new GPUComputeShader(Resources.Load<ComputeShader>("RenderingCS"), "BakeTexture");
    }

    private void InitGridBuffers()
    {
        _gridVelocityBuffer.Init(NumGrids);
        _gridDensityBuffer.Init(NumGrids);
        _gridTemperatureBuffer.Init(NumGrids);
        _gridVorticityBuffer.Init(NumGrids);
        _gridDiffusionBuffer.Init(NumGrids);
        _gridDivergenceBuffer.Init(NumGrids);
        _gridPressureBuffer.Init(NumGrids);
        _gridDensityTexture.Init(GridSize, RenderTextureFormat.RFloat);
    }

    private void InitGPUBuffers()
    {
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

        cs.SetInts("_BoundaryPositive", (int3)_boundaryPositive);
        cs.SetInts("_BoundaryNegative", (int3)_boundaryNegative);

        cs.DisableKeyword("FREE_SURFACE");
        cs.EnableKeyword("NO_SURFACE");
    }
    
    private void DispatchEmitter()
    {
        var cs = _emitterCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);
        k.SetBuffer("_GridDensityBufferRW", _gridDensityBuffer.Read);
        k.SetBuffer("_GridTemperatureBufferRW", _gridTemperatureBuffer.Read);
        k.Dispatch(NumGrids);
    }

    private void DispatchVelocityAdvection()
    {
        var cs = _gridAdvectionCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer.Read);
        k.SetBuffer("_GridVelocityBufferWrite", _gridVelocityBuffer.Write);
        k.Dispatch(NumGrids);
        _gridVelocityBuffer.Swap();
    }

    // external force term with reference to https://github.com/dli/fluid
    private float2 _lastMousePlane = float2.zero;
    private void DispatchExternalForce()
    {
        var cs = _externalForceCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);

        cs.SetVector("_Gravity", float3.zero);

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
        cs.SetVector("_MouseForceParameter", new float4(mouseAxisVelocity * _mouseForce, _mouseForceRange));

        k.Dispatch(NumGrids);
    }

    private void DispatchBuoyancy()
    {
        var cs = _buoyancyCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        cs.SetVector("_BuoyancyParameter", new float3(_buoyancyAlpha, _buoyancyBeta, 0f));
        k.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);
        k.SetBuffer("_GridDensityBufferRead", _gridDensityBuffer.Read);
        k.SetBuffer("_GridTemperatureBufferRead", _gridTemperatureBuffer.Read);
        k.Dispatch(NumGrids);
    }

    private void DispatchVorticity()
    {
        var cs = _vorticityCs;
        var k_vort = cs.Kernel[0];
        var k_conf = cs.Kernel[1];

        SetConstants(cs);

        k_vort.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer.Read);
        k_vort.SetBuffer("_GridVorticityBufferWrite", _gridVorticityBuffer);
        k_vort.Dispatch(NumGrids);

        cs.SetFloat("_VorticityEpsilon", _vorticityEpsilon);
        k_conf.SetBuffer("_GridVorticityBufferRead", _gridVorticityBuffer);
        k_conf.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);
        k_conf.Dispatch(NumGrids);
    }

    private void DispatchDiffusion()
    {
        if (_viscosity <= 0f) return;

        var cs = _diffusionCs;
        var k_diff = cs.Kernel[0];
        var k_vel = cs.Kernel[1];

        SetConstants(cs);

        // diffuse
        float temp1 = _viscosity * DeltaTime;
        float3 temp2 = 1f / GridSpacing / GridSpacing;
        float temp3 = 1f / (1f + 2f * (temp2.x + temp2.y + temp2.z) * temp1);
        float4 diffusionParameter = new(temp1 * temp2 * temp3, temp3);
        cs.SetVector("_DiffusionParameter", diffusionParameter);
        k_diff.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer.Read);
        for (uint i = 0; i < _diffusionJacobiIteration; i++)
        {
            k_diff.SetBuffer("_GridDiffusionBufferRead", _gridDiffusionBuffer.Read);
            k_diff.SetBuffer("_GridDiffusionBufferWrite", _gridDiffusionBuffer.Write);
            k_diff.Dispatch(NumGrids);
            _gridDiffusionBuffer.Swap();
        }

        // update velocity
        k_vel.SetBuffer("_GridVelocityBufferWrite", _gridVelocityBuffer.Read);
        k_vel.SetBuffer("_GridDiffusionBufferRead", _gridDiffusionBuffer.Read);
        k_vel.Dispatch(NumGrids);
    }

    // enforce boundary velocity conditions
    private void DispatchBoundaryCondition()
    {
        var cs = _boundaryCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        k.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);

        k.Dispatch(NumGrids);
    }

    // pressure projection term
    private void DispatchPressureProjection()
    {
        var cs = _pressureProjectionCs;
        var k_div = cs.Kernel[0];
        var k_proj = cs.Kernel[1];
        var k_vel = cs.Kernel[2];

        SetConstants(cs);

        // calc divergence
        float3 divergenceParameter = 1f / (2f * GridSpacing);
        cs.SetVector("_DivergenceParameter", divergenceParameter);
        k_div.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer.Read);
        k_div.SetBuffer("_GridDivergenceBufferWrite", _gridDivergenceBuffer);
        k_div.Dispatch(NumGrids);

        // project
        float3 temp1 = 1f / GridSpacing / GridSpacing;
        float temp2 = 1f / (2f * (temp1.x + temp1.y + temp1.z));
        float4 projectionParameter1 = new(temp2 / GridSpacing / GridSpacing, -temp2);
        cs.SetVector("_PressureProjectionParameter1", projectionParameter1);
        k_proj.SetBuffer("_GridDivergenceBufferRead", _gridDivergenceBuffer);
        for (uint i = 0; i < _pressureProjectionJacobiIteration; i++)
        {
            k_proj.SetBuffer("_GridPressureBufferRead", _gridPressureBuffer.Read);
            k_proj.SetBuffer("_GridPressureBufferWrite", _gridPressureBuffer.Write);
            k_proj.Dispatch(NumGrids);
            _gridPressureBuffer.Swap();
        }

        // update velocity
        float3 projectionParameter2 = 1f / (2f * GridSpacing);
        cs.SetVector("_PressureProjectionParameter2", projectionParameter2);
        k_vel.SetBuffer("_GridVelocityBufferRW", _gridVelocityBuffer.Read);
        k_vel.SetBuffer("_GridPressureBufferRead", _gridPressureBuffer.Read);
        k_vel.Dispatch(NumGrids);
    }

    private void DispatchScalarAdvection()
    {
        var cs = _gridAdvectionCs;
        var k = cs.Kernel[1];

        SetConstants(cs);

        cs.SetVector("_ScalarFieldDecay", 1f - new float2(_densityDissipation, _temperatureDissipation) * DeltaTime);
        k.SetBuffer("_GridVelocityBufferRead", _gridVelocityBuffer.Read);
        k.SetBuffer("_GridDensityBufferRead", _gridDensityBuffer.Read);
        k.SetBuffer("_GridDensityBufferWrite", _gridDensityBuffer.Write);
        k.SetBuffer("_GridTemperatureBufferRead", _gridTemperatureBuffer.Read);
        k.SetBuffer("_GridTemperatureBufferWrite", _gridTemperatureBuffer.Write);
        k.Dispatch(NumGrids);
        _gridDensityBuffer.Swap();
        _gridTemperatureBuffer.Swap();
    }

    private void DispatchRendering()
    {
        var cs = _renderingCs;
        var k = cs.Kernel[0];

        SetConstants(cs);

        cs.SetVector("_VisibleRange", _densityVisibleRange);
        k.SetBuffer("_GridBufferRead", _gridDensityBuffer.Read);
        k.SetTexture("_GridTextureWrite", _gridDensityTexture);
        k.Dispatch(NumGrids);

        _volumeRendering.SetColor("_Color", _color);
        _volumeRendering.SetTexture("_VolumeTexture", _gridDensityTexture);
        _volumeRendering.SetFloat("_SamplingDistance", _samplingDistance);
        _volumeRendering.SetInt("_Iteration", (int)math.ceil(math.sqrt(3f) / _samplingDistance));
    }
    #endregion

    #region Release Buffers
    public void Dispose()
    {
        _gridVelocityBuffer.Dispose();
        _gridDensityBuffer.Dispose();
        _gridTemperatureBuffer.Dispose();
        _gridVorticityBuffer.Dispose();
        _gridDiffusionBuffer.Dispose();
        _gridDivergenceBuffer.Dispose();
        _gridPressureBuffer.Dispose();
        _gridDensityTexture.Dispose();
    }
    #endregion

    #region RosettaUI
    public Element CreateElement()
    {
        return UI.Window(
            "Settings ( press U to open / close )",
            UI.Indent(UI.Box(UI.Indent(
            UI.Space().SetHeight(5f),
            UI.Field("Quality", () => _quality)
            .RegisterValueChangeCallback(InitGPUBuffers),
            UI.Indent(
                UI.FieldReadOnly("Num Grids", () => NumGrids)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Parameter"),
            UI.Indent(
                UI.Slider("Density Drag", () => _densityDissipation),
                UI.Slider("Temperature Drag", () => _temperatureDissipation),
                UI.Field("Buoyancy Alpha", () => _buoyancyAlpha),
                UI.Field("Buoyancy Beta", () => _buoyancyBeta),
                UI.Field("Vorticity Epsilon", () => _vorticityEpsilon),
                UI.Field("Viscosity", () => _viscosity)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Numerical Method"),
            UI.Indent(
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Boundary"),
            UI.Indent(
                UI.Field("Positive", () => _boundaryPositive),
                UI.Field("Negative", () => _boundaryNegative)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Interaction"),
            UI.Indent(
                UI.Slider("Mouse Force", () => _mouseForce),
                UI.Slider("Mouse Force Range", () => _mouseForceRange)
            ),
            UI.Space().SetHeight(10f),
            UI.Label("Jacobi Iteration"),
            UI.Indent(
                UI.Slider("Diffusion", () => _diffusionJacobiIteration),
                UI.Slider("Pressure Projection", () => _pressureProjectionJacobiIteration)
            ),
            UI.Label("Volume Rendering"),
            UI.Indent(
                UI.Field("Color", () => _color),
                UI.Field("Density Visible Range", () => _densityVisibleRange),
                UI.Slider("Sampling Distance", () => _samplingDistance)
            ),
            UI.Space().SetHeight(10f),
            UI.Field("Show FPS", () => _showFps)
            .RegisterValueChangeCallback(() => {
                FindObjectOfType<FPSCounter>().enabled = _showFps;
            }),
            UI.Space().SetHeight(10f),
            UI.Button("Restart", InitGPUBuffers),
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
        DispatchEmitter();
        DispatchVelocityAdvection();
        DispatchExternalForce();
        DispatchBuoyancy();
        DispatchVorticity();
        DispatchDiffusion();
        DispatchBoundaryCondition();
        DispatchPressureProjection();
        DispatchScalarAdvection();
        DispatchRendering();
    }

    private void OnDisable()
    {
        Dispose();
    }
    #endregion
}
