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

    public static void DrawProcedural(Material material, MaterialPropertyBlock mpb, int count, int layer)
    {
        Graphics.DrawProcedural(
            material,
            new Bounds(Vector3.zero, Vector3.one * 1000f),
            MeshTopology.Points,
            count,
            1,
            null,
            mpb,
            ShadowCastingMode.Off,
            false,
            layer
        );
    }
}