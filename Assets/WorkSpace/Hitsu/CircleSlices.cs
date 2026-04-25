using UnityEngine;

public class CircleSlices : MonoBehaviour
{
    public int sliceCount = 8;
    public float radius = 1f;

    void Start()
    {
        for (int i = 0; i < sliceCount; i++)
        {
            CreateSlice(i);
        }
    }

    void CreateSlice(int index)
    {
        GameObject slice = new GameObject("Slice_" + index);
        slice.transform.parent = transform;
        slice.transform.localPosition = Vector3.zero;

        MeshFilter mf = slice.AddComponent<MeshFilter>();
        MeshRenderer mr = slice.AddComponent<MeshRenderer>();

        mf.mesh = CreateMesh(index);
        mr.material = new Material(Shader.Find("Sprites/Default")); // 可換成你的材質
    }

    Mesh CreateMesh(int index)
    {
        Mesh mesh = new Mesh();

        float angleStep = 360f / sliceCount;
        float angleA = Mathf.Deg2Rad * (index * angleStep);
        float angleB = Mathf.Deg2Rad * ((index + 1) * angleStep);

        Vector3 center = Vector3.zero;
        Vector3 pointA = new Vector3(Mathf.Cos(angleA), Mathf.Sin(angleA), 0) * radius;
        Vector3 pointB = new Vector3(Mathf.Cos(angleB), Mathf.Sin(angleB), 0) * radius;

        mesh.vertices = new Vector3[] { center, pointA, pointB };
        mesh.triangles = new int[] { 0, 1, 2 };

        mesh.RecalculateNormals();
        return mesh;
    }
}