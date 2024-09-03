using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public int cameraMap;
    public CameraPreset target; // 目标位置的对象

    private Vector3 startPosition; // 起始位置
    private Quaternion startRotation; // 起始旋转
    private Coroutine currentCoroutine; // 当前运行的协程

    private float lastTouchTime; // 上次触摸时间
    private float touchTimeout = 2.0f; // 超时时间
    private float initialDistance; // 初始双指距离
    private Vector3 initialPosition; // 初始摄像机位置
    private Quaternion initialRotation; // 初始摄像机旋转
    private bool isMoved = false;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        lastTouchTime = Time.time; // 初始化上次触摸时间
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
        HandleTouchInput();

        // 检查是否超过触摸超时
        if (isMoved && Time.time - lastTouchTime > touchTimeout)
        {
            isMoved = false;
            target = Constants.CAMERA_PRESETS[0];
            StartFlyToAnimation();
        }
        // 获取摄像机当前的位置和方向  
        //Vector3 cameraPosition = Camera.main.transform.position;
        //Vector3 cameraForward = Camera.main.transform.forward;

        //// 创建从摄像机位置出发，沿着摄像机方向的射线  
        //Ray ray = new Ray(cameraPosition, cameraForward);

        //// 创建一个RaycastHit变量来存储射线的交点信息  
        //RaycastHit hit;

        //// 投射射线，检查是否与地面相交（这里假设地面是一个平面，高度为groundHeight）  
        //// 注意：这里我们实际上是在检查射线是否与任何物体相交，而不仅仅是地面  
        //// 你可能需要一个特定的LayerMask或其他方法来确保只与地面相交  
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground"))) // "Ground"是地面的Layer名，需要你自己设置  
        //{
        //    // 如果射线与物体相交，hit.point就是交点的位置  
        //    // 但是，因为我们假设地面在Y=groundHeight，所以我们可能需要调整这个点的Y值  
        //    Vector3 intersectionPoint = new Vector3(hit.point.x, 0, hit.point.z);
        //    Debug.Log("Intersection Point: " + intersectionPoint);
        //}
        //else
        //{
        //    Debug.Log("No intersection with ground");
        //}
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            target = Constants.CAMERA_PRESETS[0];
            StartFlyToAnimation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            target = Constants.CAMERA_PRESETS[1];
            StartFlyToAnimation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            target = Constants.CAMERA_PRESETS[2];
            StartFlyToAnimation();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            lastTouchTime = Time.time; // 更新上次触摸时间
            isMoved = true;
             if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    Vector3 deltaPosition = touch.deltaPosition;
                    transform.Translate(-deltaPosition.x * 0.01f, -deltaPosition.y * 0.01f, 0);

                    // 在拖动时LookAt (0, 0, 0)
                    transform.LookAt(new Vector3(0, 0, 10.66f));
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(touch1.position, touch2.position);
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    float scaleFactor = currentDistance / initialDistance;

                    Vector3 newPosition = initialPosition * scaleFactor;
                    newPosition.z = transform.position.z; // 保持Z轴不变

                    transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 2);
                }
            }
        }
    }

    // 触发动画的方法
    public void StartFlyToAnimation()
    {
        // 停止当前正在运行的协程（如果有的话）
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        // 记录当前摄像机的位置和旋转作为起始值
        startPosition = transform.position;
        startRotation = transform.rotation;

        // 开始协程执行动画
        currentCoroutine = StartCoroutine(FlyToCoroutine());
    }

    IEnumerator FlyToCoroutine()
    {
        float elapsedTime = 0.0f;
        float duration = 0.3f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // 使用Lerp插值平滑过渡到目标位置
            transform.position = Vector3.Lerp(startPosition, target.position, t);

            // 使用Slerp插值平滑过渡到目标旋转
            transform.rotation = Quaternion.Slerp(startRotation, target.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终位置和旋转与目标一致（尽管由于浮点精度，这通常已经接近了）
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
