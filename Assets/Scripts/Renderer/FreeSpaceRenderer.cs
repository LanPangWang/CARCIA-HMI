using Google.Protobuf.Collections;
using UnityEngine;
using Xviewer;

public class FreeSpaceRenderer : MonoBehaviour
{
    public GameObject FreeSpacePrefab;
    private SimulationWorld world;
    private TrajectoryPoint center = new TrajectoryPoint();
    private float yaw = 0;
    public Vector3[] vertices;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ClearLaneLines();
        world = WebSocketNet.Instance.world;
        center = WebSocketNet.Instance.center;
        yaw = WebSocketNet.Instance.yaw;
        // MakeFreeSpaceTest();
        if (world != null)
        { 
            RepeatedField<Point> spacePoints = WorldUtils.GetFreeSpace(world);
            MakeFreeSpace(spacePoints);
        }
    }

    // 没帧渲染前 清理上一帧的线
    void ClearLaneLines()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void MakeFreeSpace(RepeatedField<Point> points)
    {

        Vector3[] ps = Utils.ApplyArrayToCenter(points, center);
        GameObject newLine = Instantiate(FreeSpacePrefab);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
        newLine.transform.SetParent(gameObject.transform);
        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);
        newLine.transform.localRotation = rotation;
    }    

    void MakeFreeSpaceTest()
    {

        Vector3[] ps = vertices;
        GameObject newLine = Instantiate(FreeSpacePrefab);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
        newLine.transform.SetParent(gameObject.transform);
        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);
        newLine.transform.localRotation = rotation;
    }
}
