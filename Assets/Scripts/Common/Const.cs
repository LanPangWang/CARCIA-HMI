using UnityEngine;
using System.Collections.Generic;

public interface IPoint
{
    double X { get; }
    double Y { get; }
}

public class CameraPreset
{
    public Vector3 position;
    public Quaternion rotation;

    public CameraPreset(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}

public class LineTypePreSet
{
    public List<bool> dash;
    public int color;
    public float width;

    public LineTypePreSet(List<bool> dash, int color, float width)
    {
        this.dash = dash;
        this.color = color;
        this.width = width;
    }
}

public static class Constants
{
    public static bool DASH = true;
    public static bool SOLID = false;
    public static int WHITE = 0xffffff;
    public static int YELLOW = 0xffff00;
    public static int RED = 0xff564d;
    public static int GRAY = 0x828282;
    public static float NORMAL_WIDTH = 0.1f;
    public static float BOLD_WIDTH = 0.2f;

    // 镜头预设
    public static readonly List<CameraPreset> CAMERA_PRESETS = new List<CameraPreset>
    {
        new CameraPreset(new Vector3(0, 5, -8), Quaternion.Euler(15, 0, 0)), // 正常
        new CameraPreset(new Vector3(0, 8, -10), Quaternion.Euler(25, 0, 0)), // 稍微拉远
        new CameraPreset(new Vector3(0, 10, 0), Quaternion.Euler(90, 0, 0)), // 完全俯视
    };

    // 车道线属性列表
    public static readonly Dictionary<uint, LineTypePreSet> LINETYPE_PRESETS = new Dictionary<uint, LineTypePreSet>
    {
        { 0, new LineTypePreSet(new List<bool>{DASH}, WHITE, NORMAL_WIDTH) },
        { 1, new LineTypePreSet(new List<bool>{DASH}, WHITE, NORMAL_WIDTH) },
        { 2, new LineTypePreSet(new List<bool>{SOLID}, WHITE, NORMAL_WIDTH) },

        { 3, new LineTypePreSet(new List<bool>{DASH, DASH}, WHITE, NORMAL_WIDTH) },
        { 4, new LineTypePreSet(new List<bool>{SOLID, SOLID}, WHITE, NORMAL_WIDTH) },
        { 5, new LineTypePreSet(new List<bool>{SOLID, DASH}, WHITE, NORMAL_WIDTH) },
        { 6, new LineTypePreSet(new List<bool>{DASH, SOLID}, WHITE, NORMAL_WIDTH) },

        { 7, new LineTypePreSet(new List<bool>{DASH}, YELLOW, NORMAL_WIDTH) },
        { 8, new LineTypePreSet(new List<bool>{DASH}, YELLOW, NORMAL_WIDTH) },

        { 9, new LineTypePreSet(new List<bool>{DASH, DASH}, YELLOW, NORMAL_WIDTH) },
        { 10, new LineTypePreSet(new List<bool>{SOLID, SOLID}, YELLOW, NORMAL_WIDTH) },
        { 11, new LineTypePreSet(new List<bool>{SOLID, DASH}, YELLOW, NORMAL_WIDTH) },
        { 12, new LineTypePreSet(new List<bool>{DASH, SOLID}, YELLOW, NORMAL_WIDTH) },

        { 13, new LineTypePreSet(new List<bool>{DASH}, WHITE, BOLD_WIDTH) },
        { 14, new LineTypePreSet(new List<bool>{SOLID}, WHITE, BOLD_WIDTH) },

        { 20, new LineTypePreSet(new List<bool>{SOLID}, WHITE, NORMAL_WIDTH) },
        { 21, new LineTypePreSet(new List<bool>{SOLID}, WHITE, NORMAL_WIDTH) },
        { 22, new LineTypePreSet(new List<bool>{SOLID}, WHITE, NORMAL_WIDTH) },
        { 23, new LineTypePreSet(new List<bool>{SOLID}, WHITE, BOLD_WIDTH) },

        { 101, new LineTypePreSet(new List<bool>{DASH}, RED, BOLD_WIDTH) },
        { 102, new LineTypePreSet(new List<bool>{DASH}, RED, BOLD_WIDTH) },
        { 103, new LineTypePreSet(new List<bool>{DASH}, RED, BOLD_WIDTH) },
    };

    public enum GearTypes
    {
        D = 1,
        N,
        R,
        P,
    }
}