#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SaveMeshAsAnAsset : MonoBehaviour
{
    [SerializeField] private string _saveAsAnAssetInPath = "";
    [SerializeField] private Mesh _mesh;

    [ContextMenu("Save Mesh As An Asset")]
    public void SaveMesh()
    {
        if (_saveAsAnAssetInPath == "") return;
        Mesh mesh = Instantiate(_mesh);
        AssetDatabase.CreateAsset(mesh, _saveAsAnAssetInPath);
        AssetDatabase.SaveAssets();
    }
}

[CustomEditor(typeof(SaveMeshAsAnAsset))]
public class SaveMeshAsAnAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(5f);
        if (GUILayout.Button("Save Mesh As An Asset"))
        {
            var saveMeshAsAnAsset = target as SaveMeshAsAnAsset;
            saveMeshAsAnAsset.SaveMesh();
        }
    }
}
#endif