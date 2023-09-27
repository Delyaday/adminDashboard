using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Foundation.WebSocket
{
    public class WebSocket : IDisposable
    {
        private ClientWebSocket _ws;
        private readonly IWebSocketOptions _options;
        private readonly BlockingCollection<KeyValuePair<DateTime, string>> _sendQueue = new BlockingCollection<KeyValuePair<DateTime, string>>();
        private bool _disposedValue;
        private bool _disconnectCalled;
        private bool _listenerRunning;
        private bool _senderRunning;
        private bool _monitorRunning;
        private bool _reconnecting;
        private bool _reconnectNeeded;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _monitorTask;
        private Task _listenerTask;
        private Task _senderTask;

        public Uri Uri { get; }

        public WebSocketState State => _ws.State;

        public bool IsAlive => State == WebSocketState.Open;

        public int SendQueueLength => _sendQueue.Count;

        public TimeSpan DefaultKeepAliveInterval
        {
            get => _ws.Options.KeepAliveInterval;
            set => _ws.Options.KeepAliveInterval = value;
        }

        public event Data OnData;
        public event Message OnMessage;
        public event StateChanged OnStateChanged;
        public event Opened OnOpened;
        public event Closed OnClosed;
        public event Error OnError;
        public event SendFailed OnSendFailed;
        public event Fatality OnFatality;

        public WebSocket(Uri uri)
            : this(uri, new WebSocketOptions())
        { }

        public WebSocket(Uri uri, IWebSocketOptions options)
        {
            _options = options;
            Uri = uri;

            InitializeClient();
            StartMonitor();
        }

        private void InitializeClient()
        {
            _ws = new ClientWebSocket();

            try
            {
                if (_options.IgnoreCertErrors)
                {
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
                }
            }
            catch (Exception)
            {
                _options.IgnoreCertErrors = false;
            }

            if (_options.Cookies != null && _options.Cookies.Count > 0)
                _ws.Options.Cookies = _options.Cookies;

            if (_options.ClientCertificates != null && _options.ClientCertificates.Count > 0)
                _ws.Options.ClientCertificates = _options.ClientCertificates;

            if (_options.Proxy != null)
                _ws.Options.Proxy = _options.Proxy;

            if (_options.SubProtocols != null)
            {
                foreach (var protocol in _options.SubProtocols)
                {
                    try
                    {
                        _ws.Options.AddSubProtocol(protocol);
                    }
                    catch (Exception)
                    {
                        //TODO log
                    }
                }
            }

            if (_options.Headers != null)
            {
                foreach (var (Key, Value) in _options.Headers)
                {
                    try
                    {
                        _ws.Options.SetRequestHeader(Key, Value);
                    }
                    catch (Exception)
                    {
                        //TODO log
                    }
                }
            }
        }

        public bool Connect()
        {
            try
            {
                _disconnectCalled = false;
                _ws.ConnectAsync(Uri, _tokenSource.Token).Wait(15000);

                StartListener();
                StartSender();

                Task.Run(async () =>
                {
                    while (_ws.State != WebSocketState.Open)
                    {
                        await Task.Delay(1);
                    }
                }).Wait(15000);

                return _ws.State == WebSocketState.Open;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _disconnectCalled = false;
                await _ws.ConnectAsync(Uri, _tokenSource.Token).ConfigureAwait(false);

                StartListener();
                StartSender();

                await Task.Run(async () =>
                {
                    var st = DateTime.Now;

                    while (_ws.State != WebSocketState.Open && (DateTime.UtcNow - st).TotalSeconds < 16)
                    {
                        await Task.Delay(1);
                    }
                });

                return _ws.State == WebSocketState.Open;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }

        public bool Send(string data)
        {
            try
            {
                if (State != WebSocketState.Open && !_reconnecting || SendQueueLength >= _options.SendQueueLimit || _disconnectCalled)
                {
                    //TODO log
                    return false;
                }

                Task.Run(() =>
                {
                    _sendQueue.Add(new KeyValuePair<DateTime, string>(DateTime.UtcNow, data));
                }).Wait(100, _tokenSource.Token);

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }

        public async Task<bool> SendAsync(string data)
        {
            try
            {
                if (State != WebSocketState.Open && !_reconnecting || SendQueueLength >= _options.SendQueueLimit || _disconnectCalled)
                {
                    //TODO log
                    return false;
                }

                await Task.Run(() =>
                {
                    _sendQueue.Add(new KeyValuePair<DateTime, string>(DateTime.UtcNow, data));
                });

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }

        public bool Send(byte[] data, EncodingTypes encodingType = EncodingTypes.UTF8)
        {
            switch (encodingType)
            {
                case EncodingTypes.UTF7:
                    return Send(Encoding.UTF7.GetString(data));
                case EncodingTypes.UTF8:
                    return Send(Encoding.UTF8.GetString(data));
                case EncodingTypes.UTF32:
                    return Send(Encoding.UTF32.GetString(data));
                case EncodingTypes.ASCII:
                    return Send(Encoding.ASCII.GetString(data));
                case EncodingTypes.Unicode:
                    return Send(Encoding.Unicode.GetString(data));
                case EncodingTypes.BigEndianUnicode:
                    return Send(Encoding.BigEndianUnicode.GetString(data));
                case EncodingTypes.Default:
                    return Send(Encoding.Default.GetString(data));
                default:
                    return Send(Encoding.Default.GetString(data));
            }
        }

        public Task<bool> SendAsync(byte[] data, EncodingTypes encodingType = EncodingTypes.UTF8)
        {
            switch (encodingType)
            {
                case EncodingTypes.UTF7:
                    return SendAsync(Encoding.UTF7.GetString(data));
                case EncodingTypes.UTF8:
                    return SendAsync(Encoding.UTF8.GetString(data));
                case EncodingTypes.UTF32:
                    return SendAsync(Encoding.UTF32.GetString(data));
                case EncodingTypes.ASCII:
                    return SendAsync(Encoding.ASCII.GetString(data));
                case EncodingTypes.Unicode:
                    return SendAsync(Encoding.Unicode.GetString(data));
                case EncodingTypes.BigEndianUnicode:
                    return SendAsync(Encoding.BigEndianUnicode.GetString(data));
                case EncodingTypes.Default:
                    return SendAsync(Encoding.Default.GetString(data));
                default:
                    return SendAsync(Encoding.Default.GetString(data));
            }
        }

        private void StartMonitor()
        {
            _monitorTask = Task.Run(async () =>
            {
                _monitorRunning = true;
                _reconnectNeeded = false;

                try
                {
                    var lastState = State;
                    while (_ws != null && !_disposedValue)
                    {
                        if (lastState == State)
                        {
                            await Task.Delay(200);
                            continue;
                        }

                        if (_reconnecting)
                        {
                            if (_options.ReconnectStrategy != null)
                            {
                                await Task.Delay(_options.ReconnectStrategy.GetReconnectInterval() + 1000);
                                if (_reconnecting)
                                {
                                    await Task.Delay(_options.ReconnectStrategy.GetReconnectInterval());
                                    if (!_reconnecting)
                                        return;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }

                        if (lastState == WebSocketState.Aborted && (State == WebSocketState.Connecting || State == WebSocketState.Open))
                            break;

                        if (_reconnectNeeded && State == WebSocketState.Aborted)
                            break;

                        if (lastState == State)
                        {
                            await Task.Delay(200);
                            continue;
                        }

                        OnStateChanged?.Invoke(State, lastState);

                        if (State == WebSocketState.Open)
                            OnOpened?.Invoke();

                        if ((State == WebSocketState.Closed || State == WebSocketState.Aborted) && !_reconnecting)
                        {
                            if (lastState == WebSocketState.Open && !_disconnectCalled && _options.ReconnectStrategy != null && !_options.ReconnectStrategy.AreAttemptsComplete())
                            {
                                _reconnectNeeded = true;
                                break;
                            }

                            OnClosed?.Invoke(_ws.CloseStatus ?? WebSocketCloseStatus.Empty);

                            if (_ws.CloseStatus != null && _ws.CloseStatus != WebSocketCloseStatus.NormalClosure)
                                OnError?.Invoke(new Exception(_ws.CloseStatus + " " + _ws.CloseStatusDescription));
                        }

                        lastState = State;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }

                _monitorRunning = false;

                if (_reconnectNeeded && !_reconnecting && !_disconnectCalled)
                    DoReconnect();
            });
        }

        private void DoReconnect()
        {
            _ = Task.Run(async () =>
            {
                _tokenSource.Cancel();
                _reconnecting = true;

                if (!Task.WaitAll(new[] { _monitorTask, _listenerTask, _senderTask }, 15000))
                {
                    OnFatality?.Invoke("Fatal network error. Network services fail to shut down.");
                    _reconnecting = false;
                    _disconnectCalled = true;
                    _tokenSource.Cancel();
                    return;
                }

                _ws.Dispose();

                OnStateChanged?.Invoke(WebSocketState.Connecting, WebSocketState.Aborted);

                _tokenSource = new CancellationTokenSource();

                var connected = false;
                while (!_disconnectCalled && !_disposedValue && !connected && !_tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        InitializeClient();
                        if (!_monitorRunning)
                        {
                            StartMonitor();
                        }

                        connected = _ws.ConnectAsync(Uri, _tokenSource.Token).Wait(15000);
                    }
                    catch (Exception)
                    {
                        _ws.Dispose();

                        await Task.Delay(_options.ReconnectStrategy.GetReconnectInterval());
                        _options.ReconnectStrategy.Increment();
                        if (_options.ReconnectStrategy.AreAttemptsComplete())
                        {
                            OnFatality?.Invoke("Fatal network error. Max reconnect attempts reached.");
                            _reconnectNeeded = false;
                            _reconnecting = false;
                            _disconnectCalled = true;
                            _tokenSource.Cancel();
                            return;
                        }
                    }
                }

                if (connected)
                {
                    _reconnectNeeded = false;
                    _reconnecting = false;
                    if (!_monitorRunning)
                        StartMonitor();
                    if (!_listenerRunning)
                        StartListener();
                    if (!_senderRunning)
                        StartSender();
                }
                else
                {
                    //TODO log
                }
            });
        }

        private void StartListener()
        {
            _listenerTask = Task.Run(async () =>
            {
                _listenerRunning = true;

                try
                {
                    while (_ws.State == WebSocketState.Open && !_disposedValue && !_reconnecting)
                    {
                        var message = "";
                        var binary = new List<byte>();

READ:

                        var buffer = new byte[1024];
                        WebSocketReceiveResult res;

                        try
                        {
                            res = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _tokenSource.Token);
                        }
                        catch (Exception)
                        {
                            _reconnectNeeded = true;
                            _ws.Abort();
                            break;
                        }

                        if (res == null)
                            goto READ;

                        if (res.MessageType == WebSocketMessageType.Close)
                        {
                            if (_ws.State != WebSocketState.Closed)
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "SERVER REQUESTED CLOSE", CancellationToken.None);
                            _disconnectCalled = true;
                            return Task.CompletedTask;
                        }

                        if (res.MessageType == WebSocketMessageType.Text)
                        {
                            if (!res.EndOfMessage)
                            {
                                message += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                                goto READ;
                            }

                            message += Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                            if (message.Trim() == "ping")
                            {
                                _ = Send("pong");
                            }
                            else
                            {
                                Task.Run(() => OnMessage?.Invoke(message)).Wait(50);
                            }
                        }
                        else
                        {
                            var exactDataBuffer = new byte[res.Count];
                            Array.Copy(buffer, 0, exactDataBuffer, 0, res.Count);

                            if (!res.EndOfMessage)
                            {
                                binary.AddRange(exactDataBuffer);
                                goto READ;
                            }

                            binary.AddRange(exactDataBuffer);
                            var binaryData = binary.ToArray();
                            Task.Run(() => OnData?.Invoke(binaryData)).Wait(50);
                        }

                        buffer = null;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }

                _listenerRunning = false;
                return Task.CompletedTask;
            });
        }

        private void StartSender()
        {
            _senderTask = Task.Run(async () =>
            {
                _senderRunning = true;

                try
                {
                    while (!_disposedValue && !_reconnecting)
                    {
                        if (_ws.State == WebSocketState.Open && !_reconnecting && !_tokenSource.IsCancellationRequested && _sendQueue.Count > 0)
                        {
                            var message = _sendQueue.Take(_tokenSource.Token);
                            if (message.Key.Add(_options.SendCacheItemTimeout) < DateTime.UtcNow)
                            {
                                continue;
                            }

                            var buffer = Encoding.UTF8.GetBytes(message.Value);

                            try
                            {
                                await _ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _tokenSource.Token).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                OnSendFailed?.Invoke(message.Value, ex);
                                _reconnectNeeded = true;
                                _ws.Abort();
                                break;
                            }
                        }

                        Thread.Sleep(_options.SendDelay);
                    }
                }
                catch (Exception ex)
                {
                    OnSendFailed?.Invoke("", ex);
                    OnError?.Invoke(ex);
                }

                _senderRunning = false;
                return Task.CompletedTask;
            });
        }

        public void Disconnect()
        {
            try
            {
                _disconnectCalled = true;
                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "NORMAL SHUTDOWN", CancellationToken.None).Wait(_options.DisconnectWait);
            }
            catch (Exception)
            {
                //TODO log
                //игнорируем
            }
        }

        protected virtual void Dispose(bool disposing, bool waitForSendsToComplete)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_sendQueue.Count > 0 && _senderRunning && waitForSendsToComplete)
                    {
                        var i = 0;
                        while (_sendQueue.Count > 0 && _senderRunning)
                        {
                            i++;
                            Task.Delay(1000).Wait();
                            if (i > 25)
                                break;
                        }
                    }

                    Disconnect();
                    _tokenSource.Cancel();
                    Thread.Sleep(500);
                    _tokenSource.Dispose();
                    _ws.Dispose();
                    GC.Collect();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool waitForSendsToComplete)
        {
            Dispose(true, waitForSendsToComplete);
        }
    }
}
