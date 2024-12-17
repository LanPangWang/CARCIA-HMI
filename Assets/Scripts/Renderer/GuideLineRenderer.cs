using Google.Protobuf.Collections;
using UnityEngine;
using Xviewer;
using System.Linq;
using System;

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
        if (StateManager.Instance.pilotState == Constants.PilotStateMap.PARK_SUCCESS)
        {
            return;
        }
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

    Vector3[] FilterPointsByGear(Vector3[] points)
    {
        uint gear = StateManager.Instance.gear;
        Constants.GearTypes gearStr = (Constants.GearTypes)gear;
        if (gearStr == Constants.GearTypes.D)
        {
            return points.Where(point => point.x > 0).ToArray();
        }
        else if (gearStr == Constants.GearTypes.R)
        {
            return points.Where(point => point.x < 0).ToArray();
        }
        else
        {
            return points;
        }
    }

    void MakeGuideLine(TrajectoryStamped guideLine)
    {
        RepeatedField<TrajectoryPoint> points = guideLine.TrajPoints;
        Vector3[] ps = Utils.ApplyArrayToCenterWidthYaw(points, center, -yaw);
        ps = FilterPointsByGear(ps);
        ps = Utils.TranslateCurveByDistanceWithDirOffset(ps);
        GameObject newLine = Instantiate(GuideLineBase);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = ps.Length;
        lineRenderer.SetPositions(ps);
        newLine.transform.SetParent(gameObject.transform);
        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, 0, 90);
        newLine.transform.localRotation = rotation;
    }
}
