using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Google.Protobuf.Collections;
using Xviewer;

public struct EarCutJob : IJob
{
    // public Vector3[] vertices;
    public NativeArray<Vector3> vertices;
    public NativeArray<int> result;

    private bool isLeft(Vector3 Pa, Vector3 Pb, Vector3 Pc) //C是否在向量A->B的左边
    {
        return (Pb.x - Pa.x) * (Pc.y - Pa.y) - (Pb.y - Pa.y) * (Pc.x - Pa.x) >= 0;
    }
    public void Execute()
    {
        int currentIndex = 0;
        LinkedList<Tuple<int, Vector3>> verticesList;
        verticesList = new LinkedList<Tuple<int, Vector3>>();
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
                    // triArray.Add(current.Value.Item1);
                    // triArray.Add(next.Value.Item1);
                    // triArray.Add(previous.Value.Item1);
                    result[currentIndex] = current.Value.Item1;
                    result[currentIndex + 1] = next.Value.Item1;
                    result[currentIndex + 2] = previous.Value.Item1;
                    currentIndex += 3;
                    verticesList.Remove(current);
                }
            }
            current = next;
        }
    }
}
public class shapedFreeSpace : MonoBehaviour
{
    private SimulationWorld world;
    private TrajectoryPoint center = new TrajectoryPoint();
    private float yaw = 0;

    public float frequency;
    public bool updateTrigger = false;
    private bool calculating = false;
    private bool shallUpdate = false;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;

    public Vector3[] vertices;
    private Vector2[] uvs;
    private Vector3[] normals;
    private int[] triArray;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
    }
    private System.Collections.IEnumerator EarCut()
    {
        // yield return new WaitForEndOfFrame();
        uvs = new Vector2[vertices.Length];
        normals = new Vector3[vertices.Length];
        Array.Fill(uvs, Vector2.zero);
        Array.Fill(normals, Vector3.zero);
        uvs[0] = new Vector2(0f, 0.5f);
        uvs[vertices.Length - 1] = new Vector2(0f, 0.5f);

        NativeArray<Vector3> input = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
        NativeArray<int> result = new NativeArray<int>((vertices.Length - 2) * 3, Allocator.Persistent);
        EarCutJob jobData = new EarCutJob{
            vertices = input,
            result = result
        };
        jobData.vertices.CopyFrom(vertices);
		// yield break;

        JobHandle handle = jobData.Schedule();
        yield return new WaitForSecondsRealtime(frequency);
        handle.Complete();

        try
        {
            Destroy(mesh);
        }
        catch {}

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = result.ToArray();
        input.Dispose();
        result.Dispose();

        calculating = false;
        shallUpdate = true;
		yield break;
    }

    void UpdateMesh()
    {
        try
        {
            Destroy(meshFilter.mesh);
        }
        catch {}
        meshFilter.mesh = Instantiate(mesh);
    }
    void Update()
    {
        if (shallUpdate)
        {
            UpdateMesh();
            this.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);
            shallUpdate = false;
        }
        if (!calculating)
        {
            // world = WebSocketNet.Instance.world;
            // center = WebSocketNet.Instance.center;
            // yaw = WebSocketNet.Instance.yaw;
            // if (world != null)
            // { 
            //     RepeatedField<Point> spacePoints = WorldUtils.GetFreeSpace(world);
            //     vertices = Utils.ApplyArrayToCenter(spacePoints, center);            
            //     calculating = true;
            //     StartCoroutine(EarCut());
            // }

            if (updateTrigger)
            {
                calculating = true;
                StartCoroutine(EarCut());
                updateTrigger = false;
            }
        }
    }
}
