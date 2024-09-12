using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using TMPro; // 引入TextMeshPro命名空间
using System.Net.WebSockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Xviewer;
using Newtonsoft.Json;

public class HmiSocket : MonoBehaviour
{
    public static HmiSocket Instance { get; private set; }
    public SimulationWorld world;
    private ClientWebSocket WS; // WebSocket 客户端实例
    private bool isConnecting = false; // 标识是否正在尝试连接

    public TMP_InputField wsInputField; // 引用UI中的InputField
    public Button connectButton; // 引用UI中的Button

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 如果需要在场景切换时保持此实例，则使用此选项
        }
        else
        {
            Destroy(gameObject);
        }

        // 为按钮点击事件添加监听器
        connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    // 当用户点击连接按钮时触发
    private async void OnConnectButtonClicked()
    {
        string wsUrl = wsInputField.text; // 获取用户输入的WebSocket地址
        string url = Utils.GetUrl(wsUrl, "hmisocket");
        if (!string.IsNullOrEmpty(wsUrl))
        {
            await ConnectWebSocket(url);
        }
        else
        {
            Debug.LogWarning("WebSocket地址不能为空");
        }
    }

    // 连接WebSocket服务器
    private async Task ConnectWebSocket(string url)
    {
        if (isConnecting) return; // 如果已经在尝试连接，则返回
        isConnecting = true;
        while (WS == null || WS.State != WebSocketState.Open) // 持续尝试连接直到成功
        {
            if (WS != null)
            {
                WS.Dispose(); // 确保旧的WebSocket实例被正确处理
            }

            WS = new ClientWebSocket(); // 初始化WebSocket实例
            try
            {
                Debug.Log("尝试连接 Hmi - WebSocket 服务器...");
                //Debug.Log(url);
                await WS.ConnectAsync(new Uri(url), CancellationToken.None); // 连接到指定的WebSocket服务器

                // 连接成功后，启动WebSocket监听循环
                Debug.Log("Hmi - WebSocket 连接成功");
            }
            catch (Exception ex)
            {
                Debug.Log("Hmi - WebSocket 连接错误: " + ex.Message); // 捕获并输出连接异常
                await Task.Delay(5000); // 等待5秒后重试
            }
        }

        isConnecting = false;
    }

    // 发送消息到WebSocket服务器
    public async Task Send(object messageObject)
    {
        if (WS.State == WebSocketState.Open) // 检查WebSocket连接状态是否打开
        {
            // 将对象序列化为JSON字符串
            string message = JsonConvert.SerializeObject(messageObject);
            Debug.Log(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // 将消息编码为字节数组
            try
            {
                await WS.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None); // 发送消息
            }
            catch (Exception ex)
            {
                Debug.LogError("HmiSocket发送错误: " + ex.Message); // 捕获并输出发送异常
            }
        }
        else
        {
            Debug.LogWarning("HmiSocket未连接，无法发送消息"); // 输出警告信息
        }
    }
}
