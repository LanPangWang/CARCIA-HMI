using UnityEngine;

public class AvmCameraScript : MonoBehaviour
{
    public GameObject MainCamera;

    private Camera avmCamera; // 绑定AvmCamera相机
    private GameObject selectedObject; // 当前被选中的模型
    private Vector3 offset; // 点击位置和模型位置的偏移量
    private Plane dragPlane; // 用于计算拖动的平面
    private postProcessTAA postProcess;
    private float minX, maxX, minZ, maxZ; // 拖动模型的边界
    private float r; // 自定义车位的旋转

    void Start()
    {
        // 查找名为 "AvmCamera" 的正交相机
        avmCamera = GameObject.Find("AvmCamera").GetComponent<Camera>();

        // 确保该相机是正交模式
        if (avmCamera != null && !avmCamera.orthographic)
        {
            Debug.LogError("AvmCamera must be set to orthographic mode!");
        }

        // 初始化拖动平面
        dragPlane = new Plane(Vector3.forward, Vector3.zero); // 假设模型处于XZ平面（Z轴向前）
        postProcess = MainCamera.GetComponent<postProcessTAA>();
    }

    void Update()
    {
        // 确保触摸仅在AvmCamera上生效
        if (avmCamera == null || !avmCamera.orthographic)
            return;

        // 检测是否有触摸
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // 获取第一个触摸点

            // 当触摸开始时，检查是否点击到模型
            if (touch.phase == TouchPhase.Began)
            {
                OnTouchBegan(touch);
            }

            // 当触摸移动时，拖动模型
            if (touch.phase == TouchPhase.Moved && selectedObject != null)
            {
                OnTouchMove(touch);
            }

            // 当触摸结束时，取消选择
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                selectedObject = null;
            }
        }
    }

    private void OnTouchBegan (Touch touch)
    {
        Vector2 realPosition = CalRealPosition(touch.position);
        Ray ray = avmCamera.ScreenPointToRay(realPosition);
        RaycastHit hit;
        // 如果射线碰到了带有Collider的物体
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                selectedObject = hit.collider.gameObject;
                // 计算射线与物体的点击偏移
                offset = hit.point - selectedObject.transform.position;
            }
        }
    }

    private void OnTouchMove (Touch touch)
    {
        Vector2 realPosition = CalRealPosition(touch.position);
        // 对于正交相机，直接使用屏幕坐标来更新模型的位置
        Vector3 touchPosition = avmCamera.ScreenToWorldPoint(new Vector3(realPosition.x, realPosition.y, avmCamera.nearClipPlane));
        // 真实的中心坐标
        Vector3 center = new Vector3(touchPosition.x - offset.x, selectedObject.transform.position.y, touchPosition.z - offset.z);
        // 后轴中心到车模型中心的Y轴距离是1.4795
        Vector2 center2 = new Vector2(center.x, center.z - 1.4795f);
        // 车位的4个顶点
        Vector2[] vertexs = GetRectangleVertices(center2, 2.3f, 5.2f, r);

        if (CheckBorder(vertexs))
        {
            selectedObject.transform.position = center;
        }
    }

    // 计算后处理前的位置
    private Vector2 CalRealPosition(Vector2 p)
    {

        Vector2 realPosition = new Vector2();
        realPosition.x = (p.x - Screen.width * (1.0f + postProcess._AvmDevideOffset) / 2.0f);
        realPosition.y = p.y;
        return realPosition;
    }

    bool CheckBorder(Vector2[] vertices)
    {
        // 遍历所有顶点
        foreach (Vector2 vertex in vertices)
        {
            // 检查x和y坐标是否都在范围内
            if (vertex.x < -8 || vertex.x > 8 || vertex.y < -8 || vertex.y > 8)
            {
                return false; // 只要有一个点超出范围，立即返回false
            }
        }
        return true; // 如果所有点都在范围内，返回true
    }

    // 获取长方形的四个顶点
    Vector2[] GetRectangleVertices(Vector2 center, float width, float height, float rotationAngle)
    {
        // 未旋转时的顶点相对于中心的坐标
        Vector2 topLeft = new Vector2(-width / 2, height / 2);
        Vector2 topRight = new Vector2(width / 2, height / 2);
        Vector2 bottomLeft = new Vector2(-width / 2, -height / 2);
        Vector2 bottomRight = new Vector2(width / 2, -height / 2);

        // 将旋转角度从度数转换为弧度
        float radians = rotationAngle * Mathf.Deg2Rad;

        // 计算旋转矩阵的正弦和余弦
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        // 旋转每个顶点
        topLeft = RotatePoint(topLeft, cos, sin) + center;
        topRight = RotatePoint(topRight, cos, sin) + center;
        bottomLeft = RotatePoint(bottomLeft, cos, sin) + center;
        bottomRight = RotatePoint(bottomRight, cos, sin) + center;

        return new Vector2[] { topLeft, topRight, bottomLeft, bottomRight };
    }

    // 旋转顶点的方法
    Vector2 RotatePoint(Vector2 point, float cos, float sin)
    {
        float xNew = point.x * cos - point.y * sin;
        float yNew = point.x * sin + point.y * cos;
        return new Vector2(xNew, yNew);
    }
}