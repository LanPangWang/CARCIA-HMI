using UnityEngine;

public class AvmLayout : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject DriveInfoContainer;

    private bool Open = false;
    private postProcessTAA postProcess;
    private float targetOffset = -0.5f;
    private float animationDuration = 0.3f; // 动画时长为 0.3 秒
    private float animationTime = 0f;
    private bool isAnimating = false;
    private float initialOffset;

    private RectTransform driveInfoRectTransform;
    private float defaultX;

    void Start()
    {
        postProcess = MainCamera.GetComponent<postProcessTAA>();
        driveInfoRectTransform = DriveInfoContainer.GetComponent<RectTransform>();
        Debug.Log("-=---------");

        Debug.Log(driveInfoRectTransform.position);
        Debug.Log(driveInfoRectTransform.localPosition);
        defaultX = driveInfoRectTransform.localPosition.x;
        float largeSlide = Mathf.Max(Screen.width, Screen.height);
        float smallSlide = Mathf.Min(Screen.width, Screen.height);
        targetOffset = -(smallSlide / largeSlide);
    }

    void Update()
    {
        if (!Open && StateManager.Instance.AvmOpen)
        {
            Open = true;
            StartAnimation();
        }

        if (Open && !StateManager.Instance.AvmOpen)
        {
            Open = false;
            postProcess._AvmDevideOffset = 0f;
            UpdateDriveInfoPosition(0f);
        }

        if (isAnimating)
        {
            AnimateOffset();
        }
    }

    void StartAnimation()
    {
        initialOffset = postProcess._AvmDevideOffset; // 记录初始偏移量
        animationTime = 0f; // 重置动画时间
        isAnimating = true;
    }

    void AnimateOffset()
    {
        animationTime += Time.deltaTime;
        float t = animationTime / animationDuration;
        t = Mathf.Clamp01(t); // 确保 t 在 0 到 1 之间

        // 使用 Mathf.SmoothStep 实现 EaseInOut 效果
        float smoothStep = Mathf.SmoothStep(0f, 1f, t);
        float newOffset = Mathf.Lerp(initialOffset, targetOffset, smoothStep);

        postProcess._AvmDevideOffset = newOffset;
        UpdateDriveInfoPosition(newOffset);

        if (t >= 1f)
        {
            isAnimating = false; // 动画完成
        }
    }

    void UpdateDriveInfoPosition(float offset)
    {
        if (driveInfoRectTransform != null)
        {
            float x = defaultX + offset * Screen.width;
            driveInfoRectTransform.localPosition = new Vector3(x, 0, 0);
        }
    }
}
