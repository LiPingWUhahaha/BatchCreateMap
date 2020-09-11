using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditorInternal;
using System.IO;
using System.Security.AccessControl;
using UnityEditor.Animations;

public class ExportServerCFG
{
    public static string SelectObjName;

    [MenuItem("GameObject/导出地图服务器配置表", false, -5)]
    public static void CreateServerCFG()
    {
        GameObject obj = Selection.activeGameObject;
        SelectObjName = obj.name;
        ///Debug.LogError("selectionObj:"+obj.name);
        /// string path = EditorUtility.OpenFolderPanel("选择保存的文件夹", "Assets/", "");
        ExportCFGInputWindow input = EditorWindow.GetWindow<ExportCFGInputWindow>(false, "导出配置表", true);
        input.Show();
    }
    [MenuItem("GameObject/设置父节点", false, -6)]
    public static void SetParent()
    {
        GameObject obj = Selection.activeGameObject;
        SelectObjName = obj.name;
        ///Debug.LogError("selectionObj:"+obj.name);
        /// string path = EditorUtility.OpenFolderPanel("选择保存的文件夹", "Assets/", "");
        SetParentInputWindow input = EditorWindow.GetWindow<SetParentInputWindow>(false, "设置父节点", true);
        input.Show();
    }
    public static void RayCheck(Vector3 dot1, float height, float width, string savePath, LayerMask selectLayer)
    {
        //基本尺寸
        int wCount = Mathf.CeilToInt(width / 0.5f);
        int hCount = Mathf.CeilToInt(height / 0.5f);
        //基础空白格子
        List<List<int>> mapInfo = new List<List<int>>();
        for (int i = 0; i < hCount; i++)
        {
            List<int> wMap = new List<int>();
            for (int j = 0; j < wCount; j++)
            {
                wMap.Add(0);
                Ray ray = new Ray(new Vector3(dot1.x + j * 0.5f + 0.25f, 100, dot1.z + i * 0.5f + 0.25f), Vector3.down);
                RaycastHit raycastHit;
                bool ishave = Physics.Raycast(ray, out raycastHit, Mathf.Infinity, 1 << selectLayer);
                if (ishave)
                {
                    wMap[j] = 1;
                }

            }
            mapInfo.Add(wMap);
        }

        DebugDarw(mapInfo, dot1);
        SaveFile(height, width, mapInfo, dot1, savePath);
    }
    public static void DebugDarw(List<List<int>> mapInfo, Vector3 dot1)
    {
        string kkk = "";
        for (int i = 0; i < mapInfo.Count; i++)
        {
            for (int j = 0; j < mapInfo[i].Count; j++)
            {
                if (mapInfo[i][j] == 0)
                {
                    kkk += 0 + ",";
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f), new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), Color.green);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f + 0.5f), Color.green);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f), Color.green);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f), Color.green);
                }
                else
                {
                    kkk += 1 + ",";
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f), new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), Color.red);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f + 0.5f), Color.red);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f + 0.5f, 0, dot1.z + i * 0.5f), Color.red);
                    Debug.DrawLine(new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f + 0.5f), new Vector3(dot1.x + j * 0.5f, 0, dot1.z + i * 0.5f), Color.red);
                }


            }
        }
        Debug.LogError("格子：" + kkk);
    }
    static void SaveFile(float height, float width, List<List<int>> mapInfo, Vector3 dot1, string savePath)
    {
        string mapInfoPath = savePath + "/" + SelectObjName + ".txt";
        string mapCollisionPath = savePath + "/" + SelectObjName + ".xlzmp";
        int arrayCount = mapInfo.Count * mapInfo[0].Count;
        int[] array = new int[arrayCount];
        for (int i = 0; i < mapInfo.Count; i++)
        {
            for (int j = 0; j < mapInfo[i].Count; j++)
            {
                // Debug.LogError(mapInfo[i][j]);
                array[i * mapInfo[i].Count + j] = mapInfo[i][j];
            }
        }
        //byte[] data = new byte[array.Length * sizeof(int)];
        //Buffer.BlockCopy(array, 0, data, 0, array.Length);
        string llll = "";
        foreach (int i in array)
        {
            llll += i + ",";
        }
        Debug.LogError(llll);
        byte[] data = IntToBytes(array, 0);
        int[] nts = BytesToInt(data, 0);
        string testt = "";
        foreach (int i in nts)
        {
            testt += i + ",";
        }
        Debug.LogError(testt);
        FileStream filestr = File.Create(mapCollisionPath);
        filestr.Write(data, 0, data.Length);
        filestr.Flush(); //流会缓冲，此行代码指示流不要缓冲数据，立即写入到文件。
        filestr.Close(); //关闭流并释放所有资源，同时将缓冲区的没有写入的数据，写入然后再关闭。
        filestr.Dispose();//释放流所占用的资源

        string infoString = SelectObjName + "\n";
        infoString += "height=" + height + " width=" + width + "\n";
        infoString += "row=" + mapInfo.Count + " list=" + mapInfo[0].Count + "\n";
        infoString += dot1 + "\n";
        File.WriteAllText(mapInfoPath, infoString);
        Debug.LogError("完成");
    }
    /// <summary>  
    /// byte数组转int数组  
    /// </summary>  
    /// <param name="src">源byte数组</param>  
    /// <param name="offset">起始位置</param>  
    /// <returns></returns>  
    public static int[] BytesToInt(byte[] src, int offset)
    {
        int[] values = new int[src.Length / 4];
        for (int i = 0; i < src.Length / 4; i++)
        {
            int value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8)
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24));
            values[i] = value;

            offset += 4;
        }
        return values;
    }
    /// <summary>  
    /// int数组转byte数组  
    /// </summary>  
    /// <param name="src">源int数组</param> 
    /// <param name="offset">起始位置,一般为0</param>  
    /// <returns>values</returns>  
    public static byte[] IntToBytes(int[] src, int offset)
    {
        byte[] values = new byte[src.Length * 4];
        for (int i = 0; i < src.Length; i++)
        {

            values[offset + 3] = (byte)((src[i] >> 24) & 0xFF);
            values[offset + 2] = (byte)((src[i] >> 16) & 0xFF);
            values[offset + 1] = (byte)((src[i] >> 8) & 0xFF);
            values[offset] = (byte)(src[i] & 0xFF);
            offset += 4;
        }
        return values;
    }

    [MenuItem("GameObject/导入动画资源", false, -5)]
    public static void InputAnimationRes()
    {
        string folderPath = EditorUtility.OpenFolderPanel("选择保存动画的文件夹文件夹", "Assets/", "");
        string[] paths = Directory.GetFiles(folderPath);
        int indexStar = paths[0].IndexOf("\\");

        string enemyName = paths[0].Substring(indexStar + 1, paths[0].IndexOf(".FBX") - indexStar - 1);

        //创建文件夹
        string creatName = "Assets/Res/Animations/" + enemyName;
        if (!Directory.Exists(creatName))
        {
            DirectoryInfo folder = Directory.CreateDirectory(creatName);
        }
        //复制FBX
        for (int i = 0; i < paths.Length; i++)
        {
            string desPath = "";
            string fileName = paths[i].Substring(indexStar + 1);
            if (i == 0)
            {
                desPath = "Assets/Res/Model/GameCharacter/" + fileName;
                Debug.LogError(desPath);
            }
            else
            {
                desPath = creatName + "/" + fileName;
            }
            File.Copy(paths[i], desPath);
        }
        //复制纹理
        string imagSrcPath = folderPath + "/Textures/" + enemyName + ".png";
        string imagDesPath = "Assets/Res/Texture/Model/" + enemyName + ".png";
        File.Copy(imagSrcPath, imagDesPath);
        //创建材质球
        Material material = new Material(Shader.Find("Standard"));
        Texture2D texture = AssetDatabase.LoadAssetAtPath(imagDesPath, typeof(Texture2D)) as Texture2D;
        material.SetTexture("_MainTex", texture);
        material.SetTexture("_EmissionMap", texture);
        material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f, 1));
        string materialPath = "Assets/Res/Material/" + enemyName + ".mat";
        AssetDatabase.CreateAsset(material, materialPath);
        //创建动画控制器
        //string animatorPath= "Assets/Res/Animator/" + enemyName+ ".controller";
        //UnityEditor.Animations.AnimatorController animator = new UnityEditor.Animations.AnimatorController();
        //string[] cppaths = Directory.GetFiles(creatName);
        //Dictionary<int, string> clipPasths = new Dictionary<int, string>();
        //for (int i = 0; i < cppaths.Length; i++)
        //{
        //    if (cppaths[i].IndexOf(".meta") < 0)
        //    {
        //        int stindex = cppaths[i].IndexOf("_");
        //        clipPasths[i] = cppaths[i].Substring(stindex + 1, cppaths[i].IndexOf(".FBX") - stindex - 1);
        //        cppaths[i] = cppaths[i].Substring(cppaths[i].IndexOf("Assets/"));
        //    }

        //}
        //List<AnimationClip> ClipList = new List<AnimationClip>();
        //bool isHaveCbl = false;
        //foreach (KeyValuePair<int, string> i in clipPasths)
        //{
        //    AnimationClip clip = AssetDatabase.LoadAssetAtPath(cppaths[i.Key], typeof(AnimationClip)) as AnimationClip;
        //    clip.name = i.Value + 1;
        //    if (i.Value.Equals("Idle") || i.Value.Equals("Cbl"))
        //    {
        //        if (i.Value.Equals("Cbl"))
        //        {
        //            ClipList.Insert(0, clip);
        //            isHaveCbl = true;
        //        }
        //        else
        //        {
        //            if (!isHaveCbl)
        //            {
        //                ClipList.Insert(0, clip);
        //            }
        //            else
        //            {
        //                ClipList.Add(clip);
        //            }
        //        }

        //    }
        //    else
        //    {
        //        ClipList.Add(clip);
        //    }
        //}
        //foreach (AnimationClip i in ClipList)
        //{
        //    animator.AddMotion(i);
        //}
        //CreateAnimatorParameter(animator);
        // AssetDatabase.CreateAsset(animator, animatorPath);

        AssetDatabase.Refresh();
    }
    public static void CreateAnimatorParameter(UnityEditor.Animations.AnimatorController obj)
    {

        UnityEngine.AnimatorControllerParameter[] aCParameters = new UnityEngine.AnimatorControllerParameter[8];
        aCParameters[0] = new UnityEngine.AnimatorControllerParameter
        {
            name = "Hit_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[1] = new UnityEngine.AnimatorControllerParameter
        {
            name = "Run_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[2] = new UnityEngine.AnimatorControllerParameter
        {
            name = "Idel_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[3] = new UnityEngine.AnimatorControllerParameter
        {
            name = "Walk_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[4] = new UnityEngine.AnimatorControllerParameter
        {
            name = "LAtk_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[5] = new UnityEngine.AnimatorControllerParameter
        {
            name = "RAtk_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[6] = new UnityEngine.AnimatorControllerParameter
        {
            name = "AAtk_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };
        aCParameters[7] = new UnityEngine.AnimatorControllerParameter
        {
            name = "Die_1",
            type = UnityEngine.AnimatorControllerParameterType.Trigger,
        };

        obj.parameters = aCParameters;
    }

    public static void SetParentByPath(string path,GameObject childGo)
    {
        Transform parentTF = GameObject.Find(path).transform;
        childGo.transform.parent = parentTF;
        AssetDatabase.Refresh();
        childGo.SetActive(false);
        childGo.SetActive(true);
    }
}
public class SetParentInputWindow : EditorWindow
{
    public static SetParentInputWindow instance;
   
    void OnEnable()
    {
        instance = this;
       
       
       
       
    }
    void OnDisable()
    {
        instance = null;
       
    }

    public string ParentPath;
    
   
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        //标签
        GUILayout.BeginHorizontal();
        GUILayout.Box("输入父节点路径参数", TitleBoxStyle(), GUILayout.Height(60), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        //主体输入

        ParentPath = EditorGUILayout.TextField("输入父节点路径", ParentPath);


        GUILayout.Space(120);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("确定", GUILayout.Height(30)))
        {
            ExportServerCFG.SetParentByPath("020/植物/hebianshu3", Selection.activeGameObject);
            instance.Close();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
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

}
public class ExportCFGInputWindow : EditorWindow
{
    public static ExportCFGInputWindow instance;
    Vector3 dot1, dot2, dot3, dot4;
    Color mid1Cor, mid2Cor, mid3Cor, mid4Cor;
    int ExportDir = 0;
    float EditorSpeed = 0.05f;
    void OnEnable()
    {
        instance = this;
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        GameObject selectOBJ = Selection.activeGameObject;
        Vector3 selectPos = selectOBJ.transform.position;
        dot1 = new Vector3(selectPos.x, 0, selectPos.z);
        dot2 = new Vector3(selectPos.x, 0, selectPos.z + 10);
        dot3 = new Vector3(selectPos.x + 10, 0, selectPos.z + 10);
        dot4 = new Vector3(selectPos.x + 10, 0, 0);
        mid1Cor = Color.green;
        mid2Cor = Color.green;
        mid3Cor = Color.green;
        mid4Cor = Color.green;
    }
    void OnDisable()
    {
        instance = null;
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public string CFGSavePath;
    public LayerMask selectLayer = new LayerMask();
    int layerIndex = 0;
    Vector3 m1, m2, m3, m4;
    void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = Color.green;
        Handles.DrawAAPolyLine(2, dot1, dot2, dot3, dot4, dot1);

        #region 鼠标控制
        float m1px = Handles.FreeMoveHandle(new Vector3((dot1.x + dot2.x) * 0.5f, 0, (dot1.z + dot2.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, Vector3.zero, Handles.DotCap).x;
        dot1 = new Vector3(m1px, 0, dot1.z);
        dot2 = new Vector3(m1px, 0, dot2.z);

        float m2px = Handles.FreeMoveHandle(new Vector3((dot2.x + dot3.x) * 0.5f, 0, (dot2.z + dot3.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, Vector3.zero, Handles.DotCap).z;
        dot2 = new Vector3(dot2.x, 0, m2px);
        dot3 = new Vector3(dot3.x, 0, m2px);

        float m3px = Handles.FreeMoveHandle(new Vector3((dot3.x + dot4.x) * 0.5f, 0, (dot3.z + dot4.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, Vector3.zero, Handles.DotCap).x;
        dot3 = new Vector3(m3px, 0, dot3.z);
        dot4 = new Vector3(m3px, 0, dot4.z);

        float m4px = Handles.FreeMoveHandle(new Vector3((dot4.x + dot1.x) * 0.5f, 0, (dot4.z + dot1.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, Vector3.zero, Handles.DotCap).z;
        dot4 = new Vector3(dot4.x, 0, m4px);
        dot1 = new Vector3(dot1.x, 0, m4px);
        #endregion

        #region 键盘控制
        Handles.color = mid1Cor;
        Handles.DotHandleCap(0, new Vector3((dot1.x + dot2.x) * 0.5f, 0, (dot1.z + dot2.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, EventType.Repaint);
        Handles.color = mid2Cor;
        Handles.DotHandleCap(0, new Vector3((dot2.x + dot3.x) * 0.5f, 0, (dot2.z + dot3.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, EventType.Repaint);
        Handles.color = mid3Cor;
        Handles.DotHandleCap(0, new Vector3((dot3.x + dot4.x) * 0.5f, 0, (dot3.z + dot4.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, EventType.Repaint);
        Handles.color = mid4Cor;
        Handles.DotHandleCap(0, new Vector3((dot4.x + dot1.x) * 0.5f, 0, (dot4.z + dot1.z) * 0.5f), Quaternion.identity, 0.005f * sceneView.size, EventType.Repaint);
        CheckKey();
        #endregion

        //sceneView.Repaint();


    }

    void CheckKey()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {

            if (e.keyCode == KeyCode.Keypad4)
            {
                ExportDir = 4;
                mid1Cor = Color.red;
                mid2Cor = Color.green;
                mid3Cor = Color.green;
                mid4Cor = Color.green;
            }
            if (e.keyCode == KeyCode.Keypad6)
            {
                ExportDir = 6;
                mid1Cor = Color.green;
                mid2Cor = Color.green;
                mid3Cor = Color.red;
                mid4Cor = Color.green;
            }
            if (e.keyCode == KeyCode.Keypad2)
            {
                ExportDir = 2;
                mid1Cor = Color.green;
                mid2Cor = Color.green;
                mid3Cor = Color.green;
                mid4Cor = Color.red;
            }
            if (e.keyCode == KeyCode.Keypad8)
            {
                ExportDir = 8;
                mid1Cor = Color.green;
                mid2Cor = Color.red;
                mid3Cor = Color.green;
                mid4Cor = Color.green;
            }
            if (e.keyCode == KeyCode.KeypadPlus)
            {
                //ExportDir = 4;
                Plus(ExportDir);
            }
            if (e.keyCode == KeyCode.KeypadMinus)
            {
                //ExportDir = 4;
                Minus(ExportDir);
            }
        }

    }
    void Plus(int dir)
    {

        if (dir == 4)
        {
            dot1.x -= EditorSpeed;
            dot2.x -= EditorSpeed;
        }
        else if (dir == 6)
        {
            dot3.x += EditorSpeed;
            dot4.x += EditorSpeed;
        }
        else if (dir == 2)
        {
            dot1.z -= EditorSpeed;
            dot4.z -= EditorSpeed;
        }
        else if (dir == 8)
        {
            dot2.z += EditorSpeed;
            dot3.z += EditorSpeed;
        }
        else
        {
            return;
        }
    }
    void Minus(int dir)
    {

        if (dir == 4)
        {
            dot1.x += EditorSpeed;
            dot2.x += EditorSpeed;
        }
        else if (dir == 6)
        {
            dot3.x -= EditorSpeed;
            dot4.x -= EditorSpeed;
        }
        else if (dir == 2)
        {
            dot1.z += EditorSpeed;
            dot4.z += EditorSpeed;
        }
        else if (dir == 8)
        {
            dot2.z -= EditorSpeed;
            dot3.z -= EditorSpeed;
        }
        else
        {
            return;
        }
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        //标签
        GUILayout.BeginHorizontal();
        GUILayout.Box("输入相应的参数", TitleBoxStyle(), GUILayout.Height(60), GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        //主体输入

        if (GUILayout.Button("选择文件保存目录", GUILayout.Height(30), GUILayout.Width(180)))
        {
            CFGSavePath = EditorUtility.OpenFolderPanel("选择保存的文件夹", "Assets/", "");
            //Debug.LogError("CFGSavePath:"+ CFGSavePath);
        }

        GUILayout.BeginHorizontal();
        GUIStyle boxStyle = new GUIStyle();
        boxStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Box("选择层级", boxStyle, GUILayout.Height(30), GUILayout.Width(80));
        boxStyle.normal.background = Texture2D.whiteTexture;


        layerIndex = EditorGUILayout.Popup(layerIndex, UnityEditorInternal.InternalEditorUtility.layers, boxStyle, GUILayout.Height(30), GUILayout.Width(180));
        selectLayer = LayerMask.NameToLayer(InternalEditorUtility.layers[layerIndex]);

        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        boxStyle = new GUIStyle();
        boxStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Box("当前移动速度" + EditorSpeed, boxStyle, GUILayout.Height(30), GUILayout.Width(80));
        GUILayout.Space(10);
        EditorSpeed = GUILayout.HorizontalSlider(EditorSpeed, 0.01f, 1);
        GUILayout.EndHorizontal();
        //底部按钮
        GUILayout.Space(120);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("确定", GUILayout.Height(30)))
        {
            ExportServerCFG.RayCheck(dot1, dot2.z - dot1.z, dot4.x - dot1.x, CFGSavePath, selectLayer);
            instance.Close();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("取消", GUILayout.Height(30)))
        {
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

}
