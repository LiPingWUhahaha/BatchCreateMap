using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Windows;

public class XLZTerrainEditor
{
    [MenuItem("Tools/创建基本地形", false, 21)]
    static void CreateBasicTerrain()
    {
        EditorWindow.GetWindow<InputImageToEditor>(false, "InputImageToEditor", true).Show();
    }
    [MenuItem("Tools/批量创建基本地形", false, 22)]
    static void CreateBasicTerrain2()
    {
        EditorWindow.GetWindow<InputImageToEditor2>(false, "InputImageToEditor", true).Show();
    }

    /// <summary>
    /// 发起Win会话读取选择的图片
    /// </summary>
    public static void OpenImage(int mapWidth,uint mapPD,string savePath,string mapName)
    {
        string path= EditorUtility.OpenFilePanelWithFilters("选择文件", "C:\\Users\\Administrator\\Desktop",  new string[] { "图片格式", "png,jpg,jpeg", "All files", "*" });
        //创建文件读取流
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        Texture2D texture = new Texture2D(1, 1);

        bool isload = texture.LoadImage(bytes);
        //Sprite tempSp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        //GameObject.Find("TestImage").GetComponent<Image>().sprite = tempSp;

        //GameObject tGO = new GameObject("testTerrain");
        //DrawMesh(tGO, texture, mapWidth, mapPD);
        XLZMapMesh xLZMap = new XLZMapMesh(mapName, texture, savePath);
        xLZMap.CreatMesh(mapWidth, mapWidth, mapPD, mapPD, -10, 10);
    }

    /// <summary>
    /// 发起Win会话读取选择的图片
    /// </summary>
    public static void OpenImage2(int mapWidth, uint mapPD, int mapCount, string mapName)
    {
        string folderPath = EditorUtility.OpenFolderPanel("选择文件夹", "Assets/", "");
        string[] paths = Directory.GetFiles(folderPath);

        GameObject[] gameObjects = new GameObject[paths.Length];

        for (int i=0;i< paths.Length;i++)
        {
            int remainder = i % mapCount;//余数
            int multiple = Mathf.FloorToInt(i/ mapCount);//倍数

            //创建文件读取流
            FileStream fileStream = new FileStream(paths[i], FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;
            Texture2D texture = new Texture2D(1, 1);

            bool isload = texture.LoadImage(bytes);

            XLZMapMesh xLZMap = new XLZMapMesh(mapName+"_"+i, texture, "");
            xLZMap.CreatMesh(mapWidth, mapWidth, mapPD, mapPD, -10, 10);

            GameObject go = xLZMap.MMesh;
            go.transform.position = new Vector3(-mapWidth* multiple,0, remainder* mapWidth);
            gameObjects[i] = go;
        }
        float maxDis = mapWidth / mapPD;
        int pd = (int)mapPD;
        int pdMax = pd * (pd+1);
        for (int i = 0; i < mapCount; i++)
        {
            for (int j = 0; j < mapCount; j++)
            {
                if (j < mapCount - 1)//先Z轴贴合
                {
                    Mesh mesh1 = gameObjects[i * mapCount + j].GetComponent<MeshFilter>().sharedMesh;//网格
                    Mesh mesh2 = gameObjects[i * mapCount + j + 1].GetComponent<MeshFilter>().sharedMesh;//网格

                    Mesh nmesh1 = new Mesh();
                    Mesh nmesh2 = new Mesh();
                    nmesh1.name = mesh1.name;
                    nmesh2.name = mesh2.name;
                    Vector3[] m1vs = mesh1.vertices;
                    Vector3[] m2vs = mesh2.vertices;

                    // Vector3[] m1vsz = new Vector3[mapPD+1];
                    for (int k = 0; k < pd + 1; k++)
                    {
                        Vector3 v1 = m1vs[pdMax + k];
                        Vector3 v2 = m2vs[k];

                        Vector3 wv1 = gameObjects[i * mapCount + j].transform.TransformPoint(v1);
                        Vector3 wv2 = gameObjects[i * mapCount + j + 1].transform.TransformPoint(v2);

                        Vector3 v = (wv1 + wv2) / 2;

                        m1vs[pdMax + k]= gameObjects[i * mapCount + j].transform.InverseTransformPoint(v);
                        m2vs[k] = gameObjects[i * mapCount + j + 1].transform.InverseTransformPoint(v);
                    }
                    nmesh1.Clear();//更新
                    nmesh1.vertices = m1vs;
                    nmesh1.triangles = mesh2.triangles;
                    nmesh1.RecalculateNormals();
                    nmesh1.RecalculateBounds();
                    nmesh2.Clear();//更新
                    nmesh2.vertices = m2vs;
                    nmesh2.triangles = mesh2.triangles;
                    nmesh2.RecalculateNormals();
                    nmesh2.RecalculateBounds();
                    gameObjects[i * mapCount + j].GetComponent<MeshFilter>().mesh = nmesh1;
                    gameObjects[i * mapCount + j + 1].GetComponent<MeshFilter>().mesh = nmesh2;
                    //mesh2.vertices = m2vs;

                }
                if (i < mapCount - 1)//再X轴贴合
                {
                    Mesh mesh1 = gameObjects[i * mapCount + j].GetComponent<MeshFilter>().sharedMesh;//网格
                    Mesh mesh2 = gameObjects[i * mapCount + j + mapCount].GetComponent<MeshFilter>().sharedMesh;//网格

                    Mesh nmesh1 = new Mesh();
                    Mesh nmesh2 = new Mesh();
                    nmesh1.name = mesh1.name;
                    nmesh2.name = mesh2.name;
                    Vector3[] m1vs = mesh1.vertices;
                    Vector3[] m2vs = mesh2.vertices;

                    // Vector3[] m1vsz = new Vector3[mapPD+1];
                    for (int k = 0; k < pd + 1; k++)
                    {
                        Vector3 v1 = m1vs[(pd + 1) * k];//
                        Vector3 v2 = m2vs[(pd + 1) * k + pd];

                        Vector3 wv1 = gameObjects[i * mapCount + j].transform.TransformPoint(v1);
                        Vector3 wv2 = gameObjects[i * mapCount + j + mapCount].transform.TransformPoint(v2);

                        Vector3 v = (wv1 + wv2) / 2;

                        m1vs[(pd + 1) * k] = gameObjects[i * mapCount + j].transform.InverseTransformPoint(v);
                        m2vs[(pd + 1) * k + pd] = gameObjects[i * mapCount + j + mapCount].transform.InverseTransformPoint(v);
                    }
                    nmesh1.Clear();//更新
                    nmesh1.vertices = m1vs;
                    nmesh1.triangles = mesh2.triangles;
                    nmesh1.RecalculateNormals();
                    nmesh1.RecalculateBounds();
                    nmesh2.Clear();//更新
                    nmesh2.vertices = m2vs;
                    nmesh2.triangles = mesh2.triangles;
                    nmesh2.RecalculateNormals();
                    nmesh2.RecalculateBounds();
                    gameObjects[i * mapCount + j].GetComponent<MeshFilter>().mesh = nmesh1;
                    gameObjects[i * mapCount + j + mapCount].GetComponent<MeshFilter>().mesh = nmesh2;

                }
            }

        }
    }
    /// <summary>
    /// 创建地图
    /// </summary>
    /// <param name="mapName">地图名称</param>
    /// <param name="mapWidth">小地图行数</param>
    /// <param name="mapLength">小地图列数</param>
    /// <param name="mapHight">地图高度</param>
    /// <param name="highttextureAcc">高度纹理精度</param>
    /// <param name="sevepath">保存路径</param>
    /// <param name="rawpath">.raw文件路径</param>
    /// <param name="texture2DArr">贴图数组</param>
    public static void CreateTerrain(string mapName, int mapWidth, int mapLength, int mapHight,float highttextureAcc, string sevepath, string rawpath,Texture2D[] texture2DArr)
    {
        string[] rawpaths = Directory.GetFiles(rawpath);

        if (rawpaths.Length != mapLength * mapWidth)
        {
            Debug.LogError(".raw资源不足或行列数配置错误！");
            return;
        }
        string foderPath = sevepath + "/" + mapName;
        if (Directory.Exists(foderPath))
        {
            DeleteDirectory(foderPath);
        }
        DirectoryInfo directoryinfo = Directory.CreateDirectory(foderPath);
        AssetDatabase.Refresh();
        GameObject rootGo = new GameObject(mapName);

        string localpath = foderPath.Substring(foderPath.IndexOf("Assets"))+"/";
        TerrainLayer[] tls = new TerrainLayer[texture2DArr.Length];
        for (int k = 0; k < texture2DArr.Length; k++)
        {
            TerrainLayer tl = new TerrainLayer();
            AssetDatabase.CreateAsset(tl, localpath+"TerrainLayer_" + k + ".asset");
            tl.diffuseTexture = texture2DArr[k];
            tls[k] = tl;
        }

        for (int i = 0; i < mapLength; i++)
        {
            for (int j=0;j< mapWidth; j++)
            {


                TerrainData terrainData = new TerrainData();
                AssetDatabase.CreateAsset(terrainData, localpath+"TD_" + (i * mapWidth + j)+".asset");
                terrainData.heightmapResolution = 513;
                terrainData.SetDetailResolution(512,32);
                terrainData.terrainLayers = tls;
                terrainData.size = new Vector3(50, mapHight, 50);
                GameObject go = Terrain.CreateTerrainGameObject(terrainData);
                go.name = "T_" + (i * mapWidth + j);
                go.transform.parent = rootGo.transform;
                go.transform.localPosition = new Vector3(j*50,0,i*50);
                ImportRawHeightmap importRawHeightmap = new ImportRawHeightmap();
                Terrain ter = go.GetComponent<Terrain>();
                ter.heightmapPixelError = highttextureAcc;
                importRawHeightmap.InitializeImportRaw(ter, rawpaths[i * mapWidth + j]);
                PrefabUtility.SaveAsPrefabAssetAndConnect(go, localpath + mapName +"_"+ (i * mapWidth + j) + ".prefab", InteractionMode.AutomatedAction);
            }
        }
        
    }
    /// <summary>
    /// 删除文件夹以及文件
    /// </summary>
    /// <param name="directoryPath"> 文件夹路径 </param>
    /// <param name="fileName"> 文件名称 </param>
    public static void DeleteDirectory(string directoryPath)
    {
       
        //删除文件
        for (int i = 0; i < Directory.GetFiles(directoryPath).ToList().Count; i++)
        {
            File.Delete(Directory.GetFiles(directoryPath)[i]);
        }
        Directory.Delete(directoryPath, true);
       // AssetDatabase.Refresh();
       
    }
}

public class InputImageToEditor : EditorWindow
{
    public static InputImageToEditor instance;

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }
    public Texture icon;
    string mapWidth="10", mapLength="0", mapPd="0";
    string savePath;
    string mapName;
    bool IsDefault = true;
    void OnGUI()
    {
        
        GUILayout.BeginVertical();
        GUILayout.Space(10);
      
        GUI.skin.label.fontSize = 14;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.Label("输入相应参数");
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin.label.fontSize = 12;
        GUILayout.Space(10);
        mapName = EditorGUILayout.TextField("设置地图名字", mapName);
        GUILayout.Space(10);
        mapWidth = EditorGUILayout.TextField("设置地图宽度", mapWidth);
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(true);  //如果nextPath == null 为真，在Inspector面板上显示，承灰色（即不可操作）  
        mapLength = EditorGUILayout.TextField("设置地图长度", mapWidth);
        EditorGUI.EndDisabledGroup();
        GUILayout.Space(10);
     
        IsDefault = EditorGUILayout.Toggle("是否使用默认定点密度", IsDefault);
        if (!IsDefault)
        {
            GUILayout.Space(10);
            mapPd = EditorGUILayout.TextField("设置地图定点密度（建议为宽度X5）", mapPd);
        }
        else
        {
            mapPd = ""+int.Parse(mapWidth) * 5;
        }

        GUILayout.Space(10);
        if (GUILayout.Button("选择保存文件夹"))
        {
            savePath = EditorUtility.OpenFolderPanel("选择保存的文件夹", "Assets/", "");
        }
        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("确定"))
        {
            if (mapWidth.Length > 0)
            {
                XLZTerrainEditor.OpenImage(int.Parse(mapWidth), uint.Parse(mapPd), savePath,mapName);
                instance.Close();
                EditorUtility.DisplayDialog("创建结果", "创建成功", "确定");
            }
            else
            {
                Debug.LogError("请输入相应的值");
            }

        }
        GUILayout.Space(120);
        if (GUILayout.Button("取消"))
        {
            instance.Close();
        }
        GUILayout.EndHorizontal();

    }
}
public class InputImageToEditor2 : EditorWindow
{
    public static InputImageToEditor2 instance;

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }
    public Texture2D[] texture2s=new Texture2D[1];
    public int maptextureCount = 1;
    public Texture icon;
    int mapWidth = 0, mapLength = 0,mapHight=0;
    string savePath;
    string mapName;
    bool IsDefault = true;
    string rawpath = "";
    string sevepath = "";
    float highttextureAcc = 1;
    void OnGUI()
    {
        
        EditorGUILayout.Space();
        GUILayout.BeginVertical();
        GUILayout.Space(10);
      
        GUI.skin.label.fontSize = 14;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUILayout.Label("输入相应参数");
        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin.label.fontSize = 12;
        GUILayout.Space(10);
        mapName = EditorGUILayout.TextField("设置地图名字", mapName);
        GUILayout.Space(10);
        mapWidth = EditorGUILayout.IntField("设置小地图行数", mapWidth);
        GUILayout.Space(10);
      
        mapLength = EditorGUILayout.IntField("设置小地图列数", mapLength);
    
        GUILayout.Space(10);
        mapHight = EditorGUILayout.IntField("设置地图高度", mapHight);
        GUILayout.Space(10);
        highttextureAcc= EditorGUILayout.Slider("设置高度贴图精度",highttextureAcc, 1, 200);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择保存目录"))
        {
            sevepath = EditorUtility.OpenFolderPanel("选择文件", "Assets/", "");
        }
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("", sevepath);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择.RAW文件夹"))
        {
            rawpath = EditorUtility.OpenFolderPanel("选择文件", "C:\\Users\\Administrator\\Desktop","");
          
           
        }
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("", rawpath);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
       
        GUILayout.Space(10);
        maptextureCount = EditorGUILayout.IntField("地表贴图数量", maptextureCount);
        int count = maptextureCount <1? 1: maptextureCount;
        if(count!= texture2s.Length)
        {
            Texture2D[] oldlist = texture2s;
            texture2s = new Texture2D[count];
            for (int i = 0; i < texture2s.Length; i++)
            {
                if (i<= oldlist.Length-1)
                {
                    texture2s[i] = oldlist[i];
                }
            }
        }
        
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        for (int i=0;i< texture2s.Length;i++)
        {
            if (i == 0)
            {
                GUILayout.BeginHorizontal();
               
            }
            texture2s[i] = EditorGUILayout.ObjectField(texture2s[i], typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100)) as Texture2D;
            GUILayout.Space(10);
            if (i!=0&& (i+1) % 4 == 0)
            {
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("确定"))
        {
          
            XLZTerrainEditor.CreateTerrain(mapName,mapWidth,mapLength,mapHight, highttextureAcc, sevepath, rawpath, texture2s);
            

        }
        GUILayout.Space(120);
        if (GUILayout.Button("取消"))
        {
            instance.Close();
        }
        GUILayout.EndHorizontal();

    }
}