using UnityEngine;

public class AvmLayout : MonoBehaviour
{
    public GameObject AvmLayer;
    public GameObject MainCamera;

    private bool Open = false;
    private postProcessTAA postProcess;
    private float targetOffset = -0.5f;
    private float animationDuration = 0.3f; // 动画时长为 0.3 秒
    private float animationTime = 0f;
    private bool isAnimating = false;
    private float initialOffset;

    void Start()
    {
        RectTransform rectTransform = AvmLayer.GetComponent<RectTransform>();

        // 设置宽度为屏幕的一半
        rectTransform.sizeDelta = new Vector2(Screen.width / 2, 0);

        // 将 Panel 的位置设置为右半边
        rectTransform.anchoredPosition = new Vector2(Screen.width / 4, 0);

        // 锚点设置为右侧
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(1, 1);

        postProcess = MainCamera.GetComponent<postProcessTAA>();
    }

    void Update()
    {
        if (!Open && StateManager.Instance.AvmOpen)
        {
            Open = true;
            AvmLayer.SetActive(true);
            StartAnimation();
        }

        if (Open && !StateManager.Instance.AvmOpen)
        {
            Open = false;
            postProcess._AvmDevideOffset = 0f;
            AvmLayer.SetActive(false);
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
        postProcess._AvmDevideOffset = Mathf.Lerp(initialOffset, targetOffset, smoothStep);

        if (t >= 1f)
        {
            isAnimating = false; // 动画完成
        }
    }
}