using UnityEngine;

public class PlaneFromPoints : MonoBehaviour
{
    public Mesh mesh;
    private Vector3[] points = new Vector3[4];

    void Awake()
    {
        // Create a new mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetPoints(Vector3[] points)
    {
        if (points.Length != 4)
        {
            Debug.LogError("Exactly 4 points are required to create a plane.");
            return;
        }

        this.points = points;
        CreatePlane();
    }

    void CreatePlane()
    {
        // Define the vertices based on the provided points
        Vector3[] vertices = new Vector3[4];
        vertices[0] = points[0];
        vertices[1] = points[1];
        vertices[2] = points[2];
        vertices[3] = points[3];

        // Define the triangles that make up the plane
        int[] triangles = new int[]
        {
            0, 2, 1, // First triangle
            0, 3, 2,  // Second triangle
        };

        // Optionally, define the UV coordinates for texturing
        Vector2[] uv = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
        };

        // Assign vertices, triangles, and UVs to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();// Load the texture from the Resources folder
    }
}