#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LineMeshConverter : MonoBehaviour
{
    [SerializeField] private string _saveAsAnAssetInPath = "";
    [SerializeField] private Mesh _originalMesh;

    [ContextMenu("Line Mesh Converter")]
    public void ConvertMesh()
    {
        if (_saveAsAnAssetInPath == "") return;

        if (_originalMesh == null) return;

        var linesMesh = new Mesh
        {
            vertices = _originalMesh.vertices
        };

        var indices = new int[_originalMesh.triangles.Length * 2];
        for (int i = 0; i < _originalMesh.triangles.Length / 3; i++)
        {
            indices[i * 6] = _originalMesh.triangles[i * 3];
            indices[i * 6 + 1] = _originalMesh.triangles[i * 3 + 1];
            indices[i * 6 + 2] = _originalMesh.triangles[i * 3 + 1];
            indices[i * 6 + 3] = _originalMesh.triangles[i * 3 + 2];
            indices[i * 6 + 4] = _originalMesh.triangles[i * 3 + 2];
            indices[i * 6 + 5] = _originalMesh.triangles[i * 3];
        }

        linesMesh.SetIndices(indices, MeshTopology.Lines, 0);

        AssetDatabase.CreateAsset(linesMesh, _saveAsAnAssetInPath);
        AssetDatabase.SaveAssets();
    }
}

[CustomEditor(typeof(LineMeshConverter))]
public class LineMeshConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(5f);
        if (GUILayout.Button("Convert To Line Mesh"))
        {
            var lineMeshConverter = target as LineMeshConverter;
            lineMeshConverter.ConvertMesh();
        }
    }
}
#endif