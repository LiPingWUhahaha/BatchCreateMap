using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditVector : EditorWindow
{
    string SavePath = "";
    bool FreeMove = true;
    public static EditVector instance;
    public RaycastHit curHit;
    public Mesh selectMesh;//网格
    MeshCollider selectMeshCollider;
    GameObject selectObj;
    Vector3[] selectObjVectors;//用于还原
    List<Vector3> selectMeshVectorWPs; 
    List<Vector3> selectedVertex;//选择的顶点
    List<int> selectedVertexIndex;//选择的顶点的下标
    void OnEnable()
    {
        instance = this;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        selectObj = Selection.activeGameObject;
        selectMesh =Selection.activeGameObject.GetComponent<MeshFilter>().mesh;
        selectMeshCollider = Selection.activeGameObject.GetComponent<MeshCollider>();
        selectObjVectors = selectMesh.vertices;


        //获取游戏物体上的顶点在世界空间的坐标
        selectMeshVectorWPs = LocalToWorld(selectMesh.vertices, selectObj.transform);


    }

    #region 工具函数
    List<Vector3> LocalToWorld(Vector3[] s,Transform objTF)
    {
        List<Vector3> vector3s = new List<Vector3>();
        foreach (Vector3 i in s)
        {
            vector3s.Add(objTF.TransformPoint(i));
        }
        return vector3s;
    }
    Vector3[] WorldToLocal(List<Vector3> s, Transform objTF)
    {
        Vector3[] vector3s =new Vector3[s.Count];
        for (int i=0;i< s.Count;i++)
        {
            vector3s[i] = objTF.InverseTransformPoint(s[i]);
        }
        return vector3s;
    }
    #endregion


    void OnDisable()
    {
        instance = null;
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    bool isShowSelectVector = false;
    void OnSceneGUI(SceneView sceneView)
    {
        //绘制选择框
        Handles.color = Color.green;
        Handles.BeginGUI();
        if (starSelectPos != Vector2.zero)
        {

            Vector2 difference = mousePos - starSelectPos;
            difference = new Vector2(Mathf.Abs(difference.x), Mathf.Abs(difference.y));
            GUILayout.BeginArea(new Rect(starSelectPos, difference));
            GUILayout.Box("", SelectMouesDraw(), GUILayout.Height(difference.y), GUILayout.Width(difference.x));
            GUILayout.EndArea();


        }
        Handles.EndGUI();

        Handles.color = Color.red;
 
        //处理选择好的顶点
        if (isShowSelectVector)
        {
            SelectVertex(sceneView.camera);
            starSelectPos = Vector2.zero;
            isShowSelectVector = false;
        }
        //绘制选择的顶点并编辑
        if (selectedVertex != null)
        {

            Vector3 vtr3=Vector3.zero;
            Vector3 delta = Vector3.zero;
            foreach (Vector3 item in selectedVertex)
            {
                Vector3 lastVtr = new Vector3(item.x, item.y, item.z);
                
                //给选中的点挂载拖动的方法
                if (FreeMove)
                {
                    vtr3=Handles.FreeMoveHandle(lastVtr, Quaternion.identity, 0.005f * sceneView.size, Vector3.zero, Handles.DotCap);
                }
                else
                {
                    vtr3=Handles.PositionHandle(lastVtr, Quaternion.identity);
                }

                if(lastVtr!= vtr3)//有点被移动
                {
                    delta = vtr3 - lastVtr;
                    break;
                }
                
            }
            //更新网格
            for (int k=0;k< selectedVertexIndex.Count;k++)
            {
                selectMeshVectorWPs[selectedVertexIndex[k]] += delta;
                selectedVertex[k] = selectMeshVectorWPs[selectedVertexIndex[k]];
            }
            selectMesh.vertices= WorldToLocal(selectMeshVectorWPs, selectObj.transform);

            selectMesh.RecalculateNormals();
            selectMesh.RecalculateBounds();
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));//替换Uinity自带的默认控制
        SelectVectorGUI();
        sceneView.Repaint();
    }

    /// <summary>
    /// 选择顶点
    /// </summary>
    /// <param name="camera"></param>
    void SelectVertex(Camera camera)
    {
        //获取模型点在屏幕上的位置
        List<Vector3> vector3s = new List<Vector3>();
        foreach (Vector3 i in selectMeshVectorWPs)
        {
            vector3s.Add(camera.WorldToScreenPoint(i));
        }
        
        //获取在选取内的模型顶点
        float minX = Math.Min(mousePos.x, starSelectPos.x);
        float maxX = Math.Max(mousePos.x, starSelectPos.x);
        float minY = Math.Min(mousePos.y, starSelectPos.y);
        float maxY = Math.Max(mousePos.y, starSelectPos.y);
        selectedVertex = new List<Vector3>();
        selectedVertexIndex = new List<int>();
        for (int i=0;i< vector3s.Count;i++)
        {
            if (vector3s[i].x>= minX&& vector3s[i].x <= maxX && camera.scaledPixelHeight-vector3s[i].y <= maxY && camera.scaledPixelHeight-vector3s[i].y >= minY )//这里坐标有坑 选区是左上角为原点 实际上是左下角
            {
                selectedVertex.Add(selectMeshVectorWPs[i]);
                selectedVertexIndex.Add(i);
            }
        }
    }
    Vector2 mousePos;
    Vector2 starSelectPos;
    /// <summary>
    /// 检测输入
    /// </summary>
    void SelectVectorGUI()
    {
       
        Event e = Event.current;
        mousePos = e.mousePosition;
       
       
        if (e.type==EventType.MouseDrag&&e.control&&e.button==0)
        {
           
            if (starSelectPos == Vector2.zero)
            {
                starSelectPos = new Vector2(mousePos.x, mousePos.y);
            }
            isShowSelectVector = false;
           /// Debug.LogError("开始选择");
            
        }
        if (e.type == EventType.MouseUp && e.control && e.button == 0)
        {
            isShowSelectVector = true;
        }
        
       
    }
    #region 保存和取消
    void Reset()
    {
        if (selectObjVectors != null)
        {
            selectMesh.vertices = selectObjVectors;
        }
    }
    bool SaveEdit()
    {
        if (SavePath.Equals(""))
        {
            Debug.LogError("没有选择保存路径");
            return false;
        }
        selectMeshCollider.sharedMesh= selectMesh;
        MeshUtility.Optimize(selectMesh);//优化网格
        string savePath = SavePath.Substring(SavePath.IndexOf("Assets"));
        AssetDatabase.CreateAsset(selectMesh, savePath + "/"+ selectObj.name+ ".asset");
        return true;
    }
    #endregion
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        //标签
        GUILayout.BeginHorizontal();
        GUILayout.Box("正在编辑顶点...", TitleBoxStyle(), GUILayout.Height(60), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (GUILayout.Button("选择源文件夹", GUILayout.Height(30), GUILayout.Width(180)))
        {
            SavePath = EditorUtility.OpenFolderPanel("选择源文件夹", "Assets/", "");
        }
        GUILayout.Space(10);
        FreeMove=GUILayout.Toggle(FreeMove,"自由拖拽");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("保存", GUILayout.Height(30)))
        {
            if (SaveEdit())
            {
                instance.Close();
            }
            
        }
        GUILayout.Space(10);
        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
            Reset();
            instance.Close();
        }
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
       
    }
    public GUIStyle TitleBoxStyle()
    {
        GUIStyle boxStyle = new GUIStyle();

        boxStyle.margin = new RectOffset(2, 2, 2, 2);
        boxStyle.border = new RectOffset(2, 2, 2, 2);
        boxStyle.fontSize = 25;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.alignment = TextAnchor.MiddleCenter;
        return boxStyle;
    }
    public GUIStyle SelectMouesDraw()
    {
        GUIStyle boxStyle = new GUIStyle();
        boxStyle.margin = new RectOffset(0, 0, 0, 0);
        boxStyle.border = new RectOffset(2, 2, 2, 2);
        boxStyle.padding = new RectOffset(0, 0, 0, 0);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        Texture2D img = new Texture2D(1,1);
        Color[] colorlist = img.GetPixels();
        colorlist[0] = new Color(0,1,0,0.3f);
        img.SetPixels(colorlist);
        boxStyle.normal.background = img;

        return boxStyle;
    }
}
public class EditVectorManager
{
    //网格
    [MenuItem("GameObject//编辑顶点", false, -1)]
    static void EditTerrainVector()
    {
        EditorWindow ew=EditorWindow.GetWindow<EditVector>(false, "编辑顶点", true);
        ew.Show();
    }
    [MenuItem("GameObject//设置物体在地表", false, -1)]
    static void SetOBJToTerrain()
    {
        GameObject selectOBJ= Selection.activeGameObject;
        foreach (Transform child in selectOBJ.transform)
        {
          
            Vector3 posold = child.position;
            LayerMask lm = 1<<LayerMask.NameToLayer("Terrain");
            RaycastHit hitInfo = new RaycastHit();
            Vector3 o = new Vector3(posold.x, 1000, posold.z);
            Vector3 d = new Vector3(0, -1, 0);
            bool flag = Physics.Raycast(o,d,out hitInfo,5000, lm);

            if (flag)
            {
                child.position = hitInfo.point;
            }

            

           
        }
        Debug.Log("设置物体在地表完成");
    }

}


