using UnityEngine;
using System;
using System.Collections.Generic;

public interface IPoint
{
    double X { get; }
    double Y { get; }
}

public class InteractionInfo
{
    public string label { get; set; }
    public string? icon { get; set; }  // `?` 表示该属性是可选的（nullable）
    public Action? cb { get; set; }    // 可选的回调函数

    // 构造函数
    public InteractionInfo(string label, string? icon = null, Action? cb = null)
    {
        this.label = label;
        this.icon = icon;
        this.cb = cb;
    }
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
        new CameraPreset(new Vector3(0, 20, 1.4795f), Quaternion.Euler(90, 0, 0)), // 完全俯视
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

    public enum PilotStateMap
    {
        none = 0,
        PARK_SEARCH,
        PARK_LOCK,
        PARK_ING,
        PARK_SUCCESS,
        PARK_FAIL,
        PARK_TAKE_OVER,
        PARK_CHOOSE,
        PARK_PLANING,
        PARK_PLAN_ERROR,
        PARK_OUT_SEARCH = 11,
        PARK_OUT_SELECT,
        PARK_OUT_ING,
        PARK_OUT_SUCCESS,
        PARK_OUT_ERROR,
        PARK_OUT_OVER,
        PARK_OUT_START,
        DRIVE_ING = 20,
    }

    // PilotStateMap 需要正常行车视角的子集 
    public static readonly HashSet<PilotStateMap> DriveStates = new HashSet<PilotStateMap>
    {
        PilotStateMap.none,
        PilotStateMap.DRIVE_ING,
    };

    public static Dictionary<PilotStateMap, InteractionInfo> InteractionInfo = new Dictionary<PilotStateMap, InteractionInfo>
    {
        { PilotStateMap.none, new InteractionInfo("进入APA", cb: () => HmiSocket.Instance.StartApa() ) },
        { PilotStateMap.DRIVE_ING, new InteractionInfo("领航驾驶中")},
        { PilotStateMap.PARK_SEARCH, new InteractionInfo("正在搜索车位")},
        { PilotStateMap.PARK_CHOOSE, new InteractionInfo("请选择车位")},
        { PilotStateMap.PARK_PLANING, new InteractionInfo("路径规划中")},
        { PilotStateMap.PARK_PLAN_ERROR, new InteractionInfo("路径规划失败")},
        { PilotStateMap.PARK_LOCK, new InteractionInfo("开始泊车", cb: () => HmiSocket.Instance.StartPark() )},
        { PilotStateMap.PARK_ING, new InteractionInfo("自动泊入中")},
        { PilotStateMap.PARK_SUCCESS, new InteractionInfo("泊入完成")},
        { PilotStateMap.PARK_FAIL, new InteractionInfo("泊车失败", "parkFail")},
        { PilotStateMap.PARK_TAKE_OVER, new InteractionInfo("泊车失败", "parkFail")},
        { PilotStateMap.PARK_OUT_SEARCH, new InteractionInfo("搜索泊出方向")},
        { PilotStateMap.PARK_OUT_SELECT, new InteractionInfo("请选择泊出方向")},
        { PilotStateMap.PARK_OUT_START, new InteractionInfo("开始泊出", cb: () => HmiSocket.Instance.StartApaOut())},
        { PilotStateMap.PARK_OUT_ING, new InteractionInfo("正在泊出")},
        { PilotStateMap.PARK_OUT_ERROR, new InteractionInfo("泊出失败")},
        { PilotStateMap.PARK_OUT_OVER, new InteractionInfo("泊出失败")},
    };

    // 初始自定义车位在UTM坐标中的位置，每次touch end 更新
    public static Vector3 DefaultCustomSlotCenter = new Vector3(2.21f, 0, 1.4795f);
}