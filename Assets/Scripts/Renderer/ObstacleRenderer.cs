using Google.Protobuf.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xviewer;


public class ObstacleRenderer : MonoBehaviour
{
    public GameObject Car;
    public GameObject Suv;
    public GameObject Bus;
    public GameObject Truck;

    private SimulationWorld world;
    private float yaw;
    public TrajectoryPoint center = new TrajectoryPoint();
    private Dictionary<string, int> obstacleInstances = new Dictionary<string, int>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        world = WebSocketNet.Instance.world;
        center = WebSocketNet.Instance.center;
        yaw = WebSocketNet.Instance.yaw;
        ClearObstacles();
        RepeatedField<Object3D> obstacles = WorldUtils.GetObstacleList(world);
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(90, 0, 90);
        gameObject.transform.localRotation = rotation;
        foreach (Object3D obj in obstacles)
        {
            RenderObstacle(obj);
        }
    }

    // 清理不再存在的障碍物
    void ClearObstacles()
    {
        obstacleInstances.Clear();
        RepeatedField<Object3D> obstacles = WorldUtils.GetObstacleList(world);
        foreach (Transform child in gameObject.transform)
        {
            bool stillExists = false;
            foreach (Object3D obj in obstacles)
            {
                string name = GetObjectName(obj);
                if (obj.ObjectId.ToString() == child.gameObject.name)
                {
                    stillExists = true;
                    obstacleInstances.Add(name, 1);
                    break;
                }
            }

            if (!stillExists)
            {
                Destroy(child.gameObject);
            }
        }
    }

    string GetObjectName(Object3D obj)
    {
        return obj.ObjectType.ToString() + "_" + obj.ObjectId.ToString();
    }

    void RenderObstacle(Object3D obj)
    {
        int type = obj.ObjectType;
        string name = GetObjectName(obj);
        GameObject prefab = GetPrefabForType(type);
        if (obstacleInstances.ContainsKey(name))
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.name == name)
                {
                    GameObject instance = child.gameObject;
                    SetObjstaclePosition(instance, obj);
                }
            }

        }
        else if (prefab != null)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = GetObjectName(obj);
            instance.transform.SetParent(gameObject.transform);
            SetObjstaclePosition(instance, obj);
        }
    }

    void SetObjstaclePosition(GameObject instance, Object3D obj)
    {
        Point position = obj.ReferencePoint?[0];
        if (position is Point)
        {
            Vector3 p = new Vector3((float)(position.X), (float)(position.Y), 0f);
            float heading = obj.YawAngle;
            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(-90, (heading + yaw) * Mathf.Rad2Deg, 0);
            instance.transform.localRotation = rotation;
            instance.transform.localPosition = p;
        }
    }

    GameObject GetPrefabForType(int type)
    {
        switch (type)
        {
            case 1:
                return Car;
            case 2:
                return Truck;
            case 3:
                return Bus;
            // 你可以继续添加其他的类型
            default:
                return null;
        }
    }
}
