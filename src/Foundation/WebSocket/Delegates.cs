using System;
using System.Net.WebSockets;

namespace Foundation.WebSocket
{
    public delegate void Message(string message);
    public delegate void Data(byte[] data);
    public delegate void StateChanged(WebSocketState newState, WebSocketState oldState);
    public delegate void Opened();
    public delegate void Closed(WebSocketCloseStatus reason);
    public delegate void Error(Exception ex);
    public delegate void SendFailed(string data, Exception ex);
    public delegate void Fatality(string reason);
}
