using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicPatcher;
namespace Extension.CoraExtension
{
    public class WebSocketClient : IDisposable
    {
        // WebSocket 核心对象
        private ClientWebSocket _webSocket;


        public delegate void TextMessageReceivedHandler(string message);
        public event TextMessageReceivedHandler OnTextMessageReceived;
        public delegate void CloseMessageReceivedHandler(WebSocketCloseStatus closeStatus, string statusDescription);
        public event CloseMessageReceivedHandler OnCloseMessageReceived;

        public delegate void ConnectionStatusChangedHandler(bool isConnected);
        public event ConnectionStatusChangedHandler OnConnectionStatusChanged;

        public delegate void ErrorOccurredHandler(Exception ex);
        public event ErrorOccurredHandler OnErrorOccurred;


        // 取消令牌（用于终止异步操作）
        private CancellationTokenSource _cts;

        private string _url;
        private Dictionary<string, Action<string>> _handlers;
        private bool _isConnected;
        private bool debug;
        public bool IsConnected => _isConnected && _webSocket != null && _webSocket.State == WebSocketState.Open;

        public WebSocketClient(string url,bool debug = false)
        {
            this._url = url;
            _handlers = new Dictionary<string, Action<string>>();
        }

        public async Task ConnectAsync()
        {
            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open) return;
                Dispose();
            }

            _webSocket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            try
            {
                Logger.Log($"[WebSocketClient] Connecting to {_url}...");
                await _webSocket.ConnectAsync(new Uri(_url), _cts.Token);
                _isConnected = true;
                Logger.Log($"[WebSocketClient] Connected to {_url}");

                _ = ReceiveLoop();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[WebSocketClient] Connection Error: {ex.Message}");
                _isConnected = false;
            }
        }

        public async void Disconnect()
        {
            if (_webSocket != null)
            {
                _cts?.Cancel();
                if (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnect", CancellationToken.None);
                    }
                    catch { }
                }
                _webSocket.Dispose();
                _webSocket = null;
            }
            _isConnected = false;
            OnConnectionStatusChanged?.Invoke(_isConnected);
            Logger.Log("[WebSocketClient] Disconnected");
        }



        public async Task SendStringAsync(string message)
        {
            if (debug)
            {
                 Logger.Log("client -----> server");
                Logger.Log(message);
            }
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                // OnErrorOccurred?.Invoke(ex);
                // OnConnectionStatusChanged?.Invoke(false);
                CheckConnection();
                Logger.LogError($"[WebSocketClient] Send Error: {ex.Message}");
            }
        }

        private void CheckConnection()
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                _isConnected = false;
                OnConnectionStatusChanged?.Invoke(_isConnected);
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[8192];
            while (_webSocket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result;
                    var ms = new System.IO.MemoryStream();
                    do
                    {
                        result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using (var reader = new System.IO.StreamReader(ms,Encoding.UTF8))
                        {
                            string message = await reader.ReadToEndAsync();
                            if (debug)
                            {
                                Logger.Log("client <----- server");
                                Logger.Log(message);
                            }
                            
                            
                            try{HandleMessage(message);}catch (Exception ex) {Logger.PrintException(ex);}
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        OnCloseMessageReceived?.Invoke(result.CloseStatus ?? WebSocketCloseStatus.Empty, 
                                result.CloseStatusDescription);
                        CheckConnection();
                        break;
                    }
                }
                catch (OperationCanceledException ex)
                {
                    CheckConnection();
                    Logger.LogError($"[WebSocketClient] Receive Error: {ex.Message}");
                    Logger.PrintException(ex);
                    
                    // break;
                }
                catch (Exception ex)
                {
                    CheckConnection();
                    Logger.LogError($"[WebSocketClient] Receive Error: {ex.Message}");
                    Logger.PrintException(ex);
                    // OnErrorOccurred?.Invoke(ex);
                    
                    // break;
                }
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket != null)
            {
                try { await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None); } catch { }
                _webSocket.Dispose();
            }
            _isConnected = false;
            
        }

        private void HandleMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

           
            OnTextMessageReceived?.Invoke(msg);
        }

        public void Dispose()
        {
            Disconnect();
            _cts?.Dispose();
        }
    }

    
}