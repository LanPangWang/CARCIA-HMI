using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using TMPro; // 引入TextMeshPro命名空间
using System.Net.WebSockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Xviewer;

public class WebSocketNet : MonoBehaviour
{
    public static WebSocketNet Instance { get; private set; }
    public SimulationWorld world;
    public TrajectoryPoint center = new TrajectoryPoint();
    public float yaw = 0;
    private ClientWebSocket WS; // WebSocket 客户端实例
    private bool isConnecting = false; // 标识是否正在尝试连接

    public TMP_InputField wsInputField; // 引用UI中的InputField
    public Button connectButton; // 引用UI中的Button
    public GameObject panel;

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
        string url = Utils.GetUrl(wsUrl, "websocket");
        if (!string.IsNullOrEmpty(wsUrl))
        {
            panel.SetActive(false);
            await ConnectWebSocket(url);
        }
        else
        {
            Debug.LogWarning("WebSocket地址不能为空");
        }
    }

    // 在启动时连接WebSocket服务器并发送初始消息
    private async void Start()
    {
        await ConnectWebSocket("ws://192.168.8.71:8888/websocket");
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
                Debug.Log("尝试连接WebSocket服务器...");
                //Debug.Log(url);
                await WS.ConnectAsync(new Uri(url), CancellationToken.None); // 连接到指定的WebSocket服务器

                // 连接成功后，启动WebSocket监听循环
                Debug.Log("WebSocket连接成功");
                await Task.Factory.StartNew(WebSocketListenLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                // 发送初始消息
                await SendMessageWebSocket("{\"type\":\"RequestSimulationWorld\",\"planning\":false}");
            }
            catch (Exception ex)
            {
                Debug.Log("WebSocket连接错误: " + ex.Message); // 捕获并输出连接异常
                await Task.Delay(5000); // 等待5秒后重试
            }
        }

        isConnecting = false;
    }

    // 发送消息到WebSocket服务器
    private async Task SendMessageWebSocket(string message)
    {
        if (WS.State == WebSocketState.Open) // 检查WebSocket连接状态是否打开
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // 将消息编码为字节数组
            try
            {
                await WS.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None); // 发送消息
            }
            catch (Exception ex)
            {
                Debug.LogError("WebSocket发送错误: " + ex.Message); // 捕获并输出发送异常
            }
        }
        else
        {
            Debug.LogWarning("WebSocket未连接，无法发送消息"); // 输出警告信息
        }
    }

    // WebSocket监听循环，持续接收服务器消息
    private async Task WebSocketListenLoop()
    {
        byte[] receiveBuffer = new byte[2048000]; // 接收缓冲区 200kb一帧 根据实际情况给吧
        try
        {
            while (WS.State == WebSocketState.Open) // 持续监听，直到WebSocket连接关闭
            {
                WebSocketReceiveResult receiveResult = await WS.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None); // 接收消息
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
                world = SimulationWorld.Parser.ParseFrom(receiveBuffer, 0, receiveResult.Count);
                (center, yaw) = WorldUtils.GetWorldCenter(world);
                //Debug.Log(yaw);
                await SendMessageWebSocket("{\"type\":\"RequestSimulationWorld\",\"planning\":false}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("WebSocket接收错误: " + ex.Message); // 捕获并输出接收异常
            await SendMessageWebSocket("{\"type\":\"RequestSimulationWorld\",\"planning\":false}");
        }
    }

    // 当对象被销毁时关闭WebSocket连接
    private async void OnDestroy()
    {
        try
        {
            if (WS != null && WS.State == WebSocketState.Open) // 检查WebSocket实例和连接状态
            {
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket closed", CancellationToken.None); // 关闭WebSocket连接
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("WebSocket关闭错误: " + ex.Message); // 捕获并输出关闭异常
        }
        finally
        {
            if (WS != null)
            {
                WS.Dispose(); // 确保WebSocket实例被正确释放
                WS = null;
            }
        }
    }
}
