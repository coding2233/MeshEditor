using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
/// <summary>
/// 模型网格编辑辅助器
/// </summary>
public class EditMeshVertexAuxiliary : EditorWindow
{
    [@MenuItem("Window/模型网格编辑辅助界面")]
    static void main()
    {
        EditorWindow.GetWindowWithRect<EditMeshVertexAuxiliary>(new Rect(0,0,400,260),false, "模型网格编辑器");
    }

    public GameObject target;
    public GameObject targetClone;
    public EditMeshVertexSenior editMeshVertexSenior;
    private Vector2 scrollVec2;
    private string targetName;
    private string targetCloneName;
    private float vertexSize;
    private float oldVertexSize;

    void OnGUI()
    {
        scrollVec2 = EditorGUILayout.BeginScrollView(scrollVec2, GUILayout.Width(position.width), GUILayout.Height(position.height));

        #region 打开网页
        EditorGUILayout.BeginHorizontal("MeTransitionHead");
        if (GUILayout.Button("访问CSDN博客", "toolbarbutton"))
        {
            Help.BrowseURL(@"http://blog.csdn.net/qq992817263/article/details/51579913");
        }
        if (GUILayout.Button("访问github项目", "toolbarbutton"))
        {
            Help.BrowseURL(@"https://github.com/coding2233/MeshEditor");
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 当前编辑目标
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("当前编辑目标:");
        if (target != null)
        {
            targetName = target.name;
            if (GUILayout.Button(targetName,GUILayout.Width(200)))
                EditorApplication.delayCall += SelectTarget;
        }
        else
        {
            GUILayout.Label("无", "ErrorLabel");
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 目标克隆体
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("目标克隆体:");
        if (targetClone != null)
        {
            targetCloneName = targetClone.name;
            if (GUILayout.Button(targetCloneName, GUILayout.Width(200)))
                EditorApplication.delayCall += SelectTargetClone;
        }
        else
        {
            GUILayout.Label("无", "ErrorLabel");
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 顶点大小
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("顶点大小:");
        if (target != null && editMeshVertexSenior != null)
        {
            vertexSize = GUILayout.HorizontalSlider(vertexSize,0,1);
            if (vertexSize != oldVertexSize)
            {
                editMeshVertexSenior._VertexSize = oldVertexSize = vertexSize;
                editMeshVertexSenior.RefishVertexSize();
            }
        }
        else
        {
            GUILayout.Label("无", "ErrorLabel");
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 顶点数量
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("顶点数量:");
        if (target != null && editMeshVertexSenior != null)
        {
            GUILayout.Label(editMeshVertexSenior._VertexNumber.ToString());
        }
        else
        {
            GUILayout.Label("无", "ErrorLabel");
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 创建、删除
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("创建顶点", "LargeButtonLeft"))
        {
            EditorApplication.delayCall += AddVertex;
        }
        if (GUILayout.Button("删除顶点", "LargeButtonRight"))
        {
            EditorApplication.delayCall += DeleteVertex;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 相交、镜像
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("相交位移", "LargeButtonLeft"))
        {
            EditorApplication.delayCall += IntersectionDisplacement;
        }
        if (GUILayout.Button("镜像位移", "LargeButtonRight"))
        {
            EditorApplication.delayCall += MirrorDisplacement;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 两点塌陷、多点塌陷
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("两点塌陷", "LargeButtonLeft"))
        {
            EditorApplication.delayCall += CollapseOnTwoVertex;
        }
        if (GUILayout.Button("多点塌陷", "LargeButtonRight"))
        {
            EditorApplication.delayCall += CollapseOnMoreVertex;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 取消编辑
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("取消编辑", "LargeButton"))
        {
            EditorApplication.delayCall += CancelEditor;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 编辑完成
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("编辑完成", "LargeButton"))
        {
            EditorApplication.delayCall += Finish;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.EndScrollView();
    }
    /// <summary>
    /// 选中目标
    /// </summary>
    void SelectTarget()
    {
        Selection.activeGameObject = target;
    }
    /// <summary>
    /// 选中目标克隆体
    /// </summary>
    void SelectTargetClone()
    {
        Selection.activeGameObject = targetClone;
    }
    /// <summary>
    /// 创建顶点
    /// </summary>
    void AddVertex()
    {
        #region 判断是否可以创建顶点
        if (Selection.gameObjects.Length != 2)
        {
            EditorUtility.DisplayDialog("提示", "请选中两个存在直接连线的顶点，以在两个顶点之间创建新顶点！", "确定");
            return;
        }
        //记录选中的两个顶点物体
        GameObject SeObj1 = Selection.gameObjects[0];
        GameObject SeObj2 = Selection.gameObjects[1];
        //获取操作的目标物体
        if (SeObj1.transform.parent == null)
        {
            EditorUtility.DisplayDialog("提示", "未选中两个合法的顶点！", "确定");
            return;
        }
        if (SeObj1.GetComponent<VertexIdentity>() == null || SeObj2.GetComponent<VertexIdentity>() == null
            || SeObj1.GetComponent<VertexIdentity>()._Identity < 0 || SeObj2.GetComponent<VertexIdentity>()._Identity < 0
             || SeObj1.GetComponent<VertexIdentity>()._Identity == SeObj2.GetComponent<VertexIdentity>()._Identity)
        {
            EditorUtility.DisplayDialog("提示", "未选中顶点或选中了不合法的顶点！", "确定");
            return;
        }
        
        target = SeObj1.transform.parent.gameObject;
        //获取目标物体的模型网格编辑器（高级）
        editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
        if (editMeshVertexSenior == null)
        {
            EditorUtility.DisplayDialog("提示", "意外的目标物体，该物体缺少模型网格编辑器（高级）！", "确定");
            return;
        }
        targetClone = editMeshVertexSenior._target;
        oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
        #endregion

        #region 创建顶点并设置顶点参数
        //新的顶点所关联到的所有三角面
        List<List<int>> triangles = new List<List<int>>();
        triangles = IsContainOnTwoVertex(SeObj1, SeObj2);
        //新的顶点关联到的三角面为空，此顶点无法创建
        if (triangles.Count <= 0)
        {
            EditorUtility.DisplayDialog("提示", "此两点不存在直接连线，无法创建新的顶点！", "确定");
            return;
        }
        //记录选中的两个顶点的重复顶点组
        List<int> vertex1 = editMeshVertexSenior._AllVerticesGroupList[SeObj1.GetComponent<VertexIdentity>()._Identity];
        List<int> vertex2 = editMeshVertexSenior._AllVerticesGroupList[SeObj2.GetComponent<VertexIdentity>()._Identity];
        //创建新的重复顶点组（需要加入重复顶点集合）
        List<int> vertexGroup = new List<int>();
        //创建新的UV坐标组，用于更新目标物体uv
        Vector2[] uv = editMeshVertexSenior._Mesh.uv;
        //计算新顶点的UV
        Vector2[] Newuv = new Vector2[uv.Length + 1];
        for (int j = 0; j < uv.Length; j++)
        {
            Newuv[j] = uv[j];
        }
        int num1 = 0;
        if (triangles[0].IndexOf(vertex1[0]) >= 0) num1 = 0;
        else if (triangles[0].IndexOf(vertex1[1]) >= 0) num1 = 1;
        else if (triangles[0].IndexOf(vertex1[2]) >= 0) num1 = 2;
        int num2 = 0;
        if (triangles[0].IndexOf(vertex2[0]) >= 0) num2 = 0;
        else if (triangles[0].IndexOf(vertex2[1]) >= 0) num2 = 1;
        else if (triangles[0].IndexOf(vertex2[2]) >= 0) num2 = 2;
        Newuv[Newuv.Length - 1] = new Vector2((uv[vertex1[num1]].x - uv[vertex2[num2]].x) / 2 + uv[vertex2[num2]].x
            , (uv[vertex1[num1]].y - uv[vertex2[num2]].y) / 2 + uv[vertex2[num2]].y);
        uv = Newuv;
        //为此新顶点所关联的所有面创建一个新的共用顶点
        Vector3 vertex = CreateVertexOnTwoVertex(SeObj1, SeObj2);
        //将新的顶点加入所有顶点集合
        editMeshVertexSenior._AllVerticesList.Add(vertex);
        //记录新的顶点索引
        int _index = editMeshVertexSenior._AllVerticesList.IndexOf(vertex);
        //将新的顶点加入筛选后的顶点集合
        editMeshVertexSenior._VerticesList.Add(vertex);
        //将新的顶点的索引加入重复顶点集合
        vertexGroup.Add(_index);
        //创建新的顶点物体
        GameObject vertexObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        vertexObj.name = "Vertex";
        vertexObj.transform.localScale = new Vector3(editMeshVertexSenior._VertexSize, editMeshVertexSenior._VertexSize, editMeshVertexSenior._VertexSize);
        vertexObj.transform.position = target.transform.localToWorldMatrix.MultiplyPoint3x4(vertex);
        vertexObj.GetComponent<MeshRenderer>().sharedMaterial = target.GetComponent<MeshRenderer>().sharedMaterial;
        vertexObj.AddComponent<VertexIdentity>();
        vertexObj.GetComponent<VertexIdentity>()._Identity = editMeshVertexSenior._Vertices.Length;
        vertexObj.transform.SetParent(target.transform);
        //更新所有顶点物体集合
        GameObject[] _Vertexs = new GameObject[editMeshVertexSenior._Vertices.Length + 1];
        for (int j = 0; j < editMeshVertexSenior._Vertices.Length; j++)
        {
            _Vertexs[j] = editMeshVertexSenior._Vertices[j];
        }
        _Vertexs[_Vertexs.Length - 1] = vertexObj;
        editMeshVertexSenior._Vertices = _Vertexs;
        #endregion

        #region 创建三角面
        //所有面以新顶点破开，原理是将选中的两个顶点组成的所有面都以新顶点破开为两个面
        for (int i = 0; i < triangles.Count; i++)
        {
            //移除旧的三角面
            editMeshVertexSenior._AllTriangleList.Remove(triangles[i]);
            //创建新的三角面，将旧的三角面分离为两个新的三角面
            List<int> triangle1 = new List<int>();
            List<int> triangle2 = new List<int>();
            if ((vertex1.IndexOf(triangles[i][0]) >= 0 && vertex2.IndexOf(triangles[i][1]) >= 0)
                || (vertex1.IndexOf(triangles[i][1]) >= 0 && vertex2.IndexOf(triangles[i][0]) >= 0))
            {
                //顺时针顺序
                triangle1.Add(triangles[i][0]);
                triangle1.Add(_index);
                triangle1.Add(triangles[i][2]);
                triangle2.Add(_index);
                triangle2.Add(triangles[i][1]);
                triangle2.Add(triangles[i][2]);
            }
            else if ((vertex1.IndexOf(triangles[i][0]) >= 0 && vertex2.IndexOf(triangles[i][2]) >= 0)
                || (vertex1.IndexOf(triangles[i][2]) >= 0 && vertex2.IndexOf(triangles[i][0]) >= 0))
            {
                triangle1.Add(triangles[i][0]);
                triangle1.Add(triangles[i][1]);
                triangle1.Add(_index);
                triangle2.Add(_index);
                triangle2.Add(triangles[i][1]);
                triangle2.Add(triangles[i][2]);
            }
            else if ((vertex1.IndexOf(triangles[i][1]) >= 0 && vertex2.IndexOf(triangles[i][2]) >= 0)
                || (vertex1.IndexOf(triangles[i][2]) >= 0 && vertex2.IndexOf(triangles[i][1]) >= 0))
            {
                triangle1.Add(triangles[i][0]);
                triangle1.Add(triangles[i][1]);
                triangle1.Add(_index);
                triangle2.Add(triangles[i][0]);
                triangle2.Add(_index);
                triangle2.Add(triangles[i][2]);
            }
            //添加新的三角面到所有三角面集合
            editMeshVertexSenior._AllTriangleList.Add(triangle1);
            editMeshVertexSenior._AllTriangleList.Add(triangle2);
        }
        #endregion

        #region 更新目标网格
        int[] Alltriangles = new int[editMeshVertexSenior._AllTriangleList.Count * 3];
        for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
        {
            Alltriangles[i * 3] = editMeshVertexSenior._AllTriangleList[i][0];
            Alltriangles[i * 3 + 1] = editMeshVertexSenior._AllTriangleList[i][1];
            Alltriangles[i * 3 + 2] = editMeshVertexSenior._AllTriangleList[i][2];
        }
        editMeshVertexSenior._AllVerticesGroupList.Add(vertexGroup);
        editMeshVertexSenior._Mesh.Clear();
        editMeshVertexSenior._Mesh.vertices = editMeshVertexSenior._AllVerticesList.ToArray();
        editMeshVertexSenior._Mesh.triangles = Alltriangles;
        editMeshVertexSenior._Mesh.uv = uv;
        editMeshVertexSenior._Mesh.RecalculateNormals();
        editMeshVertexSenior._VertexNumber = editMeshVertexSenior._Vertices.Length;
        Selection.activeGameObject = vertexObj;
        #endregion
    }
    /// <summary>
    /// 删除顶点
    /// </summary>
    void DeleteVertex()
    {
        #region 判断是否可以删除顶点
        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("提示", "请选中一个顶点，以删除此顶点！", "确定");
            return;
        }
        //记录选中的顶点物体
        GameObject SeObj1 = Selection.gameObjects[0];
        //获取操作的目标物体
        if (SeObj1.transform.parent == null)
        {
            EditorUtility.DisplayDialog("提示", "未选中合法的顶点！", "确定");
            return;
        }
        if (SeObj1.GetComponent<VertexIdentity>() == null || SeObj1.GetComponent<VertexIdentity>()._Identity < 0)
        {
            EditorUtility.DisplayDialog("提示", "未选中顶点或选中了不合法的顶点！", "确定");
            return;
        }

        target = SeObj1.transform.parent.gameObject;
        //获取目标物体的模型网格编辑器（高级）
        editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
        if (editMeshVertexSenior == null)
        {
            EditorUtility.DisplayDialog("提示", "意外的目标物体，该物体缺少模型网格编辑器（高级）！", "确定");
            return;
        }
        targetClone = editMeshVertexSenior._target;
        oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
        #endregion

        #region 删除顶点
        //待删除顶点所关联到的所有三角面
        List<List<int>> triangles = new List<List<int>>();
        triangles = IsContainOnOneVertex(SeObj1);
        //待删除顶点关联到的三角面为空，此顶点无法删除
        if (triangles.Count <= 0)
        {
            EditorUtility.DisplayDialog("提示", "意外的顶点，此顶点已被删除或是其他原因导致无法删除此顶点！", "确定");
            return;
        }
        //删除顶点所关联的所有面
        for (int i = 0; i < triangles.Count; i++)
        {
            editMeshVertexSenior._AllTriangleList.Remove(triangles[i]);
        }
        SeObj1.GetComponent<VertexIdentity>()._Identity = -1;
        SeObj1.SetActive(false);
        #endregion

        #region 更新目标网格
        int[] Alltriangles = new int[editMeshVertexSenior._AllTriangleList.Count * 3];
        for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
        {
            Alltriangles[i * 3] = editMeshVertexSenior._AllTriangleList[i][0];
            Alltriangles[i * 3 + 1] = editMeshVertexSenior._AllTriangleList[i][1];
            Alltriangles[i * 3 + 2] = editMeshVertexSenior._AllTriangleList[i][2];
        }
        editMeshVertexSenior._Mesh.Clear();
        editMeshVertexSenior._Mesh.vertices = editMeshVertexSenior._AllVerticesList.ToArray();
        editMeshVertexSenior._Mesh.triangles = Alltriangles;
        editMeshVertexSenior._Mesh.RecalculateNormals();
        editMeshVertexSenior._VertexNumber = editMeshVertexSenior._Vertices.Length;
        Selection.activeGameObject = targetClone;
        #endregion
    }
    /// <summary>
    /// 相交位移
    /// </summary>
    void IntersectionDisplacement()
    {
        #region 判断是否可以执行
        if (Selection.gameObjects.Length != 2)
        {
            EditorUtility.DisplayDialog("提示", "请选中两个游戏物体！", "确定");
            return;
        }
        #endregion

        #region 执行
        //记录选中的两个物体
        GameObject SeObj1 = Selection.gameObjects[0];
        GameObject SeObj2 = Selection.gameObjects[1];
        
        Vector3 newPosition = new Vector3((SeObj1.transform.position.x - SeObj2.transform.position.x) / 2.0f + SeObj2.transform.position.x,
            (SeObj1.transform.position.y - SeObj2.transform.position.y) / 2.0f + SeObj2.transform.position.y,
            (SeObj1.transform.position.z - SeObj2.transform.position.z) / 2.0f + SeObj2.transform.position.z);

        SeObj2.transform.position = newPosition;
        SeObj1.transform.position = newPosition;

        if (SeObj1.transform.parent != null &&
            SeObj2.transform.parent != null &&
            SeObj1.transform.parent == SeObj2.transform.parent &&
            SeObj1.transform.parent.GetComponent<EditMeshVertexSenior>() != null)
        {
            target = SeObj1.transform.parent.gameObject;
            editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
            targetClone = editMeshVertexSenior._target;
            oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
        }
        #endregion
    }
    /// <summary>
    /// 镜像位移
    /// </summary>
    void MirrorDisplacement()
    {
        #region 判断是否可以执行
        if (Selection.gameObjects.Length != 2)
        {
            EditorUtility.DisplayDialog("提示", "请选中两个游戏物体！", "确定");
            return;
        }
        #endregion

        #region 执行
        //记录选中的两个物体
        GameObject SeObj1 = Selection.gameObjects[0];
        GameObject SeObj2 = Selection.gameObjects[1];

        Vector3 newPosition = SeObj1.transform.position;
        SeObj1.transform.position = SeObj2.transform.position;
        SeObj2.transform.position = newPosition;

        if (SeObj1.transform.parent != null &&
            SeObj2.transform.parent != null &&
            SeObj1.transform.parent == SeObj2.transform.parent &&
            SeObj1.transform.parent.GetComponent<EditMeshVertexSenior>() != null)
        {
            target = SeObj1.transform.parent.gameObject;
            editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
            targetClone = editMeshVertexSenior._target;
            oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
        }
        #endregion
    }
    /// <summary>
    /// 两点塌陷
    /// </summary>
    void CollapseOnTwoVertex()
    {
        #region 判断是否可以塌陷
        if (Selection.gameObjects.Length != 2)
        {
            EditorUtility.DisplayDialog("提示", "请选中两个合法顶点！", "确定");
            return;
        }
        //记录选中的两个顶点物体
        GameObject SeObj1 = Selection.gameObjects[0];
        GameObject SeObj2 = Selection.gameObjects[1];
        //获取操作的目标物体
        if (SeObj1.transform.parent == null)
        {
            EditorUtility.DisplayDialog("提示", "未选中两个合法的顶点！", "确定");
            return;
        }
        if (SeObj1.GetComponent<VertexIdentity>() == null || SeObj2.GetComponent<VertexIdentity>() == null
            || SeObj1.GetComponent<VertexIdentity>()._Identity < 0 || SeObj2.GetComponent<VertexIdentity>()._Identity < 0
             || SeObj1.GetComponent<VertexIdentity>()._Identity == SeObj2.GetComponent<VertexIdentity>()._Identity)
        {
            EditorUtility.DisplayDialog("提示", "未选中顶点或选中了不合法的顶点！", "确定");
            return;
        }

        target = SeObj1.transform.parent.gameObject;
        //获取目标物体的模型网格编辑器（高级）
        editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
        if (editMeshVertexSenior == null)
        {
            EditorUtility.DisplayDialog("提示", "意外的目标物体，该物体缺少模型网格编辑器（高级）！", "确定");
            return;
        }
        targetClone = editMeshVertexSenior._target;
        oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
        #endregion

        #region 删除包含此两点的所有面
        List<List<int>> triangles = new List<List<int>>();
        triangles = IsContainOnTwoVertex(SeObj1, SeObj2);
        if (triangles.Count > 0)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                editMeshVertexSenior._AllTriangleList.Remove(triangles[i]);
            }
        }
        #endregion

        #region 将包含‘被塌陷点’的所有面数据更改为包含‘塌陷点’
        List<int> vertexNumber = editMeshVertexSenior._AllVerticesGroupList[SeObj2.GetComponent<VertexIdentity>()._Identity];
        int targetNumber = editMeshVertexSenior._AllVerticesGroupList[SeObj1.GetComponent<VertexIdentity>()._Identity][0];
        triangles = IsContainOnOneVertex(SeObj2);
        if (triangles.Count > 0)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                for (int j = 0; j < vertexNumber.Count; j++)
                {
                    int number = triangles[i].IndexOf(vertexNumber[j]);
                    if (number >= 0)
                    {
                        triangles[i][number] = targetNumber;
                    }
                }
            }
        }
        SeObj2.GetComponent<VertexIdentity>()._Identity = -1;
        SeObj2.SetActive(false);
        #endregion

        #region 更新目标网格
        int[] Alltriangles = new int[editMeshVertexSenior._AllTriangleList.Count * 3];
        for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
        {
            Alltriangles[i * 3] = editMeshVertexSenior._AllTriangleList[i][0];
            Alltriangles[i * 3 + 1] = editMeshVertexSenior._AllTriangleList[i][1];
            Alltriangles[i * 3 + 2] = editMeshVertexSenior._AllTriangleList[i][2];
        }
        editMeshVertexSenior._Mesh.Clear();
        editMeshVertexSenior._Mesh.vertices = editMeshVertexSenior._AllVerticesList.ToArray();
        editMeshVertexSenior._Mesh.triangles = Alltriangles;
        editMeshVertexSenior._Mesh.RecalculateNormals();
        editMeshVertexSenior._VertexNumber = editMeshVertexSenior._Vertices.Length;
        Selection.activeGameObject = targetClone;
        #endregion
    }
    /// <summary>
    /// 多点塌陷
    /// </summary>
    void CollapseOnMoreVertex()
    {
        #region 判断是否可以塌陷
        GameObject[] obj = Selection.gameObjects;
        if (obj.Length <= 2)
        {
            EditorUtility.DisplayDialog("提示", "请选中多个合法顶点！", "确定");
            return;
        }
        #endregion

        #region 轮番执行两点塌陷
        for (int n = 1; n < obj.Length; n++)
        {
            #region 判断是否可以塌陷
            //记录两个顶点物体
            GameObject SeObj1 = obj[0];
            GameObject SeObj2 = obj[n];
            //获取操作的目标物体
            if (SeObj1.transform.parent == null)
            {
                EditorUtility.DisplayDialog("提示", "多个顶点中存在不合法的顶点！", "确定");
                return;
            }
            if (SeObj1.GetComponent<VertexIdentity>() == null || SeObj2.GetComponent<VertexIdentity>() == null
                || SeObj1.GetComponent<VertexIdentity>()._Identity < 0 || SeObj2.GetComponent<VertexIdentity>()._Identity < 0
                 || SeObj1.GetComponent<VertexIdentity>()._Identity == SeObj2.GetComponent<VertexIdentity>()._Identity)
            {
                EditorUtility.DisplayDialog("提示", "多个顶点中存在不合法的顶点！", "确定");
                return;
            }

            target = SeObj1.transform.parent.gameObject;
            //获取目标物体的模型网格编辑器（高级）
            editMeshVertexSenior = target.GetComponent<EditMeshVertexSenior>();
            if (editMeshVertexSenior == null)
            {
                EditorUtility.DisplayDialog("提示", "意外的目标物体，该物体缺少模型网格编辑器（高级）！", "确定");
                return;
            }
            targetClone = editMeshVertexSenior._target;
            oldVertexSize = vertexSize = editMeshVertexSenior._VertexSize;
            #endregion

            #region 删除包含此两点的所有面
            List<List<int>> triangles = new List<List<int>>();
            triangles = IsContainOnTwoVertex(SeObj1, SeObj2);
            if (triangles.Count > 0)
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    editMeshVertexSenior._AllTriangleList.Remove(triangles[i]);
                }
            }
            #endregion

            #region 将包含‘被塌陷点’的所有面数据更改为包含‘塌陷点’
            List<int> vertexNumber = editMeshVertexSenior._AllVerticesGroupList[SeObj2.GetComponent<VertexIdentity>()._Identity];
            int targetNumber = editMeshVertexSenior._AllVerticesGroupList[SeObj1.GetComponent<VertexIdentity>()._Identity][0];
            triangles = IsContainOnOneVertex(SeObj2);
            if (triangles.Count > 0)
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    for (int j = 0; j < vertexNumber.Count; j++)
                    {
                        int number = triangles[i].IndexOf(vertexNumber[j]);
                        if (number >= 0)
                        {
                            triangles[i][number] = targetNumber;
                        }
                    }
                }
            }
            SeObj2.GetComponent<VertexIdentity>()._Identity = -1;
            SeObj2.SetActive(false);
            #endregion

            #region 更新目标网格
            int[] Alltriangles = new int[editMeshVertexSenior._AllTriangleList.Count * 3];
            for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
            {
                Alltriangles[i * 3] = editMeshVertexSenior._AllTriangleList[i][0];
                Alltriangles[i * 3 + 1] = editMeshVertexSenior._AllTriangleList[i][1];
                Alltriangles[i * 3 + 2] = editMeshVertexSenior._AllTriangleList[i][2];
            }
            editMeshVertexSenior._Mesh.Clear();
            editMeshVertexSenior._Mesh.vertices = editMeshVertexSenior._AllVerticesList.ToArray();
            editMeshVertexSenior._Mesh.triangles = Alltriangles;
            editMeshVertexSenior._Mesh.RecalculateNormals();
            editMeshVertexSenior._VertexNumber = editMeshVertexSenior._Vertices.Length;
            Selection.activeGameObject = targetClone;
            #endregion
        }
        #endregion
    }
    /// <summary>
    /// 取消编辑
    /// </summary>
    void CancelEditor()
    {
        if (target != null && editMeshVertexSenior != null)
        {
            DestroyImmediate(editMeshVertexSenior);
            editMeshVertexSenior = null;
            target.GetComponent<MeshRenderer>().enabled = true;
            //原来的物体留之无用，删掉
            if (targetClone)
            {
                DestroyImmediate(targetClone);
            }
        }
    }

    /// <summary>
    /// 编辑完成
    /// </summary>
    void Finish()
    {
        if (target != null && editMeshVertexSenior != null)
        {
            DestroyImmediate(editMeshVertexSenior);
            editMeshVertexSenior = null;
            //原来的物体留之无用，删掉
            if (targetClone)
            {
                targetClone.transform.parent = null;
                targetClone.name = target.name;
                DestroyImmediate(target);
            }
        }
    }
    /// <summary>
    /// 包含此顶点的所有三角面
    /// </summary>
    List<List<int>> IsContainOnOneVertex(GameObject vertex)
    {
        EditMeshVertexSenior editMeshVertexSenior = vertex.transform.parent.gameObject.GetComponent<EditMeshVertexSenior>();
        List<int> line = editMeshVertexSenior._AllVerticesGroupList[vertex.GetComponent<VertexIdentity>()._Identity];

        List<List<int>> triangles = new List<List<int>>();

        for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
        {
            for (int j = 0; j < line.Count; j++)
            {
                if (editMeshVertexSenior._AllTriangleList[i].IndexOf(line[j]) >= 0)
                {
                    triangles.Add(editMeshVertexSenior._AllTriangleList[i]);
                    break;
                }
            }
        }
        return triangles;
    }
    /// <summary>
    /// 包含此两点的所有三角面
    /// </summary>
    List<List<int>> IsContainOnTwoVertex(GameObject vertex1, GameObject vertex2)
    {
        EditMeshVertexSenior editMeshVertexSenior = vertex1.transform.parent.gameObject.GetComponent<EditMeshVertexSenior>();
        List<int> line1 = editMeshVertexSenior._AllVerticesGroupList[vertex1.GetComponent<VertexIdentity>()._Identity];
        List<int> line2 = editMeshVertexSenior._AllVerticesGroupList[vertex2.GetComponent<VertexIdentity>()._Identity];

        List<List<int>> triangles = new List<List<int>>();

        for (int i = 0; i < editMeshVertexSenior._AllTriangleList.Count; i++)
        {
            for (int j = 0; j < line1.Count; j++)
            {
                if (editMeshVertexSenior._AllTriangleList[i].IndexOf(line1[j]) >= 0)
                {
                    for (int k = 0; k < line2.Count; k++)
                    {
                        if (editMeshVertexSenior._AllTriangleList[i].IndexOf(line2[k]) >= 0)
                        {
                            triangles.Add(editMeshVertexSenior._AllTriangleList[i]);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        return triangles;
    }
    /// <summary>
    /// 在两点之间创建新的顶点
    /// </summary>
    Vector3 CreateVertexOnTwoVertex(GameObject vertex1, GameObject vertex2)
    {
        EditMeshVertexSenior editMeshVertexSenior = vertex1.transform.parent.gameObject.GetComponent<EditMeshVertexSenior>();
        Vector3 line1 = editMeshVertexSenior._VerticesList[vertex1.GetComponent<VertexIdentity>()._Identity];
        Vector3 line2 = editMeshVertexSenior._VerticesList[vertex2.GetComponent<VertexIdentity>()._Identity];

        Vector3 newVertex = new Vector3((line1.x - line2.x) / 2.0f + line2.x,
            (line1.y - line2.y) / 2.0f + line2.y,
            (line1.z - line2.z) / 2.0f + line2.z);
        return newVertex;
    }
}
