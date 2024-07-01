// SimpleSonarShader 脚本和着色器由 Drew Okenfuss 编写

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarCircle : MonoBehaviour
{
    // 所有需要将声纳数据发送到其着色器的渲染器。
    private Renderer[] ObjectRenderers;

    // 开始时设置位置的占位值。
    private static readonly Vector4 GarbagePosition = new Vector4(-5000, -5000, -5000, -5000);

    // 一次可以渲染的环数。
    // 必须与着色器中的数组大小相同。
    private static int QueueSize = 20;

    // 声纳环起始位置的队列。
    // xyz 值保存位置的 xyz。
    // w 值保存位置的开始时间。
    private static Queue<Vector4> positionsQueue = new Queue<Vector4>(QueueSize);

    // 每个环的强度值队列。
    // 这些与 positionsQueue 中的顺序保持一致。
    private static Queue<float> intensityQueue = new Queue<float>(QueueSize);

    // 确保只有一个对象初始化队列。
    private static bool NeedToInitQueues = true;

    // 将为每个对象调用 SendSonarData。
    private delegate void Delegate();
    private static Delegate RingDelegate;

    // 声纳开关
    public bool SonarActive = false;

    private void Start()
    {
        // 获取将应用效果的渲染器
        ObjectRenderers = GetComponentsInChildren<Renderer>();

        if (NeedToInitQueues)
        {
            NeedToInitQueues = false;
            // 用起始值填充队列，这些是占位值
            for (int i = 0; i < QueueSize; i++)
            {
                positionsQueue.Enqueue(GarbagePosition);
                intensityQueue.Enqueue(-5000f);
            }
        }

        // 将此对象的函数添加到静态委托
        RingDelegate += SendSonarData;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetSonarActive(!this.SonarActive);
        }
    }

    /// <summary>
    /// 从此位置以给定的强度开始声纳环。
    /// </summary>
    public void StartSonarRing(Vector4 position, float intensity)
    {
        // 将值放入队列
        position.w = Time.timeSinceLevelLoad;
        positionsQueue.Dequeue();
        positionsQueue.Enqueue(position);

        intensityQueue.Dequeue();
        intensityQueue.Enqueue(intensity);

        RingDelegate();
    }

    /// <summary>
    /// 将声纳数据发送到着色器。
    /// </summary>
    private void SendSonarData()
    {
        if (!SonarActive)
        {
            // 清除着色器数据
            foreach (Renderer r in ObjectRenderers)
            {
                r.material.SetVectorArray("_hitPts", new Vector4[QueueSize]);
                r.material.SetFloatArray("_Intensity", new float[QueueSize]);
            }
            return;
        }

        // 将更新后的队列发送到着色器
        foreach (Renderer r in ObjectRenderers)
        {
            r.material.SetVectorArray("_hitPts", positionsQueue.ToArray());
            r.material.SetFloatArray("_Intensity", intensityQueue.ToArray());
        }
    }

    private void OnDestroy()
    {
        RingDelegate -= SendSonarData;
    }

    /// <summary>
    /// 触发声纳环
    /// </summary>
    private void TriggerSonarRing()
    {
        if (SonarActive)
        {
            Vector3 position = transform.position; // 获取对象的当前位置
            StartSonarRing(new Vector4(position.x, position.y, position.z, 0), 0.6f); // 以强度 1.0f 触发声纳环
        }
    }

    // 公开方法来控制声纳开关
    public void SetSonarActive(bool isActive)
    {
        SonarActive = isActive;
        // 如果关闭声纳，立即清除着色器数据
        if (!SonarActive)
        {
            CancelInvoke("TriggerSonarRing"); // 停止定时触发
            ClearSonarData();
        } else
        {
            TriggerSonarRing(); // 立即触发一次
            InvokeRepeating("TriggerSonarRing", 1f, 1f);
        }
    }

    /// <summary>
    /// 清除声纳数据
    /// </summary>
    private void ClearSonarData()
    {
        // 清空队列
        positionsQueue.Clear();
        intensityQueue.Clear();
        // 用占位值填充队列
        for (int i = 0; i < QueueSize; i++)
        {
            positionsQueue.Enqueue(GarbagePosition);
            intensityQueue.Enqueue(-5000f);
        }
        // 更新着色器数据
        SendSonarData();
    }
}
