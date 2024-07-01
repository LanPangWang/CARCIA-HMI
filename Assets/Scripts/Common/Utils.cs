using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;
using System.Linq;
using Xviewer;

public static class Utils
{

    public static List<Vector3[]> SplitLineByIndex(RepeatedField<int> indexList, RepeatedField<Point> points)
    {
        var result = new List<Vector3[]>();

        int start = 0;
        foreach (int index in indexList)
        {
            var segment = new List<Vector3>();
            for (int i = start; i < index; i++)
            {
                segment.Add(new Vector3((float)points[i].X, (float)points[i].Y, 0));
            }
            result.Add(segment.ToArray());  // 将 List<Vector3> 转换为 Vector3[]
            start = index;
        }

        // 添加最后一段
        var lastSegment = new List<Vector3>();
        for (int i = start; i < points.Count; i++)
        {
            lastSegment.Add(new Vector3((float)points[i].X, (float)points[i].Y, 0));
        }
        result.Add(lastSegment.ToArray());

        return result;
    }

    public static Vector3 ApplyCenter(TrajectoryPoint p, TrajectoryPoint c, float yaw)
    {
        // 计算相对坐标
        float x = (float)(p.X - c.X);
        float y = (float)(p.Y - c.Y);
        float z = 0f;
        Vector3 relativePos = new Vector3(x, y, z);
        //// 旋转相对坐标
        float cosYaw = Mathf.Cos(yaw);
        float sinYaw = Mathf.Sin(yaw);

        float xNew = relativePos.x * cosYaw - relativePos.y * sinYaw;
        float yNew = relativePos.x * sinYaw + relativePos.y * cosYaw;

        // 返回新的 BEV 坐标
        return new Vector3(xNew, yNew, relativePos.z);
    }

    public static Vector3[] ApplyArrayToCenter(RepeatedField<TrajectoryPoint> points, TrajectoryPoint c, float yaw)
    {
        Vector3[] newPoints = new Vector3[0];
        if (double.IsNaN(c.X) || double.IsNaN(c.Y))
        {
            Debug.LogError("Vector3 'c' contains NaN value in x or y component.");
            return newPoints;
        }
        // 检查points是否是一个Vector3数组
        if (points == null || !(points is RepeatedField<TrajectoryPoint>))
        {
            Debug.LogError("Vector3 'c' contains NaN value in x or y component.");
            return newPoints;
        }
        newPoints = points.Select(p => ApplyCenter(p, c, yaw)).ToArray();
        return newPoints;
    }

    public static float GetYOnLine(Vector3 p1, Vector3 p2, float x)
    {
        float x1 = p1.x;
        float y1 = p1.y;

        float x2 = p2.x;
        float y2 = p2.y;

        float y = y1 + (y2 - y1) * (x - x1) / (x2 - x1);
        return y;
    }
}
