using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EditMeshVertex))]
public class EditMeshVertexEditor : Editor{

    private EditMeshVertex _EditMeshVertex;

    public void OnEnable()
    {
        _EditMeshVertex = (EditMeshVertex)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("编辑完成"))
        {
            EditorApplication.delayCall += Finish;
        }
    }
    void Finish()
    {
        DestroyImmediate(_EditMeshVertex);
        _EditMeshVertex = null;
    }
}
