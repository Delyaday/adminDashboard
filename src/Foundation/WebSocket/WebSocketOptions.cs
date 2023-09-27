using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Foundation.WebSocket
{
    public class WebSocketOptions : IWebSocketOptions
    {
        public IEnumerable<(string Key, string Value)> Headers { get; set; }
        public IEnumerable<string> SubProtocols { get; set; }
        public IWebProxy Proxy { get; set; }
        public int SendQueueLimit { get; set; } = 10000;
        public TimeSpan SendCacheItemTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public ushort SendDelay { get; set; } = 80;
        public ReconnectStrategy ReconnectStrategy { get; set; } = new ReconnectStrategy();
        public bool DebugMode { get; set; }
        public bool IgnoreCertErrors { get; set; }
        public CookieContainer Cookies { get; set; }
        public X509CertificateCollection ClientCertificates { get; set; }
        public int DisconnectWait { get; set; } = 20000;
    }
}
