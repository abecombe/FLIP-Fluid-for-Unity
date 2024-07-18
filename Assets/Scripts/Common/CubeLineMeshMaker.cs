#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CubeLineMeshMaker : MonoBehaviour
{
    [SerializeField] private string _saveAsAnAssetInPath = "";
    [SerializeField] private int _division = 1;

    [ContextMenu("Line Mesh Converter")]
    public void ConvertMesh()
    {
        if (_saveAsAnAssetInPath == "") return;

        if (_division  <= 0) return;

        var linesMesh = new Mesh();

        var vertices = new Vector3[(_division + 1) * 2 * 12];
        var indices = new int[vertices.Length];
        var index = 0;
        for (int i = 0; i < 12; i++)
        {
            Vector3 p = Vector3.zero;
            Vector3 q = Vector3.zero;
            Vector3 r = Vector3.zero;
            Vector3 s = Vector3.zero;
            switch (i)
            {
                case 0:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(-0.5f, -0.5f, 0.5f);
                    r = new Vector3(-0.5f, 0.5f, -0.5f);
                    s = new Vector3(-0.5f, 0.5f, 0.5f);
                    break;
                case 1:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(-0.5f, 0.5f, -0.5f);
                    r = new Vector3(-0.5f, -0.5f, 0.5f);
                    s = new Vector3(-0.5f, 0.5f, 0.5f);
                    break;
                case 2:
                    p = new Vector3(0.5f, -0.5f, -0.5f);
                    q = new Vector3(0.5f, -0.5f, 0.5f);
                    r = new Vector3(0.5f, 0.5f, -0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case 3:
                    p = new Vector3(0.5f, -0.5f, -0.5f);
                    q = new Vector3(0.5f, 0.5f, -0.5f);
                    r = new Vector3(0.5f, -0.5f, 0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case 4:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(0.5f, -0.5f, -0.5f);
                    r = new Vector3(-0.5f, -0.5f, 0.5f);
                    s = new Vector3(0.5f, -0.5f, 0.5f);
                    break;
                case 5:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(-0.5f, -0.5f, 0.5f);
                    r = new Vector3(0.5f, -0.5f, -0.5f);
                    s = new Vector3(0.5f, -0.5f, 0.5f);
                    break;
                case 6:
                    p = new Vector3(-0.5f, 0.5f, -0.5f);
                    q = new Vector3(0.5f, 0.5f, -0.5f);
                    r = new Vector3(-0.5f, 0.5f, 0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case 7:
                    p = new Vector3(-0.5f, 0.5f, -0.5f);
                    q = new Vector3(-0.5f, 0.5f, 0.5f);
                    r = new Vector3(0.5f, 0.5f, -0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case 8:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(-0.5f, 0.5f, -0.5f);
                    r = new Vector3(0.5f, -0.5f, -0.5f);
                    s = new Vector3(0.5f, 0.5f, -0.5f);
                    break;
                case 9:
                    p = new Vector3(-0.5f, -0.5f, -0.5f);
                    q = new Vector3(0.5f, -0.5f, -0.5f);
                    r = new Vector3(-0.5f, 0.5f, -0.5f);
                    s = new Vector3(0.5f, 0.5f, -0.5f);
                    break;
                case 10:
                    p = new Vector3(-0.5f, -0.5f, 0.5f);
                    q = new Vector3(-0.5f, 0.5f, 0.5f);
                    r = new Vector3(0.5f, -0.5f, 0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case 11:
                    p = new Vector3(-0.5f, -0.5f, 0.5f);
                    q = new Vector3(0.5f, -0.5f, 0.5f);
                    r = new Vector3(-0.5f, 0.5f, 0.5f);
                    s = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
            }
            var faceVertices = new Vector3[(_division + 1) * 2];
            var faceIndices = new int[faceVertices.Length];
            for (int j = 0; j < _division + 1; j++)
            {
                float t = (float)j / _division;
                faceVertices[j * 2] = new Vector3(
                    Mathf.Lerp(p.x, q.x, t),
                    Mathf.Lerp(p.y, q.y, t),
                    Mathf.Lerp(p.z, q.z, t)
                );
                faceVertices[j * 2 + 1] = new Vector3(
                    Mathf.Lerp(r.x, s.x, t),
                    Mathf.Lerp(r.y, s.y, t),
                    Mathf.Lerp(r.z, s.z, t)
                );
                faceIndices[j * 2] = j * 2;
                faceIndices[j * 2 + 1] = j * 2 + 1;
            }

            for (int k = 0; k < faceVertices.Length; k++)
            {
                vertices[index] = faceVertices[k];
                indices[index] = index;
                index++;
            }
        }

        linesMesh.vertices = vertices;
        linesMesh.SetIndices(indices, MeshTopology.Lines, 0);

        AssetDatabase.CreateAsset(linesMesh, _saveAsAnAssetInPath);
        AssetDatabase.SaveAssets();
    }
}

[CustomEditor(typeof(CubeLineMeshMaker))]
public class CubeLineMeshMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(5f);
        if (GUILayout.Button("Convert To Line Mesh"))
        {
            var cubeLineMeshMaker = target as CubeLineMeshMaker;
            cubeLineMeshMaker.ConvertMesh();
        }
    }
}
#endif