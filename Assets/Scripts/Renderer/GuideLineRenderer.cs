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
        Vector3[] ps = Utils.ApplyArrayToCenter(points, center, yaw);
        GameObject newLine = Instantiate(GuideLineBase);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
        newLine.transform.parent = gameObject.transform;

    }
}
