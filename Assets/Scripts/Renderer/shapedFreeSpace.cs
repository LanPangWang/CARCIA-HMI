using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Google.Protobuf.Collections;
using Xviewer;

public class shapedFreeSpace : MonoBehaviour
{
    private SimulationWorld world;
    private TrajectoryPoint center = new TrajectoryPoint();
    private float yaw = 0;

    public bool updateTrigger = false;
    public Vector3[] vertices;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private LinkedList<Tuple<int, Vector3>> verticesList;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        verticesList = new LinkedList<Tuple<int, Vector3>>();
    }

    void UpdateMesh()
    {
        mesh = new Mesh();
        verticesList.Clear();

        mesh.vertices = vertices;
        Vector2[] uvs = Enumerable.Repeat(Vector2.zero, vertices.Length).ToArray();
        Vector3[] normals = Enumerable.Repeat(Vector3.zero, vertices.Length).ToArray();
        mesh.normals = normals;
        mesh.uv = uvs;
        List<int> triArray = new List<int>();

        // foreach (Vector3 vertice in vertices)
        // {
        //     verticesList.AddLast(vertice);
        // }
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesList.AddLast(new Tuple<int, Vector3>(i, vertices[i]));
        }
        LinkedListNode<Tuple<int, Vector3>> current = verticesList.First;
        while (verticesList.Count >= 3)
        {
            LinkedListNode<Tuple<int, Vector3>> next = (current.Next == null) ? verticesList.First : current.Next;
            LinkedListNode<Tuple<int, Vector3>> previous = (current.Previous == null) ? verticesList.Last : current.Previous;
            if (isLeft(current.Value.Item2, next.Value.Item2, previous.Value.Item2))
            {
                LinkedListNode<Tuple<int, Vector3>> ite = (next.Next == null) ? verticesList.First : next.Next;
                bool isEar = true;
                for (int i = 0; i < verticesList.Count - 3; i++)
                {
                    if (isLeft(current.Value.Item2, next.Value.Item2, ite.Value.Item2) 
                        && isLeft(next.Value.Item2, previous.Value.Item2, ite.Value.Item2) 
                        && isLeft(previous.Value.Item2, current.Value.Item2, ite.Value.Item2))
                    {
                        isEar = false;
                        break;
                    }
                    ite = (ite.Next == null) ? verticesList.First : ite.Next;
                }
                if (isEar)
                {
                    triArray.Add(current.Value.Item1);
                    triArray.Add(next.Value.Item1);
                    triArray.Add(previous.Value.Item1);
                    verticesList.Remove(current);
                }
            }
            current = next;
        }
        mesh.triangles = triArray.ToArray();
        meshFilter.mesh = mesh;
    }
    private bool isLeft(Vector3 Pa, Vector3 Pb, Vector3 Pc) //C是否在向量A->B的左边
    {
        return (Pb.x - Pa.x) * (Pc.y - Pa.y) - (Pb.y - Pa.y) * (Pc.x - Pa.x) >= 0;
    }
    // Update is called once per frame
    void Update()
    {
        // world = WebSocketNet.Instance.world;
        // center = WebSocketNet.Instance.center;
        // yaw = WebSocketNet.Instance.yaw;
        // if (world != null)
        // { 
        //     RepeatedField<Point> spacePoints = WorldUtils.GetFreeSpace(world);
        //     vertices = Utils.ApplyArrayToCenter(spacePoints, center);
        //     // UpdateMesh();
        // }
        if (updateTrigger)
        {
            UpdateMesh();
            updateTrigger = false;
        }
    }
}
