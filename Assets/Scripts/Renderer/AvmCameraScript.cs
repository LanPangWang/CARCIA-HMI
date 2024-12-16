using Xviewer;
using UnityEngine;
using System.Collections.Generic;

public class AvmCameraScript : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject CustomSlot;
    public GameObject SlotRotateButton;
    public GameObject SlotDirButton;
    public GameObject MainCar;
    public Material CustomSlotMat;
    public GameObject slotContainer;
    public Rigidbody CustomSlotPrefabRigidbody;
    public GameObject Radar;

    public Texture invalidSlotTex;
    public Texture validSlotTex;

    private SimulationWorld world;
    private MeshCollider SlotCollider; // 当前被选中的模型的collider
    private Camera avmCamera; // 绑定AvmCamera相机
    private GameObject selectedObject; // 当前被选中的模型
    private Bounds Border;
    private Vector3 offset; // 点击位置和模型位置的偏移量
    private postProcessTAA postProcess;
    private uint validDir;
    // private CarInfo oldCarInfo;
    // private Vector3 oldPosition;
    private Vector3 basicSlotPose;
    private Vector3 tgtSlotPose;
    private Vector3 basicRotateButtonLocalPos;
    private UnityEngine.Quaternion basicSlotRot;
    private float tgtYaw;

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
        // basicCarInfo = StateManager.Instance.carInfoDouble;
        // currentCarInfo = StateManager.Instance.carInfoDouble;
        basicSlotPose = CustomSlot.transform.parent.localPosition;
        basicSlotRot = CustomSlot.transform.parent.localRotation;
        tgtSlotPose = CustomSlot.transform.parent.localPosition;
        tgtYaw = 0;

        basicRotateButtonLocalPos = SlotRotateButton.transform.localPosition;
    }
    void FixedUpdate() 
    {
        bool inParking = StateManager.Instance.inParking;
        if (inParking)
        {
            if(StateManager.Instance.customSlotId != -1)
            {
                // Debug.Log(StateManager.Instance.customSlotId);
                try
                {
                    Transform customSlotTransform = slotContainer.transform.Find($"slot-{StateManager.Instance.customSlotId}");
                    // Debug.Log(customSlotTransform.name);
                    MeshFilter meshFilter = customSlotTransform.gameObject.GetComponent<MeshFilter>();
                    Vector3[] slotPoints = meshFilter.sharedMesh.vertices;
                    for(int i = 0; i < 4; i++)
                    {
                        slotPoints[i] = customSlotTransform.TransformVector(slotPoints[i]);
                    }
                    // Vector3[] slotPoints = StateManager.Instance.customSlotPoints;
                    // Debug.Log(slotPoints[0]);

                    Vector3 tgtDir = slotPoints[1] - slotPoints[2];
                    // tgtDir = new Vector3(tgtDir.y, 0f, -tgtDir.x);
                    // tgtDir.z = tgtDir.y;
                    tgtDir.y = 0f;
                    tgtDir.Normalize();
                    Vector3 slotDir = CustomSlot.transform.parent.forward;
                    float dirCos = tgtDir.x * slotDir.x + tgtDir.z * slotDir.z;
                    tgtDir *= (dirCos < 0) ? -1 : 1;
                    // Vector3 slotDir = SlotRotateButton
                    UnityEngine.Quaternion tgtRot = new UnityEngine.Quaternion();
                    tgtRot.SetFromToRotation(Vector3.forward, tgtDir);

                    Vector3 tgtPose = (slotPoints[0] + slotPoints[2]) / 2f;
                    // Vector3 tgtPose2 = new Vector3(tgtPose.y, 0f, -tgtPose.x);
                    // tgtPose.z = -tgtPose.y;
                    tgtPose.y = 0;
                    // CustomSlot.transform.parent.position = tgtPose;
                    // CustomSlot.transform.parent.rotation = tgtRot;
                    CustomSlotPrefabRigidbody.MovePosition(tgtPose);
                    CustomSlotPrefabRigidbody.MoveRotation(tgtRot);
                }
                catch { }
            }
        }
    }
    void FixRotButtonPos()
    {
        SlotRotateButton.transform.localPosition = basicRotateButtonLocalPos;
        bool xOut = Mathf.Abs(SlotRotateButton.transform.position.x - avmCamera.transform.position.x) > 8f;
        bool zOut = Mathf.Abs(SlotRotateButton.transform.position.z - avmCamera.transform.position.z) > 8f;
        SlotRotateButton.transform.localPosition = (xOut || zOut) ? -basicRotateButtonLocalPos : basicRotateButtonLocalPos;
    }

    void UpdateButton()
    {
        bool inParking = StateManager.Instance.inParking;
        bool needShow = validDir == 3 && !inParking;
        SlotDirButton.SetActive(needShow);
        SlotRotateButton.SetActive(!inParking);
    }

    void HandlerTouch()
    {
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
                FixRotButtonPos();
            }

            // 当触摸结束时，取消选择
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (selectedObject)
                {
                    OnTouchEnd(touch);
                }
            }
        }
    }

    void UpdateRadar()
    {
        Constants.PilotStateMap state = StateManager.Instance.pilotState;
        bool needShow = state == Constants.PilotStateMap.PARK_SEARCH || state == Constants.PilotStateMap.PARK_OUT_SEARCH;
        Radar.SetActive(needShow);
    }

    void Update()
    {
        // 确保触摸仅在AvmCamera上生效
        // Debug.Log($"{basicCarInfo.x}, {basicCarInfo.y}, {basicCarInfo.heading}");
        if (avmCamera == null || !avmCamera.orthographic)
            return;

        bool inParking = StateManager.Instance.inParking;
        validDir = StateManager.Instance.ValidCustomSlotDir;
        // CustomSlot.SetActive(!inParking);
        if(inParking)
        {
            CustomSlotMat.SetTexture("_MainTex", validSlotTex);
        }
        else
        {
            if (validDir != 0)
            {
                // CustomSlotMat.SetColor("_Color1", Color.green);
                CustomSlotMat.SetTexture("_MainTex", validSlotTex);
            } 
            else
            {
                CustomSlotMat.SetTexture("_MainTex", invalidSlotTex);
                // CustomSlotMat.SetColor("_Color1", Color.red);
            }
        }

        HandlerTouch();
        UpdateButton();
        UpdateRadar();
    }

    private void OnTouchBegan(Touch touch)
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

    private void OnTouchMove(Touch touch)
    {
        Vector3 center = GetCenter(touch);
        if (selectedObject.GetInstanceID() == SlotDirButton.GetInstanceID())
        {
            return;
        }
        else if (selectedObject.GetInstanceID() == CustomSlot.GetInstanceID())
        {
            CustomSlot.transform.parent.position = BorderFix(center);
        }
        else if (selectedObject.GetInstanceID() == SlotRotateButton.GetInstanceID())
        {

            // 使用四元数避免反三角函数以提高计算效率
            Vector3 direction = center - CustomSlot.transform.parent.position;
            direction.y = 0f;
            UnityEngine.Quaternion tgtRot = new UnityEngine.Quaternion();
            tgtRot.SetFromToRotation(Vector3.forward, direction);
            CustomSlot.transform.parent.rotation = tgtRot;

            // 保持位置不变，应用边界修正
            CustomSlot.transform.parent.position = BorderFix(CustomSlot.transform.parent.position);
        }
    }

    private async void OnTouchEnd(Touch touch)
    {
        if (selectedObject.GetInstanceID() == SlotDirButton.GetInstanceID())
        {
            OnDirClick();
        } else
        {
            StateManager.Instance.ChangeCustomSlotDir(1);
        }
        Vector3[] vertices = GetRectangleVertices(CustomSlot.transform.parent.position);
        List<string> points = Utils.GetCustomSlotPoints(vertices);
        uint frameId = StateManager.Instance.GetFrameId();
        selectedObject = null;
        await HmiSocket.Instance.LockCustomSlot(points, frameId);
    }

    private Vector3 GetCenter(Touch touch)
    {
        Vector2 realPosition = CalRealPosition(touch.position);
        // 对于正交相机，直接使用屏幕坐标来更新模型的位置
        Vector3 touchPosition = avmCamera.ScreenToWorldPoint(new Vector3(realPosition.x, realPosition.y, avmCamera.nearClipPlane));
        // 真实的中心坐标
        Vector3 center = new Vector3(touchPosition.x - offset.x, selectedObject.transform.position.y, touchPosition.z - offset.z);
        return center;
    }

    private Vector3 BorderFix(Vector3 center)
    {
        Vector3[] vertices = GetRectangleVertices(center);
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

    // 获取长方形的四个顶点
    public Vector3[] GetRectangleVertices(Vector3 center)
    {
        // 未旋转时的顶点相对于中心的坐标
        Vector3 lb = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[0]);
        Vector3 rb = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[1]);
        Vector3 lt = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[2]);
        Vector3 rt = CustomSlot.transform.TransformVector(SlotCollider.sharedMesh.vertices[3]);

        // transform.RotateAround(topLeft, Vector3.up, angle);
        // transform.RotateAround(topRight, Vector3.up, angle);
        // transform.RotateAround(bottomLeft, Vector3.up, angle);
        // transform.RotateAround(bottomRight, Vector3.up, angle);

        lb += center;
        lt += center;
        rb += center;
        rt += center;

        return new Vector3[] { lt, lb, rb, rt };
    }

    private void OnDirClick()
    {
        uint dir = StateManager.Instance.CustomSlotDir;
        Debug.Log("dir change ===" + dir + validDir);
        if (validDir != 3) return;
        if (dir == 1)
        {
            StateManager.Instance.ChangeCustomSlotDir(2);
        }
        else if (dir == 2)
        {
            StateManager.Instance.ChangeCustomSlotDir(1);
        }
    }

    public void ResetCustom()
    {
        CustomSlot.transform.parent.position = Constants.DefaultCustomSlotCenter;
        CustomSlot.transform.parent.rotation = new UnityEngine.Quaternion();
    }

    // private void OnParking()
    // {
    //     CarInfo newCarInfo = StateManager.Instance.carInfo;
    //     Vector3 positionOffset = newCarInfo.position - oldCarInfo.position; // X向前对应prefab的Z Y向右对应prefab的X
    //     Debug.Log("positionOffset====" + newCarInfo.position + oldPosition);
    //     Vector3 newPosition = new Vector3(oldPosition.x - positionOffset.y, 0, oldPosition.z - positionOffset.x);
    //     // 使用反向变换抵消B的移动和旋转  s
    //     CustomSlot.transform.parent.position = newPosition;
    //     // 以父级的位置为中心，绕Y轴旋转指定的角度
    //     float heading = oldCarInfo.heading - newCarInfo.heading;
    //     //CustomSlot.transform.parent.localRotation = UnityEngine.Quaternion.Euler(0, heading * Mathf.Rad2Deg, 0);
    //     //CustomSlot.transform.parent.RotateAround(Vector3.zero, Vector3.up, heading);
    // }
}