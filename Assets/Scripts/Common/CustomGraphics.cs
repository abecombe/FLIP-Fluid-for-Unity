using UnityEngine;
using UnityEngine.Rendering;

public static class CustomGraphics
{
    public static void DrawMeshInstancedIndirect(Mesh mesh, Material material, MaterialPropertyBlock mpb, GraphicsBuffer bufferWithArgs, int layer)
    {
        Graphics.DrawMeshInstancedIndirect(
            mesh,
            0,
            material,
            new Bounds(Vector3.zero, Vector3.one * 1000f),
            bufferWithArgs,
            0,
            mpb,
            ShadowCastingMode.Off,
            false,
            layer,
            null,
            LightProbeUsage.Off
        );
    }

    public static void DrawProceduralIndirect(Material material, MaterialPropertyBlock mpb, GraphicsBuffer bufferWithArgs, int layer)
    {
        Graphics.DrawProceduralIndirect(
            material,
            new Bounds(Vector3.zero, Vector3.one * 1000f),
            MeshTopology.Points,
            bufferWithArgs,
            0,
            null,
            mpb,
            ShadowCastingMode.Off,
            false,
            layer
        );
    }
}