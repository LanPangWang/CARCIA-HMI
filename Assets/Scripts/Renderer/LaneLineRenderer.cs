using Google.Protobuf.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xviewer;

public class LaneLineRenderer : MonoBehaviour
{
    public GameObject DashLine;
    public GameObject BaseLine;

    private SimulationWorld world;

    void Start()
    {
    }

    void Update()
    {
        ClearLaneLines();
        world = WebSocketNet.Instance.world;
        (
            RepeatedField<LaneLine> laneLines,
            RepeatedField<RepeatedField<LaneLine>> otherLines,
            RepeatedField<Lane> lanes,
            Lane currentLane,
            RepeatedField<StopLine> stopLines
        ) = WorldUtils.GetLaneLines(world);
        RenderStopLines(stopLines);
        foreach(LaneLine line in laneLines)
        {
            MakeLine(line);
        }
        foreach (RepeatedField<LaneLine> lines in otherLines)
        {
            foreach (LaneLine line in lines)
            {
                MakeLine(line);
            }
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

    // 停止线
    void RenderStopLines(RepeatedField<StopLine> stopLines)
    {
        foreach (StopLine stopLine in stopLines)
        {
            RepeatedField<Point> points = stopLine.StopLinePoints;
            int count = stopLine.StopLinePoints.Count;
            Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = new Vector3((float)points[i].X, (float)points[i].Y, 0);
            }
            RenderLine(positions, BaseLine);
        }
    }

    void MakeLine(LaneLine laneLine)
    {
        RepeatedField<uint> typeList = laneLine.TypeList;
        RepeatedField<int> indexList = laneLine.IndexList;
        RepeatedField<Point> linePoints = laneLine.LinePoints;

        // 按照indexList切分点
        List<Vector3[]> lineSegments = Utils.SplitLineByIndex(indexList, linePoints);

        for (int i = 0; i < lineSegments.Count; i++)
        {
            uint type = typeList[i];
            if (!Constants.LINETYPE_PRESETS.ContainsKey(type))
            {
                continue;
            }

            Vector3[] segmentPoints = lineSegments[i];
            // 获取该段线段属性
            LineTypePreSet lineOptions = Constants.LINETYPE_PRESETS[type];

            RenderLineSegment(segmentPoints, lineOptions);
        }
    }

    void RenderLineSegment(Vector3[] points, LineTypePreSet lineOptions)
    {
        for (int i = 0; i < lineOptions.dash.Count; i++)
        {
            GameObject linePrefab = lineOptions.dash[i] ? DashLine : BaseLine;
            Vector3[] offsetPoints = ApplyOffset(points, i, lineOptions.dash.Count);

            RenderLine(offsetPoints, linePrefab);
        }
    }

    // 判断是否是双线，如果是双线 dash中会有两项
    Vector3[] ApplyOffset(Vector3[] points, int index, int dashCount)
    {
        if (dashCount == 2)
        {
            float offsetX = (index == 0) ? -0.1f : 0.1f;
            Vector3 offset = new Vector3(offsetX, 0, 0);
            Vector3[] offsetPoints = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                offsetPoints[i] = points[i] + offset;
            }

            return offsetPoints;
        }

        return points;
    }

    void RenderLine(Vector3[] points, GameObject linePrefab)
    {
        GameObject newLine = Instantiate(linePrefab);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        newLine.transform.parent = gameObject.transform;
    }
}
