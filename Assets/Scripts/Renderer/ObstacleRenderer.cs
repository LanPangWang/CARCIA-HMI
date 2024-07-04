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
        ClearObstacles();
        RepeatedField<Object3D> obstacles = WorldUtils.GetObstacleList(world);
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
        long objId = obj.ObjectId;
        string name = GetObjectName(obj);
        Point position = obj.ReferencePointUtm?[0];
        GameObject prefab = GetPrefabForType(type);
        if (obstacleInstances.ContainsKey(name))
        {
        }
        else if (prefab != null)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = objId.ToString();
            instance.transform.SetParent(gameObject.transform);
            Debug.Log(name);
        }
    }

    void SetObjstaclePosition(GameObject obj, Point position)
    {
        Vector3 p = Utils.ApplyCenter(position, center);
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
