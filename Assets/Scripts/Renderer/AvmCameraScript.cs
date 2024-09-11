using UnityEngine;

public class AvmCameraScript : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject CustomSlot;
    public GameObject DirRotateButton;
    public GameObject SlotRotateButton;
    private MeshCollider SlotCollider; // 当前被选中的模型的collider

    private Camera avmCamera; // 绑定AvmCamera相机
    private GameObject selectedObject; // 当前被选中的模型
    private Bounds Border; 
    private Vector3 offset; // 点击位置和模型位置的偏移量
    private postProcessTAA postProcess;

    void Start()
    {
        // 查找名为 "AvmCamera" 的正交相机
        avmCamera = GameObject.Find("AvmCamera").GetComponent<Camera>();
        SlotCollider = CustomSlot.GetComponent<MeshCollider>();

        // 确保该相机是正交模式
        if (avmCamera != null && !avmCamera.orthographic)
        {
            Debug.LogError("AvmCamera must be set to orthographic mode!");
        }

        // 初始化边界
        Border = new Bounds();
        Border.center = this.transform.position;
        Border.extents = new Vector3(avmCamera.orthographicSize, 1, avmCamera.orthographicSize);

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
        // Vector2 center2 = new Vector2(center.x, center.z - 1.4795f);
        // 车位的4个顶点
        // Vector2[] vertexs = GetRectangleVertices(center2, 2.3f, 5.2f, r);
        if (selectedObject.GetInstanceID() == CustomSlot.GetInstanceID())
        {
            CustomSlot.transform.parent.position = BorderFix(center);
        }
        else if (selectedObject.GetInstanceID() == SlotRotateButton.GetInstanceID())
        {

            // 使用四元数避免反三角函数以提高计算效率
            Vector3 direction = center - CustomSlot.transform.parent.position;
            direction.y = 0f;
            Quaternion tgtRot = new Quaternion();
            tgtRot.SetFromToRotation(Vector3.forward, direction);
            CustomSlot.transform.parent.rotation = tgtRot;

            // 保持位置不变，应用边界修正
            CustomSlot.transform.parent.position = BorderFix(CustomSlot.transform.parent.position);
        }
    }

    private Vector3 BorderFix(Vector3 center)
    {
        Vector3[] vertices = GetRectangleVertices(center, 0f);
        Vector3 min = vertices[0];
        Vector3 max = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            min.x = Mathf.Min(min.x, vertices[i].x);
            min.z = Mathf.Min(min.z, vertices[i].z);
            max.x = Mathf.Max(max.x, vertices[i].x);
            max.z = Mathf.Max(max.z, vertices[i].z);
        }
        Vector3 borderOffset = center;
        borderOffset.x += min.x < Border.min.x ? (Border.min.x - min.x) : 0;
        borderOffset.x -= max.x > Border.max.x ? (max.x - Border.max.x) : 0;
        borderOffset.z += min.z < Border.min.z ? (Border.min.z - min.z) : 0;
        borderOffset.z -= max.z > Border.max.z ? (max.z - Border.max.z) : 0;
        borderOffset.y = 0f;
        return borderOffset;
    }
    // 计算后处理前的位置
    private Vector2 CalRealPosition(Vector2 p)
    {

        Vector2 realPosition = new Vector2();
        realPosition.x = (p.x - Screen.width * (1.0f + postProcess._AvmDevideOffset) / 2.0f);
        realPosition.y = p.y;
        return realPosition;
    }

    bool CheckBorder(Vector3[] vertices)
    {
        // 遍历所有顶点
        foreach (Vector3 vertex in vertices)
        {
            // 检查x和y坐标是否都在范围内
            if (vertex.x < -8 || vertex.x > 8 || vertex.z < this.transform.position.z - 8 || vertex.z > this.transform.position.z + 8)
            {
                return false; // 只要有一个点超出范围，立即返回false
            }
        }
        return true; // 如果所有点都在范围内，返回true
    }

    // 获取长方形的四个顶点
    Vector3[] GetRectangleVertices(Vector3 center, float angle)
    {
        // 未旋转时的顶点相对于中心的坐标
        Vector3 topLeft = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[0]);
        Vector3 topRight = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[1]);
        Vector3 bottomLeft = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[2]);
        Vector3 bottomRight = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[3]);

        // transform.RotateAround(topLeft, Vector3.up, angle);
        // transform.RotateAround(topRight, Vector3.up, angle);
        // transform.RotateAround(bottomLeft, Vector3.up, angle);
        // transform.RotateAround(bottomRight, Vector3.up, angle);

        topLeft += center;
        topRight += center;
        bottomLeft += center;
        bottomRight += center;

        return new Vector3[] { topLeft, topRight, bottomLeft, bottomRight };
    }

}