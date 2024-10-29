using System.Collections.Generic;
using Google.Protobuf.Collections;
using System.Reflection;
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

    public static Vector3 ApplyCenter<T>(T p, TrajectoryPoint c)
    {
        // 获取类型T的X和Y属性
        PropertyInfo xProp = typeof(T).GetProperty("X");
        PropertyInfo yProp = typeof(T).GetProperty("Y");

        // 检查是否存在这些属性
        if (xProp == null || yProp == null)
        {
            Debug.LogError("对象必须包含 X 和 Y 属性。");
            return new Vector3();
        }

        // 计算相对坐标
        double px = (double)xProp.GetValue(p);
        double py = (double)yProp.GetValue(p);

        float x = (float)(px - c.X);
        float y = (float)(py - c.Y);
        float z = 0f;
        Vector3 relativePos = new Vector3(x, y, z);

        // 返回新的 BEV 坐标
        return new Vector3(x, y, relativePos.z);
    }

    public static Vector3[] ApplyArrayToCenter<T>(RepeatedField<T> points, TrajectoryPoint c)
    {
        Vector3[] newPoints = new Vector3[0];
        if (double.IsNaN(c.X) || double.IsNaN(c.Y))
        {
            Debug.LogError("Vector3 'c' contains NaN value in x or y component.");
            return newPoints;
        }

        // 检查points是否是一个Vector3数组
        if (points == null || !(points is RepeatedField<T>))
        {
            Debug.LogError("Points 集合必须是 RepeatedField<T> 类型。");
            return newPoints;
        }

        newPoints = points.Select(p => ApplyCenter(p, c)).ToArray();
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

    public static string GetUrl(string ip, string type)
    {
        string url = $"ws://{ip}:8888/{type}";
        return url;
    }

    public static string GetCameraUrl(string ip, string type)
    {
        string url = $"ws://{ip}:8899/{type}";
        return url;
    }

    // 将自定义车位中的4个Vector3 存到一个数组中
    public static List<string> GetCustomSlotPoints(Vector3[] vertices)
    {
        List<string> points = new List<string>();
        foreach (Vector3 p in vertices)
        {
            points.Add(p.x.ToString("F5"));
            points.Add(p.y.ToString("F5"));
            points.Add(p.z.ToString("F5"));
        }
        return points;
    }
}
