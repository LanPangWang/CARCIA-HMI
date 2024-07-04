using Google.Protobuf.Collections;
using UnityEngine;
using Xviewer;

public class GuideLineRenderer : MonoBehaviour
{
    public GameObject GuideLineBase;
    private SimulationWorld world;
    public TrajectoryPoint center = new TrajectoryPoint();
    public float yaw = 0;

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
        if (world != null)
        {
            TrajectoryStamped guideLine = WorldUtils.GetGuideLine(world);
            MakeGuideLine(guideLine);
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

    void MakeGuideLine(TrajectoryStamped guideLine)
    {
        RepeatedField<TrajectoryPoint> points = guideLine.TrajPoints;
        Vector3[] ps = Utils.ApplyArrayToCenter(points, center);
        GameObject newLine = Instantiate(GuideLineBase);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
        newLine.transform.SetParent(gameObject.transform);
        // 确保子对象的局部旋转为零
        newLine.transform.localRotation = UnityEngine.Quaternion.identity;
        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, 0, -yaw * Mathf.Rad2Deg + 90);
        newLine.transform.localRotation = rotation;
    }
}
