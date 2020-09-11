using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class XLZMapMesh
{

    private GameObject mMesh;
    public GameObject MMesh {
        get { return mMesh; }
    }

    private Material mMaterial;
    private Texture2D mHeightImage;
    private Vector2 size;//长度和宽度
    public  float minHeight = -10;//最小高度
    public  float maxHeight = 10;//最大高度
    private Vector2 segment;//长度的段数和宽度的段数
    private float unitH;//最小高度和最大高度只差，值为正

    private Vector3[] vertexes;//顶点数
    private Vector2 uvs;//uvs坐标
    private int[] triangles;//三角形索引
    private string Name;
    private string SavePath="";

    public XLZMapMesh(string name, Texture2D hMap,string savePath)
    {
        Name = name;
        mHeightImage = hMap;
        if (savePath!=null)
        {
            SavePath = savePath;
        }
        
    }
    public void CreatMesh(float width, float height, uint segmentX, uint segmentY, int min, int max)
    {
        size = new Vector2(width, height);
        maxHeight = max;
        minHeight = min;
        unitH = maxHeight - minHeight;
        segment = new Vector2(segmentX, segmentY);

        mMesh = new GameObject();
        mMesh.name = Name;

        computeVertexes();
        DrawMesh();
    }
    
    private void computeVertexes()
    {
        int sum = Mathf.FloorToInt((segment.x + 1) * (segment.y + 1));//顶点总数
        float w = size.x / segment.x;//每一段的长度
        float h = size.y / segment.y;

        GetTriangles();

        int index = 0;
        vertexes = new Vector3[sum];
        for (int i = 0; i < segment.y + 1; i++)
        {
            for (int j = 0; j < segment.x + 1; j++)
            {
                vertexes[index] = new Vector3(j * w, GetHeight(mHeightImage, new Vector2(i / segment.x, j / segment.y)) * unitH, i * h);
                if (i== segment.y)
                {
                    vertexes[index] = new Vector3(j * w, vertexes[index- (int)segment.y].y, i * h);
                }
                if (j == segment.x&&i!=0)
                {
                    vertexes[index] = new Vector3(j * w, vertexes[index - 1].y, i * h);
                }

                

                index++;
            }
        }
    }

    private void DrawMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = Name;
        mMesh.AddComponent<MeshFilter>();//网格
        mMesh.AddComponent<MeshRenderer>();//网格渲染器

        mMaterial = new Material(Shader.Find("MTE/Legacy/4 Textures/Diffuse"));//材质

        mMesh.GetComponent<Renderer>().material = mMaterial;

        /*设置mesh*/
        mesh.Clear();//更新
        mesh.vertices = vertexes;
        //mesh.uv 
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mMesh.GetComponent<MeshFilter>().mesh= mesh;
        //SaveAll(mesh, mMaterial);
    }

    private void SaveAll(Mesh mesh, Material material)
    {
        string localPath = SavePath.Substring(SavePath.IndexOf("Assets"));
        localPath += "/" + Name;
        Directory.CreateDirectory(localPath);
        Object prefabObj = PrefabUtility.SaveAsPrefabAssetAndConnect(mMesh,localPath + "/" + Name + ".prefab",InteractionMode.AutomatedAction);
       

        AssetDatabase.CreateAsset(mMaterial, localPath + "/" + Name + ".mat");
        AssetDatabase.CreateAsset(mesh, localPath + "/" + Name + ".asset");
    }
    private int[] GetTriangles()
    {
        int sum = Mathf.FloorToInt(segment.x * segment.y * 6);//三角形顶点总数
        triangles = new int[sum];
        uint index = 0;
        for (int i = 0; i < segment.y; i++)
        {
            for (int j = 0; j < segment.x; j++)
            {
                int role = Mathf.FloorToInt(segment.x) + 1;
                int self = j + (i * role);
                int next = j + ((i + 1) * role);
                //顺时针
                triangles[index] = self;
                triangles[index + 1] = next + 1;
                triangles[index + 2] = self + 1;
                triangles[index + 3] = self;
                triangles[index + 4] = next;
                triangles[index + 5] = next + 1;
                index += 6;
            }
        }
        return triangles;
    }
    private static float GetHeight(Texture2D texture, Vector2 uv)
    {
        if (texture != null)
        {
            Color c = GetColor(texture, uv);

            return c.r - 0.5f;
        }
        else
        {
            return 0.5f;
        }
    }
    private static Color GetColor(Texture2D texture, Vector2 uv)
    {

        Color color = texture.GetPixel(Mathf.FloorToInt(texture.width * uv.x), Mathf.FloorToInt(texture.height * uv.y));
        return color;
    }
}
