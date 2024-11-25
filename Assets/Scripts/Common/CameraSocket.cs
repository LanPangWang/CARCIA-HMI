using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using TMPro; // 引入TextMeshPro命名空间
using System.Net.WebSockets;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Xviewer;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CameraSocket : MonoBehaviour
{
    public static CameraSocket Instance { get; private set; }
    public SimulationWorld world;
    private ClientWebSocket WS; // WebSocket 客户端实例
    private bool isConnecting = false; // 标识是否正在尝试连接
    private List<uint> ImageList = new List<uint> { 0, 100 };
    private bool AvmOpen = false;

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
        string url = Utils.GetCameraUrl(wsUrl, "camera");
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
                Debug.Log("尝试连接 Camera 服务器...");
                //Debug.Log(url);
                await WS.ConnectAsync(new Uri(url), CancellationToken.None); // 连接到指定的WebSocket服务器

                // 连接成功后，启动WebSocket监听循环
                Debug.Log("Camera 链接成功");
                await Task.Factory.StartNew(WebSocketListenLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                Debug.Log("Camera - WebSocket 连接错误: " + ex.Message); // 捕获并输出连接异常
                await Task.Delay(5000); // 等待5秒后重试
            }
        }

        isConnecting = false;
    }

    // 发送消息到WebSocket服务器
    private async Task SendMessageWebSocket(object messageObject)
    {
        if (WS.State == WebSocketState.Open) // 检查WebSocket连接状态是否打开
        {
            // 将对象序列化为JSON字符串
            string message = JsonConvert.SerializeObject(messageObject);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message); // 将消息编码为字节数组
            try
            {
                await WS.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None); // 发送消息
            }
            catch (Exception ex)
            {
                Debug.LogError("Camera Socket发送错误: " + ex.Message); // 捕获并输出发送异常
            }
        }
        else
        {
            Debug.LogWarning("Camera Socket未连接，无法发送消息"); // 输出警告信息
        }
    }

    // WebSocket监听循环，持续接收服务器消息
    private async Task WebSocketListenLoop()
    {
        byte[] receiveBuffer = new byte[4*1600*1600]; // 接收缓冲区 200kb一帧 根据实际情况给吧
        try
        {
            while (WS.State == WebSocketState.Open) // 持续监听，直到WebSocket连接关闭
            {
                WebSocketReceiveResult receiveResult = await WS.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None); // 接收消息
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
                // 将接收的数据解析为 JSON 格式
                Xviewer.Image imagesData = Xviewer.Image.Parser.ParseFrom(receiveBuffer, 0, receiveResult.Count);
                var jsonData = JObject.Parse(imagesData.ToString());
                // 将 base64 字符串转换为 byte[]

                // 解析 JSON 数据中的图片 buffer
                foreach (var item in jsonData)
                {
                    string key = item.Key; // 图片的键值，比如 imageFusion 或 image0
                    if (key == "imageFusion")
                    {
                        string base64String = item.Value.ToString(); // 获取 base64 编码的图片数据
                        StateManager.Instance.SetBevImage(base64String);
                    }
                }

                if (AvmOpen)
                {
                    GetAvmImage();
                }
                else
                {
                    StateManager.Instance.SetBevImage(null);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("WebSocket接收错误: " + ex.Message); // 捕获并输出接收异常
        }
    }

    private async void GetAvmImage()
    {

        var paramsDict = new Dictionary<string, object>
        {
            { "type", "RequestCameraData" },
            { "list", ImageList },
            { "src", "app" },
        };
        await SendMessageWebSocket(paramsDict);
    }

    // 在启动时连接WebSocket服务器并发送初始消息
    private async void Start()
    {
        //await ConnectWebSocket("ws://192.168.8.42:8899/camera");
    }

    // Update is called once per frame
    void Update()
    {
        if (AvmOpen != StateManager.Instance.AvmOpen)
        {
            AvmOpen = StateManager.Instance.AvmOpen;
            if (AvmOpen)
            {
                GetAvmImage();
            }
        }
    }
}
