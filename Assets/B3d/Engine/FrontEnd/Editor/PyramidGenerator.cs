using UnityEngine;
using UnityEditor;
using System.Collections;

public class PyramidGenerator : ScriptableWizard
{

    public int size = 4;
    public bool flat = true;

         
    Mesh GenerateMesh(Mesh oldMesh = null)
    {
        if (oldMesh == null)
        {
            oldMesh = new Mesh();
        }
        int vertextCount = size + 2;
        int triangleCount = size * 2;

        Vector3[] vertices = new Vector3[vertextCount];
        int[] triangles = new int[triangleCount * 3];

        vertices[0] = Vector3.up;
        vertices[1] = Vector3.zero;

        float alpha = Mathf.PI * 2 / size;
        float omega = alpha * 0.5f;
        for (int i = 0; i < size; i++)
        {
            vertices[i + 2] = new Vector3(Mathf.Cos(i * alpha + omega), 0, Mathf.Sin(i * alpha + omega));
            Debug.Log(i + 2);
            int oldIndex = i == 0 ? vertextCount-1 : i + 1; 
            int offset = i * 6;
            triangles[offset + 0] = i + 2;
            triangles[offset + 1] = oldIndex;
            triangles[offset + 2] = 0;
            triangles[offset + 3] = i + 2;
            triangles[offset + 4] = 1;
            triangles[offset + 5] = oldIndex;
        }
        
        oldMesh.vertices = vertices;
        oldMesh.triangles = triangles;
        oldMesh.RecalculateNormals();
        oldMesh.RecalculateBounds();

        return oldMesh;
    }

    Mesh GenerateMeshFlat(Mesh oldMesh = null)
    {
        if (oldMesh == null)
        {
            oldMesh = new Mesh();
        }
        int vertextCount = size*4 + 1;
        int triangleCount = size * 2;

        Vector3[] vertices = new Vector3[vertextCount];
        Vector2[] uvs = new Vector2[vertextCount];
        int[] triangles = new int[triangleCount * 3];

        vertices[0] = Vector3.zero;

        float alpha = Mathf.PI * 2 / size;
        float omega = alpha * 0.5f;

        for (int i = 0; i < size; i++)
        {
            int index = i * 4 + 1;
            vertices[index] = new Vector3(Mathf.Cos(i * alpha + omega), 0, Mathf.Sin(i * alpha + omega));
            vertices[index + 1] = Vector3.up;
            vertices[index + 2] = vertices[index];
            vertices[index + 3] = vertices[index];
            uvs[index] = new Vector2(0, 1);
            uvs[index + 1] = new Vector2(0.5f, 0);
            uvs[index + 2] = new Vector2(1, 1);
            uvs[index + 3] = new Vector2(0, 1);
            int oldIndex = i == 0 ? vertextCount - 1 : index - 1;
            int offset = i * 6;
            triangles[offset + 0] = index;
            triangles[offset + 1] = oldIndex;
            triangles[offset + 2] = index + 1;
            triangles[offset + 3] = index + 2;
            triangles[offset + 4] = 0;
            triangles[offset + 5] = oldIndex - 1;
        }

        oldMesh.vertices = vertices;
        oldMesh.triangles = triangles;
        oldMesh.RecalculateNormals();
        oldMesh.RecalculateBounds();

        return oldMesh;
    }

    [MenuItem("Assets/Create/Pyramid")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("PyramidGenerator", typeof(PyramidGenerator));
    }

    void OnWizardCreate()
    {
        Mesh result = flat ? GenerateMeshFlat() : GenerateMesh();
        string name = "pyramid" + (flat ? "Flat" : "") + size+ ".asset";
        AssetDatabase.CreateAsset(result, "Assets/Meshes/" + name);
    }
}