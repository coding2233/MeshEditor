using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EditMeshVertexSenior))]
public class EditMeshVertexSeniorEditor : Editor{

    private EditMeshVertexSenior _EditMeshVertexSenior;
    private EditMeshVertexAuxiliary _EditMeshVertexAuxiliary;

    public void OnEnable()
    {
        _EditMeshVertexSenior = (EditMeshVertexSenior)target;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("打开编辑辅助界面", "LargeButton"))
        {
            EditorApplication.delayCall += OpenAuxiliaryWindow;
        }
    }
    void OpenAuxiliaryWindow()
    {
        _EditMeshVertexAuxiliary = EditorWindow.GetWindowWithRect<EditMeshVertexAuxiliary>(new Rect(0, 0, 400, 260), false, "模型网格编辑器");
        _EditMeshVertexAuxiliary.target = _EditMeshVertexSenior.transform.gameObject;
        _EditMeshVertexAuxiliary.editMeshVertexSenior = _EditMeshVertexSenior;
        _EditMeshVertexAuxiliary.targetClone = _EditMeshVertexSenior._target;
    }
}
