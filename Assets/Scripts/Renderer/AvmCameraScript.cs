using UnityEngine;

public class AvmCameraScript : MonoBehaviour
{
    public GameObject MainCamera;

    private Camera avmCamera; // 绑定AvmCamera相机
    private GameObject selectedObject; // 当前被选中的模型
    private Vector3 offset; // 点击位置和模型位置的偏移量
    private Plane dragPlane; // 用于计算拖动的平面
    private postProcessTAA postProcess;

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
                Vector2 realPosition = new Vector2();
                realPosition.x = (touch.position.x - Screen.width * (1.0f + postProcess._AvmDevideOffset) / 2.0f);
                realPosition.y = touch.position.y;
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

            // 当触摸移动时，拖动模型
            if (touch.phase == TouchPhase.Moved && selectedObject != null)
            {
                // 对于正交相机，直接使用屏幕坐标来更新模型的位置
                Vector3 touchPosition = avmCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, avmCamera.nearClipPlane));

                // 更新模型的位置，同时考虑初始的点击偏移
                selectedObject.transform.position = new Vector3(touchPosition.x - offset.x, touchPosition.y - offset.y, selectedObject.transform.position.z);
            }

            // 当触摸结束时，取消选择
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                selectedObject = null;
            }
        }
    }
}