using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Google.Protobuf.Collections;
using Xviewer;

// [BurstCompile]
public struct EarCutJob : IJob
{
    // public Vector3[] vertices;
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> verticesResult;
    public NativeArray<int> result;
    public NativeArray<int> options;
    public NativeArray<float> thresholds;

    private bool isLeft(Vector3 Pa, Vector3 Pb, Vector3 Pc) //C是否在向量A->B的左边
    {
        return (Pb.x - Pa.x) * (Pc.y - Pa.y) - (Pb.y - Pa.y) * (Pc.x - Pa.x) >= 0;
    }

    private float getDis(Vector3 Pa, Vector3 Pb)
    {
        return (Pa - Pb).magnitude;
    }

    public void Execute()
    {
        
        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        long startTimestamp = startTime.ToUnixTimeMilliseconds();

        int currentIndex = 0;
        LinkedList<Tuple<int, Vector3>> verticesList;
        verticesList = new LinkedList<Tuple<int, Vector3>>();
        verticesList.AddLast(new Tuple<int, Vector3>(0, vertices[0]));
        verticesResult[0] = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            if (getDis(verticesList.Last.Value.Item2, vertices[i]) > thresholds[0])
            {
                int thisIndex = verticesList.Count;
                verticesList.AddLast(new Tuple<int, Vector3>(thisIndex, vertices[i]));
                verticesResult[thisIndex] = vertices[i];
            }
        }
        options[2] = verticesList.Count;
        LinkedListNode<Tuple<int, Vector3>> current = verticesList.First;
        while (verticesList.Count >= 3)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            long currentTimestamp = currentTime.ToUnixTimeMilliseconds();
            if ((int)(currentTimestamp - startTimestamp) > options[0])
            {
                options[1] = 0;
                return;
            }

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
        options[1] = 1;
    }
}

public class shapedFreeSpace : MonoBehaviour
{
    private SimulationWorld world;
    private TrajectoryPoint center = new TrajectoryPoint();
    private float yaw = 0;

    public int timeout;
    public bool updateTrigger = false;
    private bool calculating = false;
    private bool shallUpdate = false;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    public float[] freeSpaceThresholds;
    public Vector3[] vertices;
    public Vector3[] vertices2;
    private Vector2[] uvs;
    private Vector3[] normals;
    private int[] triArray;
    private int[] optionArray;
    private NativeArray<Vector3> jobInput;
    private NativeArray<Vector3> jobOutput;
    private NativeArray<int> jobResult;
    private NativeArray<int> jobOptions;
    private NativeArray<float> jobThresholds;
    private EarCutJob jobData;
    private JobHandle jobHandle;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        meshFilter = this.GetComponent<MeshFilter>();
        mesh = new Mesh();
        optionArray = new int[3];
        optionArray[0] = timeout;
        optionArray[1] = 0;
        optionArray[2] = 0;
    }

    private void EarCut()
    {
        // try
        // {
        //     jobHandle.Complete();    
        //     jobInput.Dispose();
        //     jobResult.Dispose();
        // }
        // catch { }
        // yield return new WaitForEndOfFrame();
        jobInput = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
        jobOutput = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
        jobResult = new NativeArray<int>((vertices.Length - 2) * 3, Allocator.Persistent);
        jobOptions = new NativeArray<int>(3, Allocator.Persistent);
        jobThresholds = new NativeArray<float>(2, Allocator.Persistent);
        jobData = new EarCutJob{
            vertices = jobInput,
            verticesResult = jobOutput,
            result = jobResult,
            options = jobOptions,
            thresholds = jobThresholds
        };
        jobData.vertices.CopyFrom(vertices);
        jobData.options.CopyFrom(optionArray);
        jobData.thresholds.CopyFrom(freeSpaceThresholds);
		// yield break;

        jobHandle = jobData.Schedule();
        // while(!handle.IsCompleted)
        // {
        //     if ((UnityEngine.Time.realtimeSinceStartup - startTime) > frequency)
        //     {
        //         break;
        //     }
        //     yield return new WaitForSecondsRealtime(UnityEngine.Time.unscaledDeltaTime);
        // }
        // if (handle.IsCompleted)
        // {
        //     handle.Complete();
        // }
        // else
        // {
        //     handle.Complete();
        //     input.Dispose();
        //     result.Dispose();
        //     Debug.LogWarning("earcutting took too long");
        //     calculating = false;
        //     yield break;
        // }
    }
    void UpdateFreeSpaceMesh()
    {
        jobHandle.Complete();
        if (jobOptions[1] == 0)
        {
            Debug.LogWarning("IJob time out");
            jobInput.Dispose();
            jobResult.Dispose();
            jobOptions.Dispose();
            return;
        }

        try
        {
            Destroy(mesh);
        }
        catch {}

        triArray = jobResult.ToArray();
        int verticesLength = jobOptions[2];
        vertices2 = new Vector3[verticesLength];
        Array.Copy(jobOutput.ToArray(), vertices2, verticesLength);

        uvs = new Vector2[verticesLength];
        normals = new Vector3[verticesLength];
        Array.Fill(uvs, Vector2.zero);
        Array.Fill(normals, Vector3.zero);
        uvs[0] = new Vector2(0f, 0.5f);
        uvs[verticesLength - 1] = new Vector2(0f, 0.5f);

        mesh = new Mesh();
        mesh.vertices = vertices2;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triArray;
        jobInput.Dispose();
        jobOutput.Dispose();
        jobResult.Dispose();
        jobOptions.Dispose();
        jobThresholds.Dispose();

        meshFilter.mesh = Instantiate(mesh);
        this.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);

        // shallUpdate = true;
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

    private float getDis(Vector3 Pa, Vector3 Pb)
    {
        return (Pa - Pb).magnitude;
    }
    void OnDestroy() {
        // try
        // {
        //     jobHandle.Complete();    
        //     jobInput.Dispose();
        //     jobResult.Dispose();
        // }
        // catch { }
    }
    void Update()
    {
        // if (shallUpdate)
        // {
        //     UpdateMesh();
        //     this.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);
        //     shallUpdate = false;
        // }
        // Debug.Log($"timestamp: {timestamp}");
        if (!calculating)
        {
            world = WebSocketNet.Instance.world;
            center = WebSocketNet.Instance.center;
            yaw = WebSocketNet.Instance.yaw;
            if (world != null)
            { 
                RepeatedField<Point> spacePoints = WorldUtils.GetFreeSpace(world);
                vertices = Utils.ApplyArrayToCenter(spacePoints, center);   
                // PointsFilter(ref vertices2, ref vertices);
                if (vertices.Length > 2)
                {
                    EarCut();       
                    calculating = true;
                }
            }

            if (updateTrigger)
            {
                // PointsFilter(ref vertices2, ref vertices);
                EarCut();
                calculating = true;
                updateTrigger = false;
            }
        }
        else
        {
            if(jobHandle.IsCompleted)
            {
                UpdateFreeSpaceMesh();
                calculating = false;
            }
        }
    }
}
