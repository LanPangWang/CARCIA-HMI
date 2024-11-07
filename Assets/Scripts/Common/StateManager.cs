using Google.Protobuf.Collections;
using UnityEngine;
using System.Linq;
using Xviewer;

public class ParkOutInfo
{
    public int[] dirMap;         // 用于存储方向的数组
    public int lockParkOutDir;   // 锁定的泊车方向
    public int type;             // 类型
    public int apaScene;         // 场景信息

    // 构造函数
    public ParkOutInfo(int[] dirMap, int lockParkOutDir, int type, int apaScene)
    {
        this.dirMap = dirMap;
        this.lockParkOutDir = lockParkOutDir;
        this.type = type;
        this.apaScene = apaScene;
    }
}

public class CarInfo
{
    public Vector3 position;
    public float heading;
}

public class StateManager : MonoBehaviour
{
    private SimulationWorld world;

    public static StateManager Instance { get; private set; }

    public bool AvmOpen { get; private set; } = false;

    public uint frameId { get; private set; } = 0;

    public uint CustomSlotDir { get; private set; } = 1;

    public Constants.PilotStateMap pilotState { get; private set; } = 0;

    public int apaPlaneState { get; private set; } = 0;

    public int speed { get; private set; } = 0;

    public RepeatedField<ParkingSpace> slots { get; private set; } = new RepeatedField<ParkingSpace>();

    public ParkOutInfo parkOutInfo { get; private set; }

    public string bevImage { get; private set; }   // avm的bev图

    public CarInfo carInfo { get; private set; }

    public bool inParking { get; private set; }


    private float parkSuccessTime = 0f; // 用于跟踪 PARK_SUCCESS 的持续时间
    private const float ParkSuccessDuration = 3f; // 持续时间阈值，单位秒

    private void Awake()
    {
        parkOutInfo = new ParkOutInfo(new int[] { 0, 0, 0, 0, 0, 0 }, -1, 1, 2); // 示例数据
        carInfo = new CarInfo();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        world = WebSocketNet.Instance.world;
        speed = WorldUtils.GetSpeed(world);
        slots = WorldUtils.GetParkingSlots(world);
        carInfo = WorldUtils.GetCarInfo(world);
        apaPlaneState = WorldUtils.GetApaPlanState(world);
        Constants.PilotStateMap state = (int)UpdateParkingState(world) != 0 ? UpdateParkingState(world) : UpdateDrivingState(world);
        UpdateState(state);
        //pilotState = Constants.PilotStateMap.PARK_SEARCH;
    }

    private async void UpdateState(Constants.PilotStateMap state)
    {
        if (state != pilotState)
        {
            parkSuccessTime = 0f;
            pilotState = state;
            inParking = StateManager.Instance.pilotState == Constants.PilotStateMap.PARK_ING;
        }
        else
        {
            if (state == Constants.PilotStateMap.PARK_SUCCESS)
            {
                parkSuccessTime += UnityEngine.Time.deltaTime; // 增加计时
                // 如果 PARK_SUCCESS 持续了 3 秒，调用ExitCustomSlot
                if (parkSuccessTime >= ParkSuccessDuration)
                {
                    uint frameId = GetFrameId();
                    await HmiSocket.Instance.ExitCustomSlot(frameId);
                }
            }
            else
            {
                // 如果状态不再是 PARK_SUCCESS，重置计时器
                parkSuccessTime = 0f;
            }
        }
    }

    private Constants.PilotStateMap UpdateParkingState(SimulationWorld world)
    {
        var state = (Constants.PilotStateMap)WorldUtils.GetState(world);
        var validSlots = slots.Where((_) => _.Valid).ToArray();
        switch (state)
        {
            case Constants.PilotStateMap.PARK_SEARCH:
                if (validSlots.Length > 0 && speed < 1)
                {
                    return Constants.PilotStateMap.PARK_CHOOSE;
                }
                if (apaPlaneState == 1)
                {
                    return Constants.PilotStateMap.PARK_PLANING;
                }
                break;
            case Constants.PilotStateMap.PARK_SUCCESS:
            case Constants.PilotStateMap.PARK_OUT_SUCCESS:
                if (parkSuccessTime >= ParkSuccessDuration)
                {
                    return Constants.PilotStateMap.none;
                }
                break;
            case Constants.PilotStateMap.PARK_TAKE_OVER:
            case Constants.PilotStateMap.PARK_OUT_OVER:
                return Constants.PilotStateMap.PARK_FAIL;
                break;
            case Constants.PilotStateMap.PARK_OUT_SELECT:
                if (parkOutInfo.lockParkOutDir >= 0)
                {
                    return Constants.PilotStateMap.PARK_OUT_START;
                }
                break;
            default:
                break;
        }
        return state >= 0 ? state : 0;
    }

    private Constants.PilotStateMap UpdateDrivingState(SimulationWorld world)
    {
        return WorldUtils.IsInPilot(world) ? Constants.PilotStateMap.DRIVE_ING : Constants.PilotStateMap.none;
    }

    public void SetAvmOpen(bool open)
    {
        AvmOpen = open;
        // 处理暂停状态的逻辑，例如暂停游戏，显示暂停菜单等
    }

    public uint GetFrameId()
    {
        uint id = frameId;
        frameId += 1;
        return id;
    }

    public void ChangeParkOutDir(int dir)
    {
        if (dir != parkOutInfo.lockParkOutDir)
        {
            parkOutInfo.lockParkOutDir = dir;
        }
    }

    public void ChangeCustomSlotDir(uint dir)
    {
        if (CustomSlotDir != dir)
        {
            CustomSlotDir = dir;
        }
    }

    public void SetBevImage(string texture)
    {
        bevImage = texture;
    }

    public void SetInparking()
    {
        inParking = !inParking;
    }
}
