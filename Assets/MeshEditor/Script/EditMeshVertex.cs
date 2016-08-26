using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
/// <summary>
/// 模型网格编辑器
/// </summary>
#region 前缀
[ExecuteInEditMode, DisallowMultipleComponent, AddComponentMenu("模型网格编辑器/普通")]
#endregion
public class EditMeshVertex : MonoBehaviour {

    //注：编辑完成之后，移除此脚本，在场景运行前，最好确保此脚本不再存在于任一物体上
    #region 变量
    //顶点大小
    [SerializeField,Header("顶点大小"), Range(0, 1)] public float _VertexSize = 0.05f;
    //顶点大小缓存
    [System.NonSerialized] public float _LastVertexSize;
    //顶点数量
    [SerializeField, Header("顶点数量")] public int _VertexNumber = 0;
    //最大处理顶点数
    [System.NonSerialized] public int _VertexNum = 10000;
    //所有顶点物体集合
    [System.NonSerialized] public GameObject[] _Vertices = new GameObject[0];
    //物体网格
    [System.NonSerialized] public Mesh _Mesh;
    //顶点物体数量
    [System.NonSerialized] public int _VerticesNum = 0;
    //所有重复顶点集合
    [System.NonSerialized] public List<List<int>> _AllVerticesGroupList;
    //所有顶点集合
    [System.NonSerialized] public List<Vector3> _AllVerticesList;
    //筛选后的顶点集合
    [System.NonSerialized] public List<Vector3> _VerticesList;
    //所有需移除的顶点集合
    [System.NonSerialized] public List<int> _VerticesRemoveList;
    //用于筛选的顶点集合
    [System.NonSerialized] public List<int> _VerticesSubList;
    //目标物体
    [System.NonSerialized] public GameObject _target;
    //克隆体名称
    [System.NonSerialized] public string _Name = "复制体";
    #endregion

    #region 函数
    void Start () {
        #region 无法处理
        if (GetComponent<MeshFilter>() == null)
        {
            DestroyImmediate(GetComponent<EditMeshVertex>());
            EditorUtility.DisplayDialog("警告", "游戏物体缺少组件 MeshFilter！", "确定");
            return;
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            DestroyImmediate(GetComponent<EditMeshVertex>());
            EditorUtility.DisplayDialog("警告", "游戏物体缺少组件 MeshRenderer！", "确定");
            return;
        }
        if (GetComponent<MeshRenderer>().sharedMaterial == null)
        {
            DestroyImmediate(GetComponent<EditMeshVertex>());
            EditorUtility.DisplayDialog("警告", "游戏物体无材质！", "确定");
            return;
        }
        _VerticesNum = GetComponent<MeshFilter>().sharedMesh.vertices.Length;
        if (_VerticesNum > _VertexNum)
        {
            _VerticesNum = 0;
            DestroyImmediate(GetComponent<EditMeshVertex>());
            EditorUtility.DisplayDialog("警告", "游戏物体顶点太多，我无法处理！", "确定");
            return;
        }
        if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
            Debug.Log("游戏物体的缩放已归为初始值");
        }
        #endregion

        #region 识别顶点
        _AllVerticesGroupList = new List<List<int>>();
        _AllVerticesList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices);
        _VerticesList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices);
        _VerticesRemoveList = new List<int>();
        
        //循环遍历并记录重复顶点
        for (int i = 0; i < _VerticesList.Count; i++)
        {
            EditorUtility.DisplayProgressBar("识别顶点", "正在识别顶点（" + i + "/" + _VerticesList.Count + "）......", 1.0f/ _VerticesList.Count * i);
            //已存在于删除集合的顶点不计算
            if (_VerticesRemoveList.IndexOf(i) >= 0)
                continue;

            _VerticesSubList = new List<int>();
            _VerticesSubList.Add(i);
            int j = i + 1;
            //发现重复顶点，将之记录在内，并加入待删除集合
            while (j < _VerticesList.Count)
            {
                if (_VerticesList[i] == _VerticesList[j])
                {
                    _VerticesSubList.Add(j);
                    _VerticesRemoveList.Add(j);
                }
                j++;
            }
            //记录重复顶点集合
            _AllVerticesGroupList.Add(_VerticesSubList);
        }
        //整理待删除集合
        _VerticesRemoveList.Sort();
        //删除重复顶点
        for (int i = _VerticesRemoveList.Count - 1; i >= 0; i--)
        {
            _VerticesList.RemoveAt(_VerticesRemoveList[i]);
        }
        _VerticesRemoveList.Clear();
        #endregion
        
        #region 创建顶点
        _VerticesNum = _VerticesList.Count;
        _VertexNumber = _VerticesNum;
        //创建顶点，应用顶点大小设置，顶点位置为删除重复顶点之后的集合
        _Vertices = new GameObject[_VerticesNum];
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayProgressBar("创建顶点", "正在创建顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            _Vertices[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _Vertices[i].name = "Vertex";
            _Vertices[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
            _Vertices[i].transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(_VerticesList[i]);
            _Vertices[i].GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
            _Vertices[i].transform.SetParent(transform);
        }
        _LastVertexSize = _VertexSize;
        #endregion

        #region 重构网格
        Transform _obj = transform.FindChild(transform.name + "(Clone)");
        if(_obj != null) DestroyImmediate(_obj.gameObject);
        _obj = null;

        _target = new GameObject(transform.name + "(Clone)");
        _target.transform.position = transform.position;
        _target.transform.rotation = transform.rotation;
        _target.transform.localScale = transform.localScale;
        _target.transform.SetParent(transform);

        _target.AddComponent<MeshFilter>();
        _target.AddComponent<MeshRenderer>();
        _target.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;

        _Mesh = new Mesh();
        _Mesh.Clear();
        _Mesh.vertices = _AllVerticesList.ToArray();
        _Mesh.triangles = GetComponent<MeshFilter>().sharedMesh.triangles;
        _Mesh.uv = GetComponent<MeshFilter>().sharedMesh.uv;
        _Mesh.name = _Name + transform.name;
        _Mesh.RecalculateNormals();
        _target.GetComponent<MeshFilter>().sharedMesh = _Mesh;

        GetComponent<MeshRenderer>().enabled = false;
        EditorUtility.ClearProgressBar();
        #endregion
    }
    void Update () {
        RefishMesh();
        if (_LastVertexSize != _VertexSize)
            RefishVertexSize();
    }
    /// <summary>
    /// 刷新网格
    /// </summary>
    void RefishMesh()
    {
        if (_Mesh != null)
        {
            //重新应用顶点位置
            for (int i = 0; i < _Vertices.Length; i++)
            {
                for (int j = 0; j < _AllVerticesGroupList[i].Count; j++)
                {
                    _AllVerticesList[_AllVerticesGroupList[i][j]] = _target.transform.worldToLocalMatrix.MultiplyPoint3x4(_Vertices[i].transform.position);
                }
            }
            //刷新网格
            _Mesh.vertices = _AllVerticesList.ToArray();
            _Mesh.RecalculateNormals();
        }
    }
    /// <summary>
    /// 刷新顶点大小
    /// </summary>
    void RefishVertexSize()
    {
        //刷新顶点大小
        if (_Vertices.Length > 0)
            if (_LastVertexSize != _VertexSize)
            {
                for (int i = 0; i < _Vertices.Length; i++)
                {
                    _Vertices[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
                }
                _LastVertexSize = _VertexSize;
            }
    }
    /// <summary>
    /// 编辑结束
    /// </summary>
    void OnDestroy()
    {
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayProgressBar("应用顶点", "正在应用顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            if (_Vertices[i] != null)
                DestroyImmediate(_Vertices[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    #endregion
}
