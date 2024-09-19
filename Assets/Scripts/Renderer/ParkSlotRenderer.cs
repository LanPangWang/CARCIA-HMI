using Google.Protobuf.Collections;
using UnityEngine;
using Xviewer;

public class ParkSlotRenderer : MonoBehaviour
{
    public GameObject slotPrefab; // 在Inspector中将你的预设体赋值给这个变量
    public GameObject MainCamera;

    private SimulationWorld world;
    private TrajectoryPoint center;
    private float yaw = 0;

    void Update()
    {
        ClearSlots();
        world = WebSocketNet.Instance.world;
        center = WebSocketNet.Instance.center;
        yaw = WebSocketNet.Instance.yaw;
        if (world is SimulationWorld)
        {
            RepeatedField<ParkingSpace> slots = WorldUtils.GetSlots(world);
            MakeSlots(slots);
        }

        if (Input.touchCount > 0)
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
            bool ishit = Physics.Raycast(ray, out hit);
            Debug.Log(ray);
            Debug.Log(hit.collider);
            // 射线检测与物体碰撞
            if (ishit)
            {
                // 检查被选中物体的Layer是否为 "Slots"
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Slots"))
                {
                    // 找到选中的 Slots 层模型
                    GameObject selectedSlot = hit.collider.gameObject;
                    Debug.Log("选中的模型是: " + selectedSlot.name);

                    // 你可以在这里对选中的模型做进一步处理
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
            MakeSlot(slot);
        }
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
        Vector3[] ps = Utils.ApplyArrayToCenter(points, center);
        return ps;
    }

    void MakeSlot(ParkingSpace slot)
    {
        Vector3[] points = GetSlotPoints(slot);
        // 实例化预设体
        GameObject plane = Instantiate(slotPrefab);

        // 获取 PlaneFromPoints 脚本并设置点
        PlaneFromPoints planeFromPoints = plane.GetComponent<PlaneFromPoints>();
        planeFromPoints.SetPoints(points);
        // 设置父对象
        plane.transform.SetParent(gameObject.transform);
        // 确保子对象的局部旋转为零
        plane.transform.localRotation = UnityEngine.Quaternion.identity;


        //MeshFilter meshFilter = plane.GetComponent<MeshFilter>();
        //plane.GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

        // 提高平面的位置
        Vector3 position = plane.transform.localPosition;
        position.z = -0.02f; // 略微抬高平面
        plane.transform.localPosition = position;

        // 根据yaw角旋转导航线
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(90, 0, -yaw * Mathf.Rad2Deg + 90);
        gameObject.transform.localRotation = rotation;
    }

    //void MakeSlot()
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

    //    // 提高平面的位置
    //    Vector3 position = plane.transform.localPosition;
    //    position.y = 0.01f; // 略微抬高平面
    //    gameObject.transform.localPosition = position;
    //}
}