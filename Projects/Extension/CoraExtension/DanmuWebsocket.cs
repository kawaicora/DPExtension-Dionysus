using System;
using Newtonsoft.Json.Linq;
using PatcherYRpp;
using DynamicPatcher;
using System.Threading;
using System.Collections;
using Extension.Coroutines;
using System.Threading.Tasks;

namespace Extension.CoraExtension {
    // ===================== 弹幕WebSocket客户端封装 =====================
    public class DouyinDanmuWebSocket
    {
        private const int MaxReconnectAttempts = 999999;
        private const int InitialReconnectDelayMs = 1000;
        private const int MaxReconnectDelayMs = 3000;
        public delegate void OnLikeHandler(string userName, string count, string total);
        public event OnLikeHandler OnLike;
        public delegate void OnGiftHandler(string userName, string giftName, string count);
        public event OnGiftHandler OnGift;
        public delegate void OnChatHandler(string userName, string content);
        public event OnChatHandler OnChat;
        public delegate void JoinRoomHandler(string userName);
        public event JoinRoomHandler JoinRoom;
        private WebSocketClient _webSocketClient;
        private int _reconnectAttempts;
        private bool _isManualDisconnect;
        private CancellationTokenSource _cancellationTokenSource;
        private static DouyinDanmuWebSocket _instance = null;
        public bool isRun = false;
        private static CoroutineSystem _coroutineSystem = new CoroutineSystem();
        private string url;
        public static DouyinDanmuWebSocket instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DouyinDanmuWebSocket();
                }
                return _instance;
            }
        }
        public void SetUrl(string url)
        {
            this.url = url;
        }

        private DouyinDanmuWebSocket()
        {
            // 测试弹幕协程
            // _coroutineSystem.StartCoroutine(TestGiftCoroutine("小心心"));
            // _coroutineSystem.StartCoroutine(TestGiftCoroutine("抖音"));
            // _coroutineSystem.StartCoroutine(TestGiftCoroutine("助力票"));
            // _coroutineSystem.StartCoroutine(TestGiftCoroutine("人气票"));
            // _coroutineSystem.StartCoroutine(TestChatCoroutine("666"));
            // _coroutineSystem.StartCoroutine(TestChatCoroutine("6"));
        }
        /// <summary>
        /// 在MOD主循环中调用Update来驱动WebSocket连接和消息处理
        /// </summary>
        public void Update()
        {
            _coroutineSystem.Update();
        }
        /// <summary>
        /// 测试礼物发送协程
        /// </summary>
        private IEnumerator TestGiftCoroutine(string giftName)
        {
            Random random = new Random();
            while (true)
            {
                OnGift?.Invoke("热心测试员", giftName, random.Next(100).ToString());
                yield return new WaitForFrames((int)Game.CurrentFrameRate * (int)random.Next(5,10));  // 等待下一帧再继续执行下一个礼物
            }
        }

        /// <summary>
        /// 测试聊天发送协程
        /// </summary>
        private IEnumerator TestChatCoroutine(string content)
        {
            Random random = new Random();
            while (true)
            {
                OnChat?.Invoke("热心测试员", content);
                yield return new WaitForFrames((int)Game.CurrentFrameRate * (int)random.Next(5,10)); 
            }
        }

        public async Task Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _isManualDisconnect = false;
            _reconnectAttempts = 0;
            isRun = true;
            InitializeWebSocketClient();

            try
            {
                await ConnectWithRetry();
                await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.Log("程序已取消");
            }
            finally
            {
                await CleanupConnection();
            }
        }

        private void InitializeWebSocketClient()
        {
            _webSocketClient = new WebSocketClient(url);
            _webSocketClient.OnConnectionStatusChanged += OnConnectionStatusChanged;
            _webSocketClient.OnTextMessageReceived += OnTextMessageReceived;
            // _webSocketClient.OnBinaryMessageReceived += (data) =>
            // {
            //     Logger.Log($"收到二进制消息，长度: {data.Length} 字节");
            // };
            _webSocketClient.OnCloseMessageReceived += (status, description) =>
            {
                Logger.Log($"收到关闭消息: {status} - {description}");
            };
            _webSocketClient.OnErrorOccurred += (ex) =>
            {
                Logger.Log($"WebSocket错误: {ex.Message}");
                
            };
        }

        private async void OnConnectionStatusChanged(bool isConnected)
        {
            if (isConnected)
            {
                Logger.Log("连接成功");
                _reconnectAttempts = 0;
            }
            else if (!_isManualDisconnect)
            {
                Logger.Log("连接已断开，准备重连...");
                await ScheduleReconnect();
            }
        }

        private async Task ScheduleReconnect()
        {
            if (_reconnectAttempts >= MaxReconnectAttempts)
            {
                Logger.Log($"已达到最大重连次数({MaxReconnectAttempts})，停止重试");
                return;
            }

            int delay = (int)Math.Min(
                InitialReconnectDelayMs * Math.Pow(2, _reconnectAttempts),
                MaxReconnectDelayMs
            );

            _reconnectAttempts++;
            Logger.Log($"第 {_reconnectAttempts} 次重连将在 {delay}ms 后尝试...");

            await Task.Delay(delay, _cancellationTokenSource.Token);

            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await ConnectWithRetry();
            }
        }

        private async Task ConnectWithRetry()
        {
            try
            {
                if (_webSocketClient == null)
                {
                    InitializeWebSocketClient();
                }

                Logger.Log("尝试连接到WebSocket服务器...");
                await _webSocketClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                _ = ex;
                Logger.Log($"连接异常");
                Logger.PrintException(ex);
            }
        }

        private async void OnTextMessageReceived(string message)
        {
            try
            {
                JObject jsonObj = JObject.Parse(message);
                var method = jsonObj["common"]?["method"]?.Value<string>();

                if (string.IsNullOrEmpty(method))
                {
                    // Logger.Log("收到的消息不包含method字段");
                    return;
                }

                if (method == "WebcastRoomDataSyncMessage")
                {
                    return;
                }

                var roomId = jsonObj["common"]?["room_id"]?.Value<string>();
                var user = jsonObj["user"];

                switch (method)
                {
                    case "WebcastChatMessage":
                        var content = jsonObj["content"]?.Value<string>();
                        if (!string.IsNullOrEmpty(content) && user != null)
                        {
                            OnChat?.Invoke($"{user["nick_name"]}", content);
                        }
                        else
                        {
                            Logger.Log("弹幕消息缺少内容或用户信息");
                        }
                        break;
                    case "WebcastMemberMessage":
                        
                       if (user != null)
                        {
                            JoinRoom?.Invoke($"{user["nickName"]}");
                        }
                        else
                        {
                            Logger.Log("弹幕消息缺少用户信息");
                        }
                        break;
                    case "WebcastLikeMessage":
                        if (user != null)
                        {
                            var count = jsonObj["count"]?.Value<string>() ?? "1";
                            var total = jsonObj["total"]?.Value<string>() ?? "0";
                            OnLike?.Invoke($"{user["nickName"]}", count, total);
                        }
                        else
                        {
                            Logger.Log("点赞消息缺少用户信息");
                        }
                        break;

                    case "WebcastGiftMessage":
                        var describe = jsonObj["common"]?["describe"]?.Value<string>();
                        var gift = jsonObj["gift"];

                        if (user != null && gift != null)
                        {
                            var giftName = gift["name"]?.Value<string>();
                            var repeat_count = jsonObj["repeat_count"]?.Value<string>() ?? "0";
                            var combo_count = jsonObj["combo_count"]?.Value<string>() ?? "1";
                            var count = 0.ToString();
                            if (int.Parse(repeat_count) > int.Parse(combo_count))
                            {
                                count = repeat_count;
                            }
                            else
                            {
                                count = combo_count;
                            }

                            if (!string.IsNullOrEmpty(describe))
                            {
                                Logger.Log(describe);
                            }
                            else
                            {
                                Logger.Log("礼物消息缺少描述信息");
                            }
                            if (!string.IsNullOrEmpty(giftName))
                            {
                                OnGift?.Invoke($"{user["nickName"]}", giftName, count);
                            }
                            else
                            {
                                Logger.Log("礼物消息缺少礼物名字");
                            }
                        }
                        else
                        {
                            Logger.Log("礼物消息缺少用户信息或礼物信息");
                        }
                        break;

                    default:
                        await _webSocketClient.SendStringAsync($"{{\"type\":\"{method}\",\"message\":\"收到未处理的消息类型\"}}");
                        break;
                }
                
            }
            catch (Exception ex)
            {
                _ = ex;
                Logger.Log($"解析消息时出错");
                Logger.Log($"recv:      {message}");
                Logger.PrintException(ex);
            }
        }

        private async Task CleanupConnection()
        {
            _isManualDisconnect = true;

            if (_webSocketClient != null)
            {
                _webSocketClient.Disconnect();
                _webSocketClient = null;
            }

            _cancellationTokenSource?.Dispose();
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }

}