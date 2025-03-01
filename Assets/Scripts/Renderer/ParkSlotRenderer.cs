using Google.Protobuf.Collections;
using UnityEngine;
using Xviewer;

public class ParkSlotRenderer : MonoBehaviour
{
    public GameObject slotPrefab; // 在Inspector中将你的预设体赋值给这个变量
    public GameObject MainCamera;
    public GameObject AvmCamera;
    public Material CustomMat;
    public Texture2D invalidTexture;
    public Texture2D invalidTexture2;
    public Texture2D validTexture;
    public Texture2D lockTexture;
    public Texture2D parkingTexture;

    private SimulationWorld world;
    private TrajectoryPoint center;
    private float yaw = 0;
    private int lockId = -1;
    private RepeatedField<Avaliableslot> avaliableslots;
    private AvmCameraScript avmScript;

    private void Start()
    {
        avmScript = AvmCamera.GetComponent<AvmCameraScript>();
    }

    void Update()
    {
        ClearSlots();
        world = WebSocketNet.Instance.world;
        center = WebSocketNet.Instance.center;
        yaw = WebSocketNet.Instance.yaw;
        lockId = StateManager.Instance.lockSlotId;
        avaliableslots = StateManager.Instance.avaliableslots;
        if (world is SimulationWorld)
        {
            RepeatedField<ParkingSpace> slots = WorldUtils.GetSlots(world);
            MakeSlots(slots);
        }
        bool avmOpen = StateManager.Instance.AvmOpen;
        if (Input.touchCount > 0 && !avmOpen)
        {
            Touch touch = Input.GetTouch(0); // 获取第一个触摸点
            if (touch.phase == TouchPhase.Ended)
            {
                SelectSlot(touch);
            }
        }
    }

    void SelectSlot(Touch touch)
    {
        Constants.PilotStateMap pilotState = StateManager.Instance.pilotState;
        if (pilotState == Constants.PilotStateMap.PARK_CHOOSE || pilotState == Constants.PilotStateMap.PARK_LOCK)
        {
            // 获取触摸位置
            Vector3 touchPosition = touch.position;
            // 从摄像机向触摸点发出射线
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            // 射线检测与物体碰撞
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // 检查被选中物体的Layer是否为 "Slots"
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Slots"))
                {
                    // 找到选中的 Slots 层模型
                    string name = hit.collider.gameObject.name;
                    int id = int.Parse(name.Split("-")[1]);
                    Debug.Log("选中的模型是: " + id);
                    HmiSocket.Instance.LockSlot(id, 1);
                }
            }
        }
    }

    // 没帧渲染前 清理上一帧的线
    void ClearSlots()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void MakeSlots(RepeatedField<ParkingSpace> slots)
    {
        foreach (ParkingSpace slot in slots)
        {
            if (slot.Src == 4)
            {
                if (StateManager.Instance.inParking)
                {
                    StateManager.Instance.customSlotId = (int)slot.Id;
                }
            }
            MakeSlot(slot);
        }
        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(90, 0, 90);
        gameObject.transform.localRotation = rotation;
    }

    Vector3[] GetSlotPoints(ParkingSpace slot)
    {
        RepeatedField<TrajectoryPoint> points = new();

        for (int i = 0; i < 8; i += 2)
        {
            TrajectoryPoint p = new TrajectoryPoint
            {
                X = slot.ParkingSlotPoint[i],
                Y = slot.ParkingSlotPoint[i + 1]
            };
            points.Add(p);
        }
        Vector3[] ps = Utils.ApplyArrayToCenterWidthYaw(points, center, -yaw);
        return ps;
    }

    Texture2D GetTexture(long id, bool valid)
    {
        bool inParking = StateManager.Instance.inParking;
        if (id == lockId)
        {
            return inParking ? parkingTexture : lockTexture;
        }
        else if (inParking)
        {
            return invalidTexture2;
        }
        else if (valid)
        {
            foreach (var slot in avaliableslots)
            {
                if (slot.ParkingSlotId == id && slot.ParkingDir > 0)
                {
                    return validTexture;
                }
            }
            return invalidTexture;
        }
        return invalidTexture;
    }

    void MakeSlot(ParkingSpace slot)
    {
        Vector3[] points = GetSlotPoints(slot);

        if (points.Length != 4) return;

        points = Utils.ReorderPointsForShortEdges(points);

        // 检查每两个点之间的距离

        if (Utils.CalculateDistance(points[0], points[2]) < 0.1f || Utils.CalculateDistance(points[1], points[3]) < 0.1f)
        {
            return;
        }

        // 实例化预设体
        GameObject plane = Instantiate(slotPrefab);

        Texture2D texture = GetTexture(slot.Id, slot.Valid);
        Renderer renderer = plane.GetComponent<Renderer>();
        Material material = renderer.material; // 获取当前材质实例
        material.mainTexture = texture;       // 赋值纹理


        // 获取 PlaneFromPoints 脚本并设置点
        PlaneFromPoints planeFromPoints = plane.GetComponent<PlaneFromPoints>();
        planeFromPoints.SetPoints(points);
        // 设置父对象
        plane.transform.SetParent(gameObject.transform);
        // 确保子对象的局部旋转为零
        plane.transform.localRotation = UnityEngine.Quaternion.identity;

        plane.GetComponent<MeshCollider>().sharedMesh = planeFromPoints.mesh;

        plane.name = $"slot-{slot.Id}";
        // 如果 custom 为 true，设置特殊材质
        // 提高平面的位置
        Vector3 position = plane.transform.localPosition;
        position.z = -0.02f; // 略微抬高平面
        plane.transform.localPosition = position;
        if (slot.Src == 4)
        {
            plane.SetActive(false);
        }
    }

    //void MakeSlot1()
    //{
    //    Vector3[] points = new Vector3[4];
    //    points[0] = new Vector3(-1, -1, 0);
    //    points[1] = new Vector3(-1, 1, 0);
    //    points[2] = new Vector3(1, 1, 0);
    //    points[3] = new Vector3(1, -1, 0);
    //    // 实例化预设体
    //    GameObject plane = Instantiate(slotPrefab);

    //    // 获取 PlaneFromPoints 脚本并设置点
    //    PlaneFromPoints planeFromPoints = plane.GetComponent<PlaneFromPoints>();
    //    planeFromPoints.SetPoints(points);
    //    // 设置父对象
    //    plane.transform.SetParent(gameObject.transform);
    //    // 确保子对象的局部旋转为零
    //    plane.transform.localRotation = UnityEngine.Quaternion.identity;

    //    // 添加mesh 让射线能点击到车位
    //    MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
    //    plane.GetComponent<MeshCollider>().sharedMesh = planeFromPoints.mesh;

    //    plane.name = $"slot-1";
    //    // 提高平面的位置
    //    Vector3 position = plane.transform.localPosition;
    //    position.z = -0.02f; // 略微抬高平面
    //    plane.transform.localPosition = position;

    //    // 根据yaw角旋转导航线
    //    UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(90, 0, 90);
    //    gameObject.transform.localRotation = rotation;

    //}
}