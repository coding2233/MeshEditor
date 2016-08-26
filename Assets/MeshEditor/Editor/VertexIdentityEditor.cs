using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VertexIdentity))]
public class VertexIdentityEditor : Editor {

    private VertexIdentity _VertexIdentity;
    private string _Identity = "";

    public void OnEnable()
    {
        _VertexIdentity = (VertexIdentity)target;
    }
    public override void OnInspectorGUI()
    {
        if (_VertexIdentity._Identity == -1)
        {
            _Identity = "请注意！这是一个不合法的顶点！";
            GUILayout.Label(_Identity, "ErrorLabel");
        }
        else
        {
            _Identity = "这是一个合法的顶点！";
            GUILayout.Label(_Identity, "BoldLabel");
        }
    }
}
