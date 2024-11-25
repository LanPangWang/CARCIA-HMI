using Google.Protobuf.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xviewer;


public class ApaObstacleRenderer : MonoBehaviour
{
    public GameObject Car;
    public GameObject Bicycle;
    public GameObject Pedestrain;
    public GameObject Barrier;
    public GameObject Cone;
    public GameObject Fence;

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
        RepeatedField<TrackBox> obstacles = WorldUtils.GetApaObstacleList(world);
        foreach (TrackBox obj in obstacles)
        {
            RenderObstacle(obj);
        }
    }

    // 清理不再存在的障碍物
    void ClearObstacles()
    {
        obstacleInstances.Clear();
        RepeatedField<TrackBox> obstacles = WorldUtils.GetApaObstacleList(world);
        foreach (Transform child in gameObject.transform)
        {
            bool stillExists = false;
            foreach (TrackBox obj in obstacles)
            {
                string name = GetObjectName(obj);
                if (obj.TrackId.ToString() == child.gameObject.name)
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

    string GetObjectName(TrackBox obj)
    {
        return obj.ClassLabel.ToString() + "_" + obj.TrackId.ToString();
    }

    void RenderObstacle(TrackBox obj)
    {
        int type = obj.ClassLabel;
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

    void SetObjstaclePosition(GameObject instance, TrackBox obj)
    {
        Vector3 p = new Vector3(obj.Cx, obj.Cy, 0);
        float heading = obj.Yaw;
        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, -heading * Mathf.Rad2Deg + 90, 0);
        instance.transform.rotation = rotation;
        instance.transform.localPosition = p;
        //Point position = obj.ReferencePoint?[0];
        //if (position is Point)
        //{
        //    Vector3 p = new Vector3((float)(position.X), (float)(position.Y), 0f);
        //}
    }

    GameObject GetPrefabForType(int type)
    {
        GameObject[] prefabs = new GameObject[]
        {
            null,
            null,
            Car,
            Bicycle,
            Pedestrain,
            Barrier,
            Cone,
            Fence,
        };
        if (prefabs[type] is GameObject)
        {
            return prefabs[type];
        }
        return null;
    }
}
